using System;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Toggl.Multivac.Extensions;
using Xunit;

namespace Toggl.Multivac.Tests
{
    public sealed class RxActionTests
    {
        public sealed class TheErrorsProperty : ReactiveTest
        {
            [Fact, LogIfTooSlow]
            public void ForwardsErrorsInTheExecution()
            {
                var testScheduler = new TestScheduler();
                var exception = new Exception("This is an error");

                var action = new ThrowingRxAction(exception);

                var observer = testScheduler.CreateObserver<Exception>();

                action.Errors.Subscribe(observer);

                testScheduler.Sleep(3);
                action.Execute(2);
                testScheduler.Start();

                observer.Messages.AssertEqual(
                    OnNext(3, exception)
                );
            }
        }

        public sealed class TheExecutingProperty : ReactiveTest
        {
            [Fact, LogIfTooSlow]
            public void ReturnsTrueWhileExecuting()
            {
                var testScheduler = new TestScheduler();

                var action = new TestRxAction(testScheduler);

                var observer = testScheduler.CreateObserver<bool>();

                action.Executing.Subscribe(observer);

                testScheduler.Sleep(3);
                action.Execute(2);
                testScheduler.Start();

                observer.Messages.AssertEqual(
                    OnNext(0, false),
                    OnNext(3, true),
                    OnNext(5, false)
                );
            }
        }

        public sealed class TheEnabledProperty : ReactiveTest
        {
            [Fact, LogIfTooSlow]
            public void ReturnsFalseWhileExecuting()
            {
                var testScheduler = new TestScheduler();

                var action = new TestRxAction(testScheduler);

                var observer = testScheduler.CreateObserver<bool>();

                action.Enabled.Subscribe(observer);

                testScheduler.Sleep(3);
                action.Execute(2);
                testScheduler.Start();

                observer.Messages.AssertEqual(
                    OnNext(0, true),
                    OnNext(3, false),
                    OnNext(5, true)
                );
            }
        }

        public sealed class TheExecuteMethod : ReactiveTest
        {
            [Fact, LogIfTooSlow]
            public void ReturnsTheResultsOfTheOperation()
            {
                var testScheduler = new TestScheduler();

                var action = new TestRxAction(testScheduler);

                var observer = testScheduler.CreateObserver<string>();

                testScheduler.Sleep(3);
                action.Execute(2).Subscribe(observer);
                testScheduler.Start();

                observer.Messages.AssertEqual(
                    OnNext(4, "0"),
                    OnNext(5, "1"),
                    OnCompleted<string>(5)
                );
            }
        }

        private sealed class ThrowingRxAction : RxAction<int, string>
        {
            public ThrowingRxAction(Exception exception)
                : base(workFactory(exception))
            {
            }

            private static Func<int, IObservable<string>> workFactory(Exception exception)
                => i => Observable.Throw<string>(exception);
        }

        private sealed class TestRxAction : RxAction<int, string>
        {
            public TestRxAction(TestScheduler scheduler)
                : base(workFactory(scheduler))
            {
            }

            private static Func<int, IObservable<string>> workFactory(TestScheduler scheduler)
                => i => Observable.Interval(TimeSpan.FromTicks(1), scheduler)
                    .Take(i)
                    .Select(l => l.ToString());
        }
    }
}
