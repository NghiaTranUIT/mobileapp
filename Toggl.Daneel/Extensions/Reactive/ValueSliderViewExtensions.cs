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
    public static class ValueSliderViewExtensions
    {
        public static Action<float> Hue(this IReactive<ValueSliderView> reactive)
            => hue => reactive.Base.Hue = hue;
       
        public static Action<float> Saturation(this IReactive<ValueSliderView> reactive)
            => saturation => reactive.Base.Saturation = saturation;
    }
}
