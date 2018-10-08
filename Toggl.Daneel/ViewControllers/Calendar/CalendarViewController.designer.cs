// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers
{
    [Register ("CalendarViewController")]
    partial class CalendarViewController
    {
        [Outlet]
        UIKit.UICollectionView CalendarCollectionView { get; set; }

        [Outlet]
        UIKit.UIButton GetStartedButton { get; set; }

        [Outlet]
        UIKit.UIView OnboardingView { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (GetStartedButton != null) {
                GetStartedButton.Dispose ();
                GetStartedButton = null;
            }

            if (OnboardingView != null) {
                OnboardingView.Dispose ();
                OnboardingView = null;
            }

            if (CalendarCollectionView != null) {
                CalendarCollectionView.Dispose ();
                CalendarCollectionView = null;
            }
        }
    }
}
