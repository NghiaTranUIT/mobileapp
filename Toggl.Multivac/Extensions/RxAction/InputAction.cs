using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Toggl.Multivac.Extensions
{
    public class InputAction<TInput> : RxAction<TInput, Unit>
    {
        private InputAction(Func<TInput, IObservable<Unit>> workFactory, IObservable<bool> enabledIf = null)
            : base(workFactory, enabledIf)
        {
        }

        public static InputAction<TInput> FromAction(Action<TInput> action)
        {
            IObservable<Unit> workFactory(TInput input)
                => Observable.Create<Unit>(observer =>
                    {
                        action(input);
                        observer.CompleteWith(Unit.Default);
                        return Disposable.Empty;
                    });

            return new InputAction<TInput>(workFactory);
        }

        public static InputAction<TInput> FromAsync(Func<TInput, Task> asyncAction, IObservable<bool> enabledIf = null)
        {
            IObservable<Unit> workFactory(TInput input)
                => asyncAction(input).ToObservable();

            return new InputAction<TInput>(workFactory, enabledIf);
        }

        public static InputAction<TInput> FromObservable(Func<TInput, IObservable<Unit>> workFactory, IObservable<bool> enabledIf = null)
            => new InputAction<TInput>(workFactory, enabledIf);
    }

    public static class CompletableActionExtensions
    {
        public static IObservable<Unit> Execute<TInput>(this InputAction<TInput> action, TInput value)
        {
            return action.Execute(value);
        }
    }
}
