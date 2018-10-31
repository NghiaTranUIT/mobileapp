using System;
using Toggl.Multivac.Extensions;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Extensions.Reactive
{
    public static class ReactiveBindingHolderExtensions
    {
        public static void BindToAction<TInput, TOutput>(this IReactiveBindingHolder holder, UIButton button, RxAction<TInput, TOutput> action, Func<UIButton, TInput> transform)
        {
            button.Rx().Tap()
                .Select(_ => transform(button))
                .Subscribe(action.Inputs)
                .DisposedBy(holder.DisposeBag);

            action.Enabled
                .Subscribe(e => { button.Enabled = e; })
                .DisposedBy(holder.DisposeBag);
        }

        public static void BindToAction<TOutput>(this IReactiveBindingHolder holder, UIButton button, RxAction<Unit, TOutput> action)
        {
            holder.BindToAction(button, action, _ => Unit.Default);
        }
    }
}
