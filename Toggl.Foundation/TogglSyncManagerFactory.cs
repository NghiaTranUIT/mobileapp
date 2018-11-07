using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.CleanUp;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Foundation.Sync.States.Push;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.ApiClients;
using Toggl.Ultrawave.ApiClients.Interfaces;

namespace Toggl.Foundation
{
    public static class TogglSyncManager
    {
        public static ISyncManager CreateSyncManager(
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            ILastTimeUsageStorage lastTimeUsageStorage,
            TimeSpan? retryLimit,
            IScheduler scheduler)
        {
            var random = new Random();
            var queue = new SyncStateQueue();
            var entryPoints = new StateMachineEntryPoints();
            var transitions = new TransitionHandlerProvider();
            var apiDelay = new RetryDelayService(random, retryLimit);
            var delayCancellation = new Subject<Unit>();
            var delayCancellationObservable = delayCancellation.AsObservable().Replay();
            ConfigureTransitions(transitions, database, api, dataSource, apiDelay, scheduler, timeService, analyticsService, entryPoints, delayCancellationObservable, queue);
            var stateMachine = new StateMachine(transitions, scheduler, delayCancellation);
            var orchestrator = new StateMachineOrchestrator(stateMachine, entryPoints);

            return new SyncManager(queue, orchestrator, analyticsService, lastTimeUsageStorage, timeService);
        }

        public static void ConfigureTransitions(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            StateMachineEntryPoints entryPoints,
            IObservable<Unit> delayCancellation,
            ISyncStateQueue queue)
        {
            configurePullTransitions(transitions, database, api, dataSource, timeService, analyticsService, scheduler, entryPoints.StartPullSync, delayCancellation, queue);
            configurePushTransitions(transitions, api, dataSource, analyticsService, apiDelay, scheduler, entryPoints.StartPushSync, delayCancellation);
            configureCleanUpTransitions(transitions, timeService, dataSource, entryPoints.StartCleanUp);
        }

        private static void configurePullTransitions(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation,
            ISyncStateQueue queue)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var fetchAllSince = new FetchAllSinceState(database, api, timeService);

            var ensureFetchWorkspacesSucceeded = new EnsureFetchListSucceededState<IWorkspace>();
            var ensureFetchWorkspaceFeaturesSucceeded = new EnsureFetchListSucceededState<IWorkspaceFeatureCollection>();
            var ensureFetchTagsSucceeded = new EnsureFetchListSucceededState<ITag>();
            var ensureFetchClientsSucceeded = new EnsureFetchListSucceededState<IClient>();
            var ensureFetchProjectsSucceeded = new EnsureFetchListSucceededState<IProject>();
            var ensureFetchTasksSucceeded = new EnsureFetchListSucceededState<ITask>();
            var ensureFetchTimeEntriesSucceeded = new EnsureFetchListSucceededState<ITimeEntry>();
            var ensureFetchUserSucceeded = new EnsureFetchSingletonSucceededState<IUser>();
            var ensureFetchPreferencesSucceeded = new EnsureFetchSingletonSucceededState<IPreferences>();

            var scheduleCleanUp = new ScheduleCleanUpState(queue);

            var detectGainingAccessToWorkspaces =
                new DetectGainingAccessToWorkspacesState(
                    dataSource.Workspaces,
                    analyticsService,
                    () => new HasFinsihedSyncBeforeInteractor(dataSource));

            var resetSinceParams = new ResetSinceParamsState(database.SinceParameters);

            var persistNewWorkspaces =
                new PersistNewWorkspacesState(dataSource.Workspaces);

            var detectLosingAccessToWorkspaces =
                new DetectLosingAccessToWorkspacesState(dataSource.Workspaces, analyticsService);

            var deleteRunningInaccessibleTimeEntry = new DeleteInaccessibleRunningTimeEntryState(dataSource.TimeEntries);

            var markWorkspacesAsInaccessible = new MarkWorkspacesAsInaccessibleState(dataSource.Workspaces);

            var persistWorkspaces =
                new PersistListState<IWorkspace, IDatabaseWorkspace, IThreadSafeWorkspace>(dataSource.Workspaces, Workspace.Clean);

            var updateWorkspacesSinceDate =
                new SinceDateUpdatingState<IWorkspace, IDatabaseWorkspace>(database.SinceParameters);

            var detectNoWorkspaceState = new NoWorkspaceDetectingState(dataSource);

            var persistWorkspaceFeatures =
                new PersistListState<IWorkspaceFeatureCollection, IDatabaseWorkspaceFeatureCollection, IThreadSafeWorkspaceFeatureCollection>(
                    dataSource.WorkspaceFeatures, WorkspaceFeatureCollection.From);

            var persistUser =
                new PersistSingletonState<IUser, IDatabaseUser, IThreadSafeUser>(dataSource.User, User.Clean);

            var noDefaultWorkspaceDetectingState = new NoDefaultWorkspaceDetectingState(dataSource, analyticsService);

            var trySetDefaultWorkspaceState = new TrySetDefaultWorkspaceState(timeService, dataSource);

            var persistTags =
                new PersistListState<ITag, IDatabaseTag, IThreadSafeTag>(dataSource.Tags, Tag.Clean);

            var updateTagsSinceDate = new SinceDateUpdatingState<ITag, IDatabaseTag>(database.SinceParameters);

            var persistClients =
                new PersistListState<IClient, IDatabaseClient, IThreadSafeClient>(dataSource.Clients, Client.Clean);

            var updateClientsSinceDate = new SinceDateUpdatingState<IClient, IDatabaseClient>(database.SinceParameters);

            var persistPreferences =
                new PersistSingletonState<IPreferences, IDatabasePreferences, IThreadSafePreferences>(dataSource.Preferences, Preferences.Clean);

            var persistProjects =
                new PersistListState<IProject, IDatabaseProject, IThreadSafeProject>(dataSource.Projects, Project.Clean);

            var updateProjectsSinceDate = new SinceDateUpdatingState<IProject, IDatabaseProject>(database.SinceParameters);

            var createProjectPlaceholders = new CreateArchivedProjectPlaceholdersState(dataSource.Projects, analyticsService);

            var persistTimeEntries =
                new PersistListState<ITimeEntry, IDatabaseTimeEntry, IThreadSafeTimeEntry>(dataSource.TimeEntries, TimeEntry.Clean);

            var updateTimeEntriesSinceDate = new SinceDateUpdatingState<ITimeEntry, IDatabaseTimeEntry>(database.SinceParameters);

            var persistTasks =
                new PersistListState<ITask, IDatabaseTask, IThreadSafeTask>(dataSource.Tasks, Task.Clean);

            var updateTasksSinceDate = new SinceDateUpdatingState<ITask, IDatabaseTask>(database.SinceParameters);

            var refetchInaccessibleProjects =
                new TryFetchInaccessibleProjectsState(dataSource.Projects, timeService, api.Projects);

            var retryOrThrow = new SevereApiExceptionsRethrowingState();
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);

            var finished = new ResetAPIDelayState(apiDelay);

            // start all the API requests first
            transitions.ConfigureTransition(entryPoint, fetchAllSince);

            // detect gaining access to workspaces
            transitions.ConfigureTransition(fetchAllSince.FetchStarted, ensureFetchWorkspacesSucceeded);
            transitions.ConfigureTransition(ensureFetchWorkspacesSucceeded.Continue, detectGainingAccessToWorkspaces);
            transitions.ConfigureTransition(detectGainingAccessToWorkspaces.Continue, detectLosingAccessToWorkspaces);
            transitions.ConfigureTransition(detectGainingAccessToWorkspaces.NewWorkspacesDetected, resetSinceParams);
            transitions.ConfigureTransition(resetSinceParams.Continue, persistNewWorkspaces);
            transitions.ConfigureTransition(persistNewWorkspaces.FinishedPersisting, fetchAllSince);

            // detect losing access to workspaces
            transitions.ConfigureTransition(detectLosingAccessToWorkspaces.Continue, persistWorkspaces);
            transitions.ConfigureTransition(detectLosingAccessToWorkspaces.WorkspaceAccessLost, markWorkspacesAsInaccessible);
            transitions.ConfigureTransition(markWorkspacesAsInaccessible.Continue, scheduleCleanUp);
            transitions.ConfigureTransition(scheduleCleanUp.CleanUpScheduled, deleteRunningInaccessibleTimeEntry);
            transitions.ConfigureTransition(deleteRunningInaccessibleTimeEntry.Continue, persistWorkspaces);

            // persist all the data pulled from the server
            transitions.ConfigureTransition(persistWorkspaces.FinishedPersisting, updateWorkspacesSinceDate);
            transitions.ConfigureTransition(updateWorkspacesSinceDate.Finished, detectNoWorkspaceState);
            transitions.ConfigureTransition(detectNoWorkspaceState.Continue, ensureFetchUserSucceeded);

            transitions.ConfigureTransition(ensureFetchUserSucceeded.Continue, persistUser);
            transitions.ConfigureTransition(persistUser.FinishedPersisting, ensureFetchWorkspaceFeaturesSucceeded);

            transitions.ConfigureTransition(ensureFetchWorkspaceFeaturesSucceeded.Continue, persistWorkspaceFeatures);
            transitions.ConfigureTransition(persistWorkspaceFeatures.FinishedPersisting, ensureFetchPreferencesSucceeded);

            transitions.ConfigureTransition(ensureFetchPreferencesSucceeded.Continue, persistPreferences);
            transitions.ConfigureTransition(persistPreferences.FinishedPersisting, ensureFetchTagsSucceeded);

            transitions.ConfigureTransition(ensureFetchTagsSucceeded.Continue, persistTags);
            transitions.ConfigureTransition(persistTags.FinishedPersisting, updateTagsSinceDate);
            transitions.ConfigureTransition(updateTagsSinceDate.Finished, ensureFetchClientsSucceeded);

            transitions.ConfigureTransition(ensureFetchClientsSucceeded.Continue, persistClients);
            transitions.ConfigureTransition(persistClients.FinishedPersisting, updateClientsSinceDate);
            transitions.ConfigureTransition(updateClientsSinceDate.Finished, ensureFetchProjectsSucceeded);

            transitions.ConfigureTransition(ensureFetchProjectsSucceeded.Continue, persistProjects);
            transitions.ConfigureTransition(persistProjects.FinishedPersisting, updateProjectsSinceDate);
            transitions.ConfigureTransition(updateProjectsSinceDate.Finished, ensureFetchTasksSucceeded);

            transitions.ConfigureTransition(ensureFetchTasksSucceeded.Continue, persistTasks);
            transitions.ConfigureTransition(persistTasks.FinishedPersisting, updateTasksSinceDate);
            transitions.ConfigureTransition(updateTasksSinceDate.Finished, ensureFetchTimeEntriesSucceeded);

            transitions.ConfigureTransition(ensureFetchTimeEntriesSucceeded.Continue, createProjectPlaceholders);
            transitions.ConfigureTransition(createProjectPlaceholders.FinishedPersisting, persistTimeEntries);
            transitions.ConfigureTransition(persistTimeEntries.FinishedPersisting, updateTimeEntriesSinceDate);
            transitions.ConfigureTransition(updateTimeEntriesSinceDate.Finished, refetchInaccessibleProjects);
            transitions.ConfigureTransition(refetchInaccessibleProjects.FetchNext, refetchInaccessibleProjects);

            transitions.ConfigureTransition(refetchInaccessibleProjects.FinishedPersisting, noDefaultWorkspaceDetectingState);
            transitions.ConfigureTransition(noDefaultWorkspaceDetectingState.NoDefaultWorkspaceDetected, trySetDefaultWorkspaceState);

            // process server errors
            transitions.ConfigureTransition(ensureFetchWorkspacesSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchUserSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchWorkspaceFeaturesSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchPreferencesSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchTagsSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchClientsSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchProjectsSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchTasksSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(ensureFetchTimeEntriesSucceeded.ErrorOccured, retryOrThrow);
            transitions.ConfigureTransition(refetchInaccessibleProjects.ErrorOccured, retryOrThrow);

            // retry loop
            transitions.ConfigureTransition(retryOrThrow.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, finished);
            transitions.ConfigureTransition(finished.Continue, fetchAllSince);
        }

        private static void configurePushTransitions(
            ITransitionConfigurator transitions,
            ITogglApi api,
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var pushingWorkspacesFinished = configureCreateOnlyPush(transitions, entryPoint, dataSource.Workspaces, analyticsService, api.Workspaces, Workspace.Clean, Workspace.Unsyncable, api, scheduler, delayCancellation);
            var pushingUsersFinished = configurePushSingleton(transitions, pushingWorkspacesFinished, dataSource.User, analyticsService, api.User, User.Clean, User.Unsyncable, api, scheduler, delayCancellation);
            var pushingPreferencesFinished = configurePushSingleton(transitions, pushingUsersFinished, dataSource.Preferences, analyticsService, api.Preferences, Preferences.Clean, Preferences.Unsyncable, api, scheduler, delayCancellation);
            var pushingTagsFinished = configureCreateOnlyPush(transitions, pushingPreferencesFinished, dataSource.Tags, analyticsService, api.Tags, Tag.Clean, Tag.Unsyncable, api, scheduler, delayCancellation);
            var pushingClientsFinished = configureCreateOnlyPush(transitions, pushingTagsFinished, dataSource.Clients, analyticsService, api.Clients, Client.Clean, Client.Unsyncable, api, scheduler, delayCancellation);
            var pushingProjectsFinished = configureCreateOnlyPush(transitions, pushingClientsFinished, dataSource.Projects, analyticsService, api.Projects, Project.Clean, Project.Unsyncable, api, scheduler, delayCancellation);
            configurePush(transitions, pushingProjectsFinished, dataSource.TimeEntries, analyticsService, api.TimeEntries, api.TimeEntries, api.TimeEntries, TimeEntry.Clean, TimeEntry.Unsyncable, api, apiDelay, scheduler, delayCancellation);
        }

        private static void configureCleanUpTransitions(
            ITransitionConfigurator transitions,
            ITimeService timeService,
            ITogglDataSource dataSource,
            StateResult entryPoint)
        {
            var deleteOlderEntries = new DeleteOldEntriesState(timeService, dataSource.TimeEntries);
            var deleteUnsnecessaryProjectPlaceholders = new DeleteUnnecessaryProjectPlaceholdersState(dataSource.Projects, dataSource.TimeEntries);

            var deleteInaccessibleTimeEntries = new DeleteInaccessibleTimeEntriesState(dataSource.TimeEntries);
            var deleteInaccessibleTags = new DeleteNonReferencedInaccessibleTagsState(dataSource.Tags, dataSource.TimeEntries);
            var deleteInaccessibleTasks = new DeleteNonReferencedInaccessibleTasksState(dataSource.Tasks, dataSource.TimeEntries);
            var deleteInaccessibleProjects = new DeleteNonReferencedInaccessibleProjectsState(dataSource.Projects, dataSource.Tasks, dataSource.TimeEntries);
            var deleteInaccessibleClients = new DeleteNonReferencedInaccessibleClientsState(dataSource.Clients, dataSource.Projects);
            var deleteInaccessibleWorkspaces = new DeleteNonReferencedInaccessibleWorkspacesState(
                dataSource.Workspaces,
                dataSource.TimeEntries,
                dataSource.Projects,
                dataSource.Tasks,
                dataSource.Clients,
                dataSource.Tags);

            transitions.ConfigureTransition(entryPoint, deleteOlderEntries);
            transitions.ConfigureTransition(deleteOlderEntries.FinishedDeleting, deleteUnsnecessaryProjectPlaceholders);

            transitions.ConfigureTransition(deleteUnsnecessaryProjectPlaceholders.FinishedDeleting, deleteInaccessibleTimeEntries);
            transitions.ConfigureTransition(deleteInaccessibleTimeEntries.FinishedDeleting, deleteInaccessibleTags);
            transitions.ConfigureTransition(deleteInaccessibleTags.FinishedDeleting, deleteInaccessibleTasks);
            transitions.ConfigureTransition(deleteInaccessibleTasks.FinishedDeleting, deleteInaccessibleProjects);
            transitions.ConfigureTransition(deleteInaccessibleProjects.FinishedDeleting, deleteInaccessibleClients);
            transitions.ConfigureTransition(deleteInaccessibleClients.FinishedDeleting, deleteInaccessibleWorkspaces);
        }

        private static IStateResult configurePush<TModel, TDatabase, TThreadsafe>(
            ITransitionConfigurator transitions,
            IStateResult entryPoint,
            IDataSource<TThreadsafe, TDatabase> dataSource,
            IAnalyticsService analyticsService,
            ICreatingApiClient<TModel> creatingApi,
            IUpdatingApiClient<TModel> updatingApi,
            IDeletingApiClient<TModel> deletingApi,
            Func<TModel, TThreadsafe> toClean,
            Func<TThreadsafe, string, TThreadsafe> toUnsyncable,
            ITogglApi api,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            IObservable<Unit> delayCancellation)
            where TModel : class, IIdentifiable, ILastChangedDatable
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadSafeModel
        {
            var rnd = new Random();
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<TDatabase, TThreadsafe>(dataSource);
            var pushOne = new PushOneEntityState<TThreadsafe>();
            var create = new CreateEntityState<TModel, TDatabase, TThreadsafe>(creatingApi, dataSource, analyticsService, toClean);
            var update = new UpdateEntityState<TModel, TThreadsafe>(updatingApi, dataSource, analyticsService, toClean);
            var delete = new DeleteEntityState<TModel, TDatabase, TThreadsafe>(deletingApi, analyticsService, dataSource);
            var deleteLocal = new DeleteLocalEntityState<TDatabase, TThreadsafe>(dataSource);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TThreadsafe>(dataSource, toUnsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, push);
            transitions.ConfigureTransition(push.PushEntity, pushOne);
            transitions.ConfigureTransition(pushOne.CreateEntity, create);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update);
            transitions.ConfigureTransition(pushOne.DeleteEntity, delete);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, deleteLocal);

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(update.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(delete.ClientError, tryResolveClientError);

            transitions.ConfigureTransition(create.ServerError, checkServerStatus);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus);
            transitions.ConfigureTransition(delete.ServerError, checkServerStatus);

            transitions.ConfigureTransition(create.UnknownError, checkServerStatus);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus);
            transitions.ConfigureTransition(delete.UnknownError, checkServerStatus);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, unsyncable);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push);

            transitions.ConfigureTransition(create.EntityChanged, push);
            transitions.ConfigureTransition(update.EntityChanged, push);

            transitions.ConfigureTransition(create.Finished, finished);
            transitions.ConfigureTransition(update.Finished, finished);
            transitions.ConfigureTransition(delete.DeletingFinished, finished);
            transitions.ConfigureTransition(deleteLocal.Deleted, finished);
            transitions.ConfigureTransition(deleteLocal.DeletingFailed, finished);
            transitions.ConfigureTransition(unsyncable.MarkedAsUnsyncable, finished);

            transitions.ConfigureTransition(finished.Continue, push);

            return push.NothingToPush;
        }

        private static IStateResult configureCreateOnlyPush<TModel, TDatabase, TThreadsafe>(
            ITransitionConfigurator transitions,
            IStateResult entryPoint,
            IDataSource<TThreadsafe, TDatabase> dataSource,
            IAnalyticsService analyticsService,
            ICreatingApiClient<TModel> creatingApi,
            Func<TModel, TThreadsafe> toClean,
            Func<TThreadsafe, string, TThreadsafe> toUnsyncable,
            ITogglApi api,
            IScheduler scheduler,
            IObservable<Unit> delayCancellation)
            where TModel : IIdentifiable, ILastChangedDatable
            where TDatabase : class, TModel, IDatabaseSyncable
            where TThreadsafe : class, TDatabase, IThreadSafeModel
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushState<TDatabase, TThreadsafe>(dataSource);
            var pushOne = new PushOneEntityState<TThreadsafe>();
            var create = new CreateEntityState<TModel, TDatabase, TThreadsafe>(creatingApi, dataSource, analyticsService, toClean);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TThreadsafe>(dataSource, toUnsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, push);
            transitions.ConfigureTransition(push.PushEntity, pushOne);
            transitions.ConfigureTransition(pushOne.CreateEntity, create);

            transitions.ConfigureTransition(pushOne.UpdateEntity, new InvalidTransitionState($"Updating is not supported for {typeof(TModel).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(TModel).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(TModel).Name} during Push sync."));

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(create.ServerError, checkServerStatus);
            transitions.ConfigureTransition(create.UnknownError, checkServerStatus);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, unsyncable);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push);

            transitions.ConfigureTransition(create.EntityChanged, new InvalidTransitionState($"Entity cannot have changed since updating is not supported for {typeof(TModel).Name} during Push sync."));
            transitions.ConfigureTransition(create.Finished, finished);
            transitions.ConfigureTransition(unsyncable.MarkedAsUnsyncable, finished);

            transitions.ConfigureTransition(finished.Continue, push);

            return push.NothingToPush;
        }

        private static IStateResult configurePushSingleton<TModel, TThreadsafe>(
            ITransitionConfigurator transitions,
            IStateResult entryPoint,
            ISingletonDataSource<TThreadsafe> dataSource,
            IAnalyticsService analyticsService,
            IUpdatingApiClient<TModel> updatingApi,
            Func<TModel, TThreadsafe> toClean,
            Func<TThreadsafe, string, TThreadsafe> toUnsyncable,
            ITogglApi api,
            IScheduler scheduler,
            IObservable<Unit> delayCancellation)
            where TModel : class
            where TThreadsafe : class, TModel, IThreadSafeModel, IDatabaseSyncable, IIdentifiable
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushSingleState<TThreadsafe>(dataSource);
            var pushOne = new PushOneEntityState<TThreadsafe>();
            var update = new UpdateEntityState<TModel, TThreadsafe>(updatingApi, dataSource, analyticsService, toClean);
            var tryResolveClientError = new TryResolveClientErrorState<TThreadsafe>();
            var unsyncable = new UnsyncableEntityState<TThreadsafe>(dataSource, toUnsyncable);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, push);
            transitions.ConfigureTransition(push.PushEntity, pushOne);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update);

            transitions.ConfigureTransition(pushOne.CreateEntity, new InvalidTransitionState($"Creating is not supported for {typeof(TModel).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(TModel).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(TModel).Name} during Push sync."));

            transitions.ConfigureTransition(update.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, unsyncable);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push);

            transitions.ConfigureTransition(update.Finished, finished);
            transitions.ConfigureTransition(unsyncable.MarkedAsUnsyncable, finished);
            transitions.ConfigureTransition(update.EntityChanged, finished);

            transitions.ConfigureTransition(finished.Continue, push);

            return push.NothingToPush;
        }
    }
}
