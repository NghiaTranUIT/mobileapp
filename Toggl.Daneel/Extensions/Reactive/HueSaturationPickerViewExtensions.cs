using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Daneel.Views;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.Reactive;
using Toggl.Multivac.Extensions;
using UIKit;

namespace Toggl.Daneel.Extensions.Reactive
{
    public static class HueSaturationPickerViewExtensions
    {
        public static IObservable<Unit> Hue(this IReactive<HueSaturationPickerView> reactive)
            => Observable
                .FromEventPattern(e => reactive.Base.HueChanged += e, e => reactive.Base.HueChanged -= e)
                .SelectUnit();

        public static IObservable<Unit> Saturation(this IReactive<HueSaturationPickerView> reactive)
            => Observable
                .FromEventPattern(e => reactive.Base.SaturationChanged += e, e => reactive.Base.SaturationChanged -= e)
                .SelectUnit();

        public static Action<float> HueObserver(this IReactive<HueSaturationPickerView> reactive)
            => hue => reactive.Base.Hue = hue;
       
        public static Action<float> SaturationObserver(this IReactive<HueSaturationPickerView> reactive)
            => saturation => reactive.Base.Saturation = saturation;

        public static Action<float> ValueObserver(this IReactive<HueSaturationPickerView> reactive)
            => value => reactive.Base.Value = value;
    }
}
