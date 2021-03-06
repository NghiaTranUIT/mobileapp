﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Diagnostics;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using Toggl.Foundation.Services;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Network;
using static Toggl.Multivac.Extensions.CommonFunctions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    using WorkspaceToSelectableWorkspaceLambda = Func<IEnumerable<IThreadSafeWorkspace>, IList<SelectableWorkspaceViewModel>>;

    [Preserve(AllMembers = true)]
    public sealed class SettingsViewModel : MvxViewModel
    {
        private readonly ISubject<Unit> loggingOutSubject = new Subject<Unit>();
        private readonly ISubject<bool> isFeedbackSuccessViewShowing = new Subject<bool>();
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly UserAgent userAgent;
        private readonly IMailService mailService;
        private readonly ITogglDataSource dataSource;
        private readonly IDialogService dialogService;
        private readonly IUserPreferences userPreferences;
        private readonly IFeedbackService feedbackService;
        private readonly IAnalyticsService analyticsService;
        private readonly IPlatformConstants platformConstants;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly IPrivateSharedStorageService privateSharedStorageService;
        private readonly IIntentDonationService intentDonationService;
        private readonly IStopwatchProvider stopwatchProvider;

        private bool isSyncing;
        private bool isLoggingOut;
        private IThreadSafeUser currentUser;
        private IThreadSafePreferences currentPreferences;
        private IStopwatch navigationFromMainViewModelStopwatch;

        public string Title { get; private set; } = Resources.Settings;

        public IObservable<string> Name { get; }

        public IObservable<string> Email { get; }

        public IObservable<bool> IsSynced { get; }

        public IObservable<Unit> LoggingOut { get; }

        public IObservable<byte[]> UserAvatar { get; }

        public IObservable<string> DateFormat { get; }

        public IObservable<bool> IsRunningSync { get; }

        public IObservable<string> WorkspaceName { get; }

        public IObservable<string> DurationFormat { get; }

        public IObservable<string> BeginningOfWeek { get; }

        public IObservable<bool> IsManualModeEnabled { get; }

        public IObservable<bool> AreRunningTimerNotificationsEnabled { get; }

        public IObservable<bool> AreStoppedTimerNotificationsEnabled { get; }

        public IObservable<bool> UseTwentyFourHourFormat { get; }

        public IObservable<IList<SelectableWorkspaceViewModel>> Workspaces { get; }

        public IObservable<bool> IsFeedbackSuccessViewShowing { get; }

        public bool CalendarSettingsEnabled => onboardingStorage.CompletedCalendarOnboarding();

        public string Version => $"{userAgent.Version} ({platformConstants.BuildNumber})";

        public UIAction OpenCalendarSettings { get; }

        public UIAction OpenNotificationSettings { get; }

        public UIAction ToggleTwentyFourHourSettings { get; }

        public SettingsViewModel(
            UserAgent userAgent,
            IMailService mailService,
            ITogglDataSource dataSource,
            IDialogService dialogService,
            IUserPreferences userPreferences,
            IFeedbackService feedbackService,
            IAnalyticsService analyticsService,
            IInteractorFactory interactorFactory,
            IPlatformConstants platformConstants,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IPrivateSharedStorageService privateSharedStorageService,
            IIntentDonationService intentDonationService,
            IStopwatchProvider stopwatchProvider)
        {
            Ensure.Argument.IsNotNull(userAgent, nameof(userAgent));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(mailService, nameof(mailService));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(feedbackService, nameof(feedbackService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(platformConstants, nameof(platformConstants));
            Ensure.Argument.IsNotNull(privateSharedStorageService, nameof(privateSharedStorageService));
            Ensure.Argument.IsNotNull(intentDonationService, nameof(intentDonationService));
            Ensure.Argument.IsNotNull(stopwatchProvider, nameof(stopwatchProvider));

            this.userAgent = userAgent;
            this.dataSource = dataSource;
            this.mailService = mailService;
            this.dialogService = dialogService;
            this.userPreferences = userPreferences;
            this.feedbackService = feedbackService;
            this.analyticsService = analyticsService;
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;
            this.platformConstants = platformConstants;
            this.onboardingStorage = onboardingStorage;
            this.privateSharedStorageService = privateSharedStorageService;
            this.intentDonationService = intentDonationService;
            this.stopwatchProvider = stopwatchProvider;

            IsSynced = dataSource.SyncManager.ProgressObservable.SelectMany(checkSynced);

            IsRunningSync =
                dataSource.SyncManager
                    .ProgressObservable
                    .Select(isRunningSync);

            Name =
                dataSource.User.Current
                    .Select(user => user.Fullname)
                    .DistinctUntilChanged();

            Email =
                dataSource.User.Current
                    .Select(user => user.Email.ToString())
                    .DistinctUntilChanged();

            IsManualModeEnabled = userPreferences.IsManualModeEnabledObservable;
            AreRunningTimerNotificationsEnabled = userPreferences.AreRunningTimerNotificationsEnabledObservable;
            AreStoppedTimerNotificationsEnabled = userPreferences.AreStoppedTimerNotificationsEnabledObservable;

            WorkspaceName =
                dataSource.User.Current
                    .DistinctUntilChanged(user => user.DefaultWorkspaceId)
                    .SelectMany(_ => interactorFactory.GetDefaultWorkspace().Execute())
                    .Select(workspace => workspace.Name);

            BeginningOfWeek =
                dataSource.User.Current
                    .Select(user => user.BeginningOfWeek)
                    .DistinctUntilChanged()
                    .Select(beginningOfWeek => beginningOfWeek.ToString());

            DateFormat =
                dataSource.Preferences.Current
                    .Select(preferences => preferences.DateFormat.Localized)
                    .DistinctUntilChanged();

            DurationFormat =
                dataSource.Preferences.Current
                    .Select(preferences => preferences.DurationFormat)
                    .Select(DurationFormatToString.Convert)
                    .DistinctUntilChanged();

            UseTwentyFourHourFormat =
                dataSource.Preferences.Current
                    .Select(preferences => preferences.TimeOfDayFormat.IsTwentyFourHoursFormat)
                    .DistinctUntilChanged();

            UserAvatar =
                dataSource.User.Current
                    .Select(user => user.ImageUrl)
                    .DistinctUntilChanged()
                    .SelectMany(url => interactorFactory.GetUserAvatar(url).Execute());

            Workspaces =
                dataSource.User.Current
                    .DistinctUntilChanged(user => user.DefaultWorkspaceId)
                    .SelectMany(user => interactorFactory
                        .GetAllWorkspaces()
                        .Execute()
                        .Select(selectableWorkspacesFromWorkspaces(user))
                    );

            LoggingOut = loggingOutSubject.AsObservable();

            dataSource.User.Current
                .Subscribe(user => currentUser = user)
                .DisposedBy(disposeBag);

            dataSource.Preferences.Current
                .Subscribe(preferences => currentPreferences = preferences)
                .DisposedBy(disposeBag);

            IsRunningSync
                .Subscribe(isSyncing => this.isSyncing = isSyncing)
                .DisposedBy(disposeBag);

            IsFeedbackSuccessViewShowing = isFeedbackSuccessViewShowing.AsObservable();

            OpenCalendarSettings = UIAction.FromAsync(openCalendarSettings);
            OpenNotificationSettings = UIAction.FromAsync(openNotificationSettings);
            ToggleTwentyFourHourSettings = UIAction.FromAsync(toggleUseTwentyFourHourClock);
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            navigationFromMainViewModelStopwatch = stopwatchProvider.Get(MeasuredOperation.OpenSettingsView);
            stopwatchProvider.Remove(MeasuredOperation.OpenStartView);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            navigationFromMainViewModelStopwatch?.Stop();
            navigationFromMainViewModelStopwatch = null;
        }

        public void CloseFeedbackSuccessView()
        {
            isFeedbackSuccessViewShowing.OnNext(false);
        }

        public Task OpenAboutView()
            => navigationService.Navigate<AboutViewModel>();

        public Task OpenHelpView() =>
            navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(platformConstants.HelpUrl, Resources.Help)
            );

        public async Task PickDefaultWorkspace()
        {
            var defaultWorkspace = await interactorFactory.GetDefaultWorkspace().Execute();
            var parameters = WorkspaceParameters.Create(defaultWorkspace.Id, Resources.SetDefaultWorkspace, allowQuerying: false);
            var selectedWorkspaceId =
                await navigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(parameters);

            await changeDefaultWorkspace(selectedWorkspaceId);
        }

        public async Task SubmitFeedback()
        {
            var sendFeedbackSucceed = await navigationService.Navigate<SendFeedbackViewModel, bool>();
            isFeedbackSuccessViewShowing.OnNext(sendFeedbackSucceed);
        }

        public async Task SubmitFeedbackUsingEmail()
        {
            feedbackService.SubmitFeedback();
        }

        public Task SelectDefaultWorkspace(SelectableWorkspaceViewModel workspace)
            => changeDefaultWorkspace(workspace.WorkspaceId);

        private async Task toggleUseTwentyFourHourClock()
        {
            var timeFormat = currentPreferences.TimeOfDayFormat.IsTwentyFourHoursFormat
                ? TimeFormat.TwelveHoursFormat
                : TimeFormat.TwentyFourHoursFormat;

            await updatePreferences(timeFormat: timeFormat);
        }

        public void ToggleManualMode()
        {
            if (userPreferences.IsManualModeEnabled)
            {
                userPreferences.EnableTimerMode();
            }
            else
            {
                userPreferences.EnableManualMode();
            }
        }

        public void ToggleRunningTimerNotifications()
        {
            var newState = !userPreferences.AreRunningTimerNotificationsEnabled;
            userPreferences.SetRunningTimerNotifications(newState);
        }

        public void ToggleStoppedTimerNotifications()
        {
            var newState = !userPreferences.AreStoppedTimerNotificationsEnabled;
            userPreferences.SetStoppedTimerNotifications(newState);
        }

        public async Task TryLogout()
        {
            var synced = !isSyncing && await isSynced();
            if (synced)
            {
                await logout();
                return;
            }

            var (title, message) = isSyncing
                ? (Resources.SettingsSyncInProgressTitle, Resources.SettingsSyncInProgressMessage)
                : (Resources.SettingsUnsyncedTitle, Resources.SettingsUnsyncedMessage);

            await dialogService
                .Confirm(title, message, Resources.SettingsDialogButtonSignOut, Resources.Cancel)
                .SelectMany(shouldLogout
                    => shouldLogout ? logout() : Observable.Return(Unit.Default));
        }

        public async Task SelectDateFormat()
        {
            var newDateFormat = await navigationService
                .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(currentPreferences.DateFormat);

            if (currentPreferences.DateFormat == newDateFormat)
                return;

            await updatePreferences(dateFormat: newDateFormat);
        }

        public async Task SelectBeginningOfWeek()
        {
            var newBeginningOfWeek = await navigationService
                .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(currentUser.BeginningOfWeek);

            if (currentUser.BeginningOfWeek == newBeginningOfWeek)
                return;

            await interactorFactory.UpdateUser(new EditUserDTO { BeginningOfWeek = newBeginningOfWeek }).Execute();
            dataSource.SyncManager.InitiatePushSync();
        }

        public async Task SelectDurationFormat()
        {
            var newDurationFormat = await navigationService
                .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(currentPreferences.DurationFormat);

            if (currentPreferences.DurationFormat == newDurationFormat)
                return;

            await updatePreferences(newDurationFormat);
        }

        private Task openCalendarSettings()
            => navigationService.Navigate<CalendarSettingsViewModel>();

        private Task openNotificationSettings()
            => navigationService.Navigate<NotificationSettingsViewModel>();

        private IObservable<Unit> logout()
        {
            isLoggingOut = true;
            loggingOutSubject.OnNext(Unit.Default);
            analyticsService.Logout.Track(LogoutSource.Settings);
            userPreferences.Reset();

            privateSharedStorageService.ClearAll();
            intentDonationService.ClearAll();

            return dataSource.Logout().Do(_ => navigationService.Navigate<LoginViewModel>());
        }

        private IObservable<bool> isSynced()
            => dataSource.HasUnsyncedData().Select(Invert);

        private IObservable<bool> checkSynced(SyncProgress progress)
        {
            if (isLoggingOut || progress != SyncProgress.Synced)
                return Observable.Return(false);

            return isSynced();
        }

        private bool isRunningSync(SyncProgress progress)
            => isLoggingOut == false && progress == SyncProgress.Syncing;

        private async Task updatePreferences(
            New<DurationFormat> durationFormat = default(New<DurationFormat>),
            New<DateFormat> dateFormat = default(New<DateFormat>),
            New<TimeFormat> timeFormat = default(New<TimeFormat>))
        {
            var preferencesDto = new EditPreferencesDTO
            {
                DurationFormat = durationFormat,
                DateFormat = dateFormat,
                TimeOfDayFormat = timeFormat
            };

            await interactorFactory.UpdatePreferences(preferencesDto).Execute();
            dataSource.SyncManager.InitiatePushSync();
        }

        private async Task changeDefaultWorkspace(long selectedWorkspaceId)
        {
            if (selectedWorkspaceId == currentUser.DefaultWorkspaceId) return;

            await interactorFactory.UpdateDefaultWorkspace(selectedWorkspaceId).Execute();
            dataSource.SyncManager.InitiatePushSync();
        }

        private WorkspaceToSelectableWorkspaceLambda selectableWorkspacesFromWorkspaces(IThreadSafeUser user)
            => workspaces
                => workspaces
                    .Select(workspace => new SelectableWorkspaceViewModel(workspace, user.DefaultWorkspaceId == workspace.Id))
                    .ToList();
    }
}
