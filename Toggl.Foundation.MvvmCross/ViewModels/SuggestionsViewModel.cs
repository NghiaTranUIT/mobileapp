using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SuggestionsViewModel : MvxViewModel
    {
        public IObservable<Suggestion[]> Suggestions { get; private set; }

        public IObservable<bool> IsEmpty { get; private set; }

        public InputAction<Suggestion> StartTimeEntry { get; private set; }

        private readonly IInteractorFactory interactorFactory;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly ISuggestionProviderContainer suggestionProviders;
        private readonly ISchedulerProvider schedulerProvider;

        public SuggestionsViewModel(
            ITogglDataSource dataSource,
            IInteractorFactory interactorFactory,
            IOnboardingStorage onboardingStorage,
            ISuggestionProviderContainer suggestionProviders,
            ISchedulerProvider schedulerProvider)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(suggestionProviders, nameof(suggestionProviders));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));

            this.interactorFactory = interactorFactory;
            this.onboardingStorage = onboardingStorage;
            this.suggestionProviders = suggestionProviders;
            this.schedulerProvider = schedulerProvider;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            StartTimeEntry = InputAction<Suggestion>.FromAsync(suggestion => startTimeEntry(suggestion));

            Suggestions = suggestionProviders
                .Providers
                .Select(provider => provider.GetSuggestions())
                .Aggregate(Observable.Merge)
                .ToArray()
                .AsDriver(onErrorJustReturn: new Suggestion[0], schedulerProvider: schedulerProvider);

            IsEmpty = Suggestions
                .Select(suggestions => suggestions.Length == 0)
                .StartWith(true)
                .AsDriver(onErrorJustReturn: true, schedulerProvider: schedulerProvider);
        }

        private async Task startTimeEntry(Suggestion suggestion)
        {
            onboardingStorage.SetTimeEntryContinued();

            await interactorFactory
                .StartSuggestion(suggestion)
                .Execute();
        }
    }
}
