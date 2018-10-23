using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Toggl.Multivac.Extensions
{
    public sealed class UIAction : RxAction<Unit, Unit>
    {
        private UIAction(Func<IObservable<Unit>> workFactory, IObservable<bool> enabledIf)
            : base(_ => workFactory(), enabledIf)
        {
        }

        public IObservable<Unit> Execute()
            => Execute(Unit.Default);

        public static UIAction FromAction(Action action, IObservable<bool> enabledIf = null)
        {
            IObservable<Unit> workFactory()
                => Observable.Create<Unit>(observer =>
                {
                    action();
                    observer.CompleteWith(Unit.Default);
                    return Disposable.Empty;
                });

            return new UIAction(workFactory, enabledIf);
        }

        public static UIAction FromAsync(Func<Task> asyncAction, IObservable<bool> enabledIf = null)
        {
            IObservable<Unit> workFactory()
                => asyncAction().ToObservable();

            return new UIAction(workFactory, enabledIf);
        }

        public static UIAction FromObservable(Func<IObservable<Unit>> workFactory, IObservable<bool> enabledIf = null)
            => new UIAction(workFactory, enabledIf);
    }

    public static class RxActionExtensions
    {
        public static IObservable<Unit> Execute(this UIAction action)
        {
            return action.Execute(Unit.Default);
        }
    }
}
