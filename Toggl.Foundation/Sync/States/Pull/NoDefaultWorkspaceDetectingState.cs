using System;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;

namespace Toggl.Foundation.Sync.States.Pull
{
    internal sealed class NoDefaultWorkspaceDetectingState : ISyncState
    {
        private readonly ITogglDataSource dataSource;
        private readonly IAnalyticsService analyticsService;

        public StateResult Continue { get; } = new StateResult();
        public StateResult NoDefaultWorkspaceDetected { get; } = new StateResult();

        public NoDefaultWorkspaceDetectingState(
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.dataSource = dataSource;
            this.analyticsService = analyticsService;
        }

        public IObservable<ITransition> Start()
            => dataSource.User.Get()
                .Select(user => user.DefaultWorkspaceId.HasValue)
                .Do(trackNoDefaultWorkspaceIfNeeded)
                .Select(userHasDefaultWorkspace => userHasDefaultWorkspace
                    ? Continue.Transition()
                    : NoDefaultWorkspaceDetected.Transition());

        private void trackNoDefaultWorkspaceIfNeeded(bool userHasDefaultWorkspace)
        {
            if (!userHasDefaultWorkspace)
                analyticsService.NoDefaultWorkspace.Track();
        }
    }
}
