using System;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Diagnostics;
using Xunit;

namespace Toggl.Foundation.Tests.Diagnostics
{
    public sealed class BaseStopwatchProviderTests
    {
        public sealed class TheCreateMethod
        {
            private readonly BaseStopwatchProvider stopwatchProvider = new TestStopwatchProvider();

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsStopwatchesThatCanOnlyBeStartedOnce(bool outputToConsole)
            {
                var stopwatch = stopwatchProvider.Create(MeasuredOperation.None, outputToConsole);
                stopwatch.Start();

                Action callingStartTwice = stopwatch.Start;

                callingStartTwice.Should().Throw<InvalidOperationException>();
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsStopwatchesThatCanOnlyBeStoppedOnceStarted(bool outputToConsole)
            {
                var stopwatch = stopwatchProvider.Create(MeasuredOperation.None, outputToConsole);

                Action callingStopTwice = stopwatch.Stop;

                callingStopTwice.Should().Throw<InvalidOperationException>();
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsStopwatchesThatCanOnlyBeStoppedOnce(bool outputToConsole)
            {
                var stopwatch = stopwatchProvider.Create(MeasuredOperation.None, outputToConsole);
                stopwatch.Start();
                stopwatch.Stop();

                Action callingStopBeforeStart = stopwatch.Stop;

                callingStopBeforeStart.Should().Throw<InvalidOperationException>();
            }
        }

        public sealed class TheCreateAndStoreMethod
        {
            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void CallsTheCreateMethod(bool outputToConsole)
            {
                var stopwatchProviderMock = Substitute.For<BaseStopwatchProvider>();

                stopwatchProviderMock.CreateAndStore(MeasuredOperation.None, outputToConsole);

                stopwatchProviderMock.Received().Create(MeasuredOperation.None, outputToConsole);
            }
        }

        public sealed class TheGetMethod
        {
            private readonly BaseStopwatchProvider stopwatchProvider = new TestStopwatchProvider();

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnStopwatchesCreatedWithCreateAndStore(bool outputToConsole)
            {
                var stopwatch = stopwatchProvider.CreateAndStore(MeasuredOperation.None, outputToConsole);

                var storedStopwatch = stopwatchProvider.Get(MeasuredOperation.None);

                Assert.Equal(stopwatch, storedStopwatch);
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnNullForStopwatchesCreatedWithCreate(bool outputToConsole)
            {
                stopwatchProvider.Create(MeasuredOperation.None, outputToConsole);

                var storedStopwatch = stopwatchProvider.Get(MeasuredOperation.None);

                Assert.Null(storedStopwatch);
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnNullForStopwatchesThatWereRemoved(bool outputToConsole)
            {
                stopwatchProvider.CreateAndStore(MeasuredOperation.None, outputToConsole);
                stopwatchProvider.Remove(MeasuredOperation.None);

                var storedStopwatch = stopwatchProvider.Get(MeasuredOperation.None);

                Assert.Null(storedStopwatch);
            }
        }

        public sealed class TheRemoveMethod
        {
            private readonly BaseStopwatchProvider stopwatchProvider = new TestStopwatchProvider();

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void RemovesStopwatchesCreatedWithCreateAndStore(bool outputToConsole)
            {
                stopwatchProvider.CreateAndStore(MeasuredOperation.None, outputToConsole);
                var storedStopwatch = stopwatchProvider.Get(MeasuredOperation.None);

                stopwatchProvider.Remove(MeasuredOperation.None);
                var stopwatchRemovedGetResult = stopwatchProvider.Get(MeasuredOperation.None);

                Assert.NotNull(storedStopwatch);
                Assert.Null(stopwatchRemovedGetResult);
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void HasNoEffectForStopwatchesCreatedWithCreate(bool outputToConsole)
            {
                stopwatchProvider.Create(MeasuredOperation.None, outputToConsole);
                var storedStopwatch = stopwatchProvider.Get(MeasuredOperation.None);

                stopwatchProvider.Remove(MeasuredOperation.None);
                var stopwatchRemovedGetResult = stopwatchProvider.Get(MeasuredOperation.None);

                Assert.Null(storedStopwatch);
                Assert.Null(stopwatchRemovedGetResult);
            }

            [Fact]
            public void DoesNothingForStopwatchesThatWereNeverCreated()
            {
                stopwatchProvider.Remove(MeasuredOperation.None);
            }
        }

        private class TestStopwatchProvider : BaseStopwatchProvider
        {
            protected override IStopwatch NativeCreate(MeasuredOperation operation)
                => new DummyStopwatch(operation);

            private class DummyStopwatch : IStopwatch
            {
                public MeasuredOperation Operation { get; }

                public DummyStopwatch(MeasuredOperation operation)
                {
                    Operation = operation;
                }

                public void Start()
                {
                }

                public void Stop()
                {
                }
            }
        }
    }
}
