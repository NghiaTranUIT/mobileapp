﻿using System;
using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static partial class UIKitRxExtensions
    {
        public static Action<nfloat> Constant<T>(this Reactive<T> reactive) where T: NSLayoutConstraint
            => constant => reactive.Base.Constant = constant;
    }
}
