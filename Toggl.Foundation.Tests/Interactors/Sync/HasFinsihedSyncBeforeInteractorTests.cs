using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.Mocks;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors.Workspace
{
    public sealed class HasFinsihedSyncBeforeInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource)
            {
                Action tryingToConstructWithNull = () => new HasFinsihedSyncBeforeInteractor(
                    useDataSource ? DataSource : null
                );

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private HasFinsihedSyncBeforeInteractor interactor;

            public TheExecuteMethod()
            {
                interactor = new HasFinsihedSyncBeforeInteractor(DataSource);
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsFalseIfPreferencesIsNotPersisted()
            {
                DataSource.Preferences.Get().Returns(Observable.Throw<IThreadSafePreferences>(new Exception()));
                (await interactor.Execute().SingleAsync()).Should().Be(false);
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsTrueIfPreferencesArePersisted()
            {
                DataSource.Preferences.Get().Returns(Observable.Return(new MockPreferences()));
                (await interactor.Execute().SingleAsync()).Should().Be(true);
            }
        }
    }
}
