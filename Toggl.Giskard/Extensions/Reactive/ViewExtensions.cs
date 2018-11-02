using System;
using System.Reactive;
using System.Reactive.Linq;
using Android.Views;
using Toggl.Foundation.MvvmCross.Reactive;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Extensions.Reactive
{
    public enum ButtonEventType
    {
        Tap,
        LongPress
    }

    public static class ViewExtensions
    {
        public static IObservable<Unit> Tap(this IReactive<View> reactive)
            => Observable
                .FromEventPattern(e => reactive.Base.Click += e, e => reactive.Base.Click -= e)
                .SelectUnit();

        public static Action<bool> Enabled(this IReactive<View> reactive)
            => enabled => reactive.Base.Enabled = enabled;

        public static Action<bool> IsVisible(this IReactive<View> reactive, bool useGone = true)
            => isVisible => reactive.Base.Visibility = isVisible.ToVisibility(useGone);

        public static IDisposable BindAction(this IReactive<View> reactive, UIAction action, ButtonEventType eventType = ButtonEventType.Tap)
        {
            IObservable<Unit> eventObservable = Observable.Empty<Unit>();
            switch (eventType)
            {
                case ButtonEventType.Tap:
                    eventObservable = reactive.Base.Rx().Tap();
                    break;
                case ButtonEventType.LongPress:
                    throw new ArgumentException("Event type not implemented");
                    //eventObservable = reactive.Base.Rx().LongPress();
                    break;
            }

            return Observable.Using(
                    () => action.Enabled.Subscribe(e => { reactive.Base.Enabled = e; }),
                    _ => eventObservable
                )
                .Subscribe(action.Inputs);
        }
    }
}
