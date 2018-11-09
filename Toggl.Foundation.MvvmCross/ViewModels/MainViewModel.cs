using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Diagnostics;
using Toggl.Foundation.Experiments;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Extensions;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.MvvmCross.ViewModels.Reports;
using Toggl.Foundation.Services;
using Toggl.Foundation.Suggestions;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;

[assembly: MvxNavigation(typeof(MainViewModel), ApplicationUrls.Main.Regex)]
namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class MainViewModel : MvxViewModel
    {
        // Outputs
        public ObservableGroupedOrderedCollection<TimeEntryViewModel> TimeEntries => TimeEntriesViewModel.TimeEntries;
        public IObservable<bool> LogEmpty { get; }
        public IObservable<int> TimeEntriesCount { get; }
        public IObservable<SyncProgress> SyncProgressState { get; private set; }
        public IObservable<bool> ShouldShowEmptyState { get; private set; }
        public IObservable<bool> ShouldShowWelcomeBack { get; private set; }
        public IObservable<IThreadSafeTimeEntry> CurrentRunningTimeEntry { get; private set; }
        public IObservable<bool> ShouldShowRunningTimeEntryNotification { get; private set; }
        public IObservable<bool> ShouldShowStoppedTimeEntryNotification { get; private set; }
        public IObservable<Unit> ShouldReloadTimeEntryLog { get; private set; }

        public TimeSpan CurrentTimeEntryElapsedTime { get; private set; } = TimeSpan.Zero;
        public DurationFormat CurrentTimeEntryElapsedTimeFormat { get; } = DurationFormat.Improved;
        public long? CurrentTimeEntryId { get; private set; }
        public string CurrentTimeEntryDescription { get; private set; }
        public string CurrentTimeEntryProject { get; private set; }
        public string CurrentTimeEntryProjectColor { get; private set; }
        public string CurrentTimeEntryTask { get; private set; }
        public string CurrentTimeEntryClient { get; private set; }

        public IObservable<bool> CurrentTimeEntryHasDescription { get; private set; }

        public IObservable<bool> IsTimeEntryRunning { get; private set; }

        public bool IsAddDescriptionLabelVisible =>
            string.IsNullOrEmpty(CurrentTimeEntryDescription)
            && string.IsNullOrEmpty(CurrentTimeEntryProject);


        public int NumberOfSyncFailures { get; private set; }
        public bool IsInManualMode { get; set; } = false;

        public SuggestionsViewModel SuggestionsViewModel { get; }
        public RatingViewModel RatingViewModel { get; }
        public IOnboardingStorage OnboardingStorage => onboardingStorage;

        public new IMvxNavigationService NavigationService => navigationService;

        public IMvxAsyncCommand StartTimeEntryCommand { get; }
        public IMvxAsyncCommand AlternativeStartTimeEntryCommand { get; }
        public IMvxAsyncCommand<TimeEntryStopOrigin> StopTimeEntryCommand { get; }
        public IMvxAsyncCommand OpenSettingsCommand { get; }
        public IMvxAsyncCommand OpenReportsCommand { get; }
        public IMvxAsyncCommand OpenSyncFailuresCommand { get; }
        public IMvxCommand ToggleManualMode { get; }

        // Inputs
        public UIAction Refresh { get; }
        public InputAction<TimeEntryViewModel> DeleteTimeEntry { get; }
        public InputAction<TimeEntryViewModel> SelectTimeEntry { get; }
        public InputAction<TimeEntryViewModel> ContinueTimeEntry { get; }

        // Private
        private const int ratingViewTimeout = 5;

        public ITimeService TimeService { get; }
        public ISchedulerProvider SchedulerProvider { get; }

        private readonly ITogglDataSource dataSource;
        private readonly IUserPreferences userPreferences;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly IIntentDonationService intentDonationService;
        private readonly IAccessRestrictionStorage accessRestrictionStorage;
        private readonly IStopwatchProvider stopwatchProvider;

        private CompositeDisposable disposeBag = new CompositeDisposable();

        private RatingViewExperiment ratingViewExperiment;

        private bool isStopButtonEnabled = false;
        private string urlNavigationAction;
        private bool hasStopButtonEverBeenUsed;
        private bool isEditViewOpen = false;
        private object isEditViewOpenLock = new object();

        private bool noWorkspaceViewPresented;
        private bool noDefaultWorkspaceViewPresented;

        private DateTimeOffset? currentTimeEntryStart;

        public TimeEntriesViewModel TimeEntriesViewModel { get; }

        // Deprecated properties
        [Obsolete("Use SelectTimeEntry RxAction instead")]
        public IMvxAsyncCommand EditTimeEntryCommand { get; }

        public MainViewModel(
            ITogglDataSource dataSource,
            ITimeService timeService,
            IRatingService ratingService,
            IUserPreferences userPreferences,
            IAnalyticsService analyticsService,
            IOnboardingStorage onboardingStorage,
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService,
            IRemoteConfigService remoteConfigService,
            ISuggestionProviderContainer suggestionProviders,
            IIntentDonationService intentDonationService,
            IAccessRestrictionStorage accessRestrictionStorage,
            ISchedulerProvider schedulerProvider,
            IStopwatchProvider stopwatchProvider)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(ratingService, nameof(ratingService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(remoteConfigService, nameof(remoteConfigService));
            Ensure.Argument.IsNotNull(suggestionProviders, nameof(suggestionProviders));
            Ensure.Argument.IsNotNull(intentDonationService, nameof(intentDonationService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(accessRestrictionStorage, nameof(accessRestrictionStorage));
            Ensure.Argument.IsNotNull(stopwatchProvider, nameof(stopwatchProvider));

            this.dataSource = dataSource;
            this.userPreferences = userPreferences;
            this.analyticsService = analyticsService;
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;
            this.onboardingStorage = onboardingStorage;
            this.SchedulerProvider = schedulerProvider;
            this.intentDonationService = intentDonationService;
            this.accessRestrictionStorage = accessRestrictionStorage;
            this.stopwatchProvider = stopwatchProvider;

            TimeService = timeService;

            SuggestionsViewModel = new SuggestionsViewModel(dataSource, interactorFactory, onboardingStorage, suggestionProviders, schedulerProvider);
            RatingViewModel = new RatingViewModel(timeService, dataSource, ratingService, analyticsService, onboardingStorage, navigationService, SchedulerProvider);
            TimeEntriesViewModel = new TimeEntriesViewModel(dataSource, interactorFactory, analyticsService, SchedulerProvider);

            LogEmpty = TimeEntriesViewModel.Empty.AsDriver(SchedulerProvider);
            TimeEntriesCount = TimeEntriesViewModel.Count.AsDriver(SchedulerProvider);

            ratingViewExperiment = new RatingViewExperiment(timeService, dataSource, onboardingStorage, remoteConfigService);

            OpenSettingsCommand = new MvxAsyncCommand(openSettings);
            OpenReportsCommand = new MvxAsyncCommand(openReports);
            OpenSyncFailuresCommand = new MvxAsyncCommand(openSyncFailures);
            EditTimeEntryCommand = new MvxAsyncCommand(editTimeEntry, canExecuteEditTimeEntryCommand);
            StopTimeEntryCommand = new MvxAsyncCommand<TimeEntryStopOrigin>(stopTimeEntry, _ => isStopButtonEnabled);
            StartTimeEntryCommand = new MvxAsyncCommand(startTimeEntry, () => CurrentTimeEntryId.HasValue == false);
            AlternativeStartTimeEntryCommand = new MvxAsyncCommand(alternativeStartTimeEntry, () => CurrentTimeEntryId.HasValue == false);

            Refresh = UIAction.FromAsync(refresh);
            DeleteTimeEntry = InputAction<TimeEntryViewModel>.FromObservable(deleteTimeEntry);
            SelectTimeEntry = InputAction<TimeEntryViewModel>.FromAsync(timeEntrySelected);
            ContinueTimeEntry = InputAction<TimeEntryViewModel>.FromObservable(continueTimeEntry);
        }

        public void Init(string action, string description)
        {
            urlNavigationAction = action;

            if (description != null)
            {
                interactorFactory
                    .GetDefaultWorkspace()
                    .Execute()
                    .SelectMany(workspace => interactorFactory
                        .CreateTimeEntry(description.AsTimeEntryPrototype(TimeService.CurrentDateTime, workspace.Id))
                        .Execute())
                    .Subscribe()
                    .DisposedBy(disposeBag);
            }
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await TimeEntriesViewModel.Initialize();
            await SuggestionsViewModel.Initialize();
            await RatingViewModel.Initialize();

            SyncProgressState = dataSource
                .SyncManager
                .ProgressObservable
                .AsDriver(SchedulerProvider);

            var isWelcome = onboardingStorage.IsNewUser;

            var noTimeEntries = Observable
                .CombineLatest(TimeEntriesViewModel.Empty, SuggestionsViewModel.IsEmpty,
                    (isTimeEntryEmpty, isSuggestionEmpty) => isTimeEntryEmpty && isSuggestionEmpty)
                .DistinctUntilChanged();

            ShouldShowEmptyState = ObservableAddons.CombineLatestAll(
                    isWelcome,
                    noTimeEntries
                )
                .DistinctUntilChanged()
                .AsDriver(SchedulerProvider);

            ShouldShowWelcomeBack = ObservableAddons.CombineLatestAll(
                    isWelcome.Select(b => !b),
                    noTimeEntries
                )
                .StartWith(false)
                .DistinctUntilChanged()
                .AsDriver(SchedulerProvider);

            ShouldShowRunningTimeEntryNotification = userPreferences.AreRunningTimerNotificationsEnabledObservable;
            ShouldShowStoppedTimeEntryNotification = userPreferences.AreStoppedTimerNotificationsEnabledObservable;

            CurrentRunningTimeEntry = dataSource
                .TimeEntries
                .CurrentlyRunningTimeEntry
                .AsDriver(SchedulerProvider);

            CurrentTimeEntryHasDescription = CurrentRunningTimeEntry
                .Select(te => !string.IsNullOrWhiteSpace(te?.Description))
                .DistinctUntilChanged();

            IsTimeEntryRunning = CurrentRunningTimeEntry
                .Select(te => te != null)
                .DistinctUntilChanged();

            TimeService
                .CurrentDateTimeObservable
                .Where(_ => currentTimeEntryStart != null)
                .Subscribe(currentTime => CurrentTimeEntryElapsedTime = currentTime - currentTimeEntryStart.Value)
                .DisposedBy(disposeBag);

            interactorFactory
                .GetItemsThatFailedToSync()
                .Execute()
                .Select(i => i.Count())
                .Subscribe(n => NumberOfSyncFailures = n)
                .DisposedBy(disposeBag);

            ShouldReloadTimeEntryLog = Observable.Merge(
                TimeService.MidnightObservable.SelectUnit(),
                TimeService.SignificantTimeChangeObservable.SelectUnit())
                .AsDriver(SchedulerProvider);

            switch (urlNavigationAction)
            {
                case ApplicationUrls.Main.Action.Continue:
                    await continueMostRecentEntry();
                    break;

                case ApplicationUrls.Main.Action.Stop:
                    await stopTimeEntry(TimeEntryStopOrigin.Deeplink);
                    break;
            }

            ratingViewExperiment
                .RatingViewShouldBeVisible
                .Subscribe(presentRatingViewIfNeeded)
                .DisposedBy(disposeBag);

            onboardingStorage.StopButtonWasTappedBefore
                             .Subscribe(hasBeen => hasStopButtonEverBeenUsed = hasBeen)
                             .DisposedBy(disposeBag);

            dataSource
                .TimeEntries
                .CurrentlyRunningTimeEntry
                .Subscribe(setRunningEntry)
                .DisposedBy(disposeBag);

            interactorFactory
                .GetDefaultWorkspace()
                .Execute()
                .Subscribe(intentDonationService.SetDefaultShortcutSuggestions)
                .DisposedBy(disposeBag);

            dataSource
                .Workspaces
                .Created
                .VoidSubscribe(onWorkspaceCreated)
                .DisposedBy(disposeBag);

            dataSource
                .Workspaces
                .Updated
                .Subscribe(onWorkspaceUpdated)
                .DisposedBy(disposeBag);
        }

        private async void onWorkspaceCreated()
        {
            await TimeEntriesViewModel.ReloadData();
        }

        private async void onWorkspaceUpdated(EntityUpdate<IThreadSafeWorkspace> update)
        {
            var workspace = update.Entity;
            if (workspace == null) return;

            if (workspace.IsInaccessible)
            {
                await TimeEntriesViewModel.ReloadData();
            }
        }

        public void Track(ITrackableEvent e)
        {
            analyticsService.Track(e);
        }

        private void presentRatingViewIfNeeded(bool shouldBevisible)
        {
            if (!shouldBevisible) return;

            var wasShownMoreThanOnce = onboardingStorage.NumberOfTimesRatingViewWasShown() > 1;
            if (wasShownMoreThanOnce) return;

            var lastOutcome = onboardingStorage.RatingViewOutcome();
            if (lastOutcome != null)
            {
                var thereIsInteractionFormLastTime = lastOutcome != RatingViewOutcome.NoInteraction;
                if (thereIsInteractionFormLastTime) return;
            }

            var lastOutcomeTime = onboardingStorage.RatingViewOutcomeTime();
            if (lastOutcomeTime != null)
            {
                var oneDayHasNotPassedSinceLastTime = lastOutcomeTime + TimeSpan.FromHours(24) > TimeService.CurrentDateTime;
                if (oneDayHasNotPassedSinceLastTime && !wasShownMoreThanOnce) return;
            }

            navigationService.ChangePresentation(ToggleRatingViewVisibilityHint.Show());
            analyticsService.RatingViewWasShown.Track();
            onboardingStorage.SetDidShowRatingView();
            onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.NoInteraction, TimeService.CurrentDateTime);
            TimeService.RunAfterDelay(TimeSpan.FromMinutes(ratingViewTimeout), () =>
            {
                navigationService.ChangePresentation(ToggleRatingViewVisibilityHint.Hide());
            });
        }

        private async Task continueMostRecentEntry()
        {
            await interactorFactory.ContinueMostRecentTimeEntry().Execute();
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            IsInManualMode = userPreferences.IsManualModeEnabled;
            handleNoWorkspaceState()
                .ContinueWith(_ => handleNoDefaultWorkspaceState());
        }

        private async Task handleNoWorkspaceState()
        {
            if (accessRestrictionStorage.HasNoWorkspace() && !noWorkspaceViewPresented)
            {
                noWorkspaceViewPresented = true;
                await navigationService.Navigate<NoWorkspaceViewModel, Unit>();
                noWorkspaceViewPresented = false;
            }
        }

        private async Task handleNoDefaultWorkspaceState()
        {
            if (accessRestrictionStorage.HasNoDefaultWorkspace() && !noDefaultWorkspaceViewPresented)
            {
                noDefaultWorkspaceViewPresented = true;
                await navigationService.Navigate<SelectDefaultWorkspaceViewModel, Unit>();
                noDefaultWorkspaceViewPresented = false;
            }
        }

        private void setRunningEntry(IThreadSafeTimeEntry timeEntry)
        {
            CurrentTimeEntryId = timeEntry?.Id;
            currentTimeEntryStart = timeEntry?.Start;
            CurrentTimeEntryDescription = timeEntry?.Description ?? "";
            CurrentTimeEntryElapsedTime = TimeService.CurrentDateTime - currentTimeEntryStart ?? TimeSpan.Zero;

            CurrentTimeEntryTask = timeEntry?.Task?.Name ?? "";
            CurrentTimeEntryProject = timeEntry?.Project?.DisplayName() ?? "";
            CurrentTimeEntryProjectColor = timeEntry?.Project?.DisplayColor() ?? "";
            CurrentTimeEntryClient = timeEntry?.Project?.Client?.Name ?? "";

            isStopButtonEnabled = timeEntry != null;

            StopTimeEntryCommand.RaiseCanExecuteChanged();
            StartTimeEntryCommand.RaiseCanExecuteChanged();
            EditTimeEntryCommand.RaiseCanExecuteChanged();

            RaisePropertyChanged(nameof(IsTimeEntryRunning));
        }

        private Task openSettings()
        {
            var settingsStopwatch = stopwatchProvider.CreateAndStore(MeasuredOperation.OpenSettingsView);
            settingsStopwatch.Start();
            return navigate<SettingsViewModel>();
        }

        private Task openReports()
        {
            var openReportsStopwatch = stopwatchProvider.CreateAndStore(MeasuredOperation.OpenReportsFromGiskard);
            openReportsStopwatch.Start();
            return navigate<ReportsViewModel>();
        }

        private Task openSyncFailures()
            => navigate<SyncFailuresViewModel>();

        private Task startTimeEntry()
            => startTimeEntry(IsInManualMode);

        private Task alternativeStartTimeEntry()
            => startTimeEntry(!IsInManualMode);

        private Task startTimeEntry(bool initializeInManualMode)
        {
            OnboardingStorage.StartButtonWasTapped();
            var startTimeEntryStopwatch = stopwatchProvider.CreateAndStore(MeasuredOperation.OpenStartView);
            startTimeEntryStopwatch.Start();

            if (hasStopButtonEverBeenUsed)
                onboardingStorage.SetNavigatedAwayFromMainViewAfterStopButton();

            var parameter = initializeInManualMode
                ? StartTimeEntryParameters.ForManualMode(TimeService.CurrentDateTime)
                : StartTimeEntryParameters.ForTimerMode(TimeService.CurrentDateTime);

            return navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(parameter);
        }

        private IObservable<Unit> continueTimeEntry(TimeEntryViewModel timeEntry)
        {
            return interactorFactory
                .ContinueTimeEntry(timeEntry)
                .Execute()
                .Do( _ => onboardingStorage.SetTimeEntryContinued())
                .SelectUnit();
        }

        private async Task timeEntrySelected(TimeEntryViewModel timeEntry)
        {
            if (isEditViewOpen)
                return;

            onboardingStorage.TimeEntryWasTapped();

            var editTimeEntryStopwatch = stopwatchProvider.CreateAndStore(MeasuredOperation.EditTimeEntryFromMainLog);
            editTimeEntryStopwatch.Start();

            await navigate<EditTimeEntryViewModel, long>(timeEntry.Id);
        }

        private async Task refresh()
        {
            await dataSource.SyncManager.ForceFullSync();
        }

        private IObservable<Unit> deleteTimeEntry(TimeEntryViewModel timeEntry)
        {
            return interactorFactory
                .DeleteTimeEntry(timeEntry.Id)
                .Execute()
                .Do( _ => {
                    analyticsService.DeleteTimeEntry.Track();
                    dataSource.SyncManager.PushSync();
                });
        }

        private async Task stopTimeEntry(TimeEntryStopOrigin origin)
        {
            OnboardingStorage.StopButtonWasTapped();

            isStopButtonEnabled = false;
            StopTimeEntryCommand.RaiseCanExecuteChanged();

            await interactorFactory
                .StopTimeEntry(TimeService.CurrentDateTime, origin)
                .Execute()
                .Do(_ => intentDonationService.DonateStopCurrentTimeEntry())
                .Do(dataSource.SyncManager.InitiatePushSync);

            CurrentTimeEntryElapsedTime = TimeSpan.Zero;
        }

        private async Task editTimeEntry()
        {
            lock (isEditViewOpenLock)
            {
                isEditViewOpen = true;
            }

            var editTimeEntryStopwatch = stopwatchProvider.CreateAndStore(MeasuredOperation.EditTimeEntryFromMainLog);
            editTimeEntryStopwatch.Start();

            await navigate<EditTimeEntryViewModel, long>(CurrentTimeEntryId.Value);

            lock (isEditViewOpenLock)
            {
                isEditViewOpen = false;
            }
        }

        private bool canExecuteEditTimeEntryCommand()
        {
            lock (isEditViewOpenLock)
            {
                if (isEditViewOpen)
                    return false;
            }

            return CurrentTimeEntryId.HasValue;
        }

        private Task navigate<TModel, TParameters>(TParameters value)
            where TModel : IMvxViewModel<TParameters>
        {
            if (hasStopButtonEverBeenUsed)
                onboardingStorage.SetNavigatedAwayFromMainViewAfterStopButton();

            return navigationService.Navigate<TModel, TParameters>(value);
        }

        private Task navigate<TModel>()
            where TModel : IMvxViewModel
        {
            if (hasStopButtonEverBeenUsed)
                onboardingStorage.SetNavigatedAwayFromMainViewAfterStopButton();

            return navigationService.Navigate<TModel>();
        }
    }
}
