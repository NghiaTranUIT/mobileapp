﻿using System;
using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static partial class UIKitRxExtensions
    {
        public static Action<bool> NetworkActivityIndicatorVisible<T>(this Reactive<T> reactive) where T : UIApplication
            => visible => reactive.Base.NetworkActivityIndicatorVisible = visible;
    }
}
