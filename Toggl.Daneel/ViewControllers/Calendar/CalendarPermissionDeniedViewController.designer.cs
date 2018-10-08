// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers.Calendar
{
    [Register ("CalendarPermissionDeniedViewController")]
    partial class CalendarPermissionDeniedViewController
    {
        [Outlet]
        UIKit.UIButton ContinueWithoutAccessButton { get; set; }


        [Outlet]
        UIKit.UIButton EnableAccessButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ContinueWithoutAccessButton != null) {
                ContinueWithoutAccessButton.Dispose ();
                ContinueWithoutAccessButton = null;
            }

            if (EnableAccessButton != null) {
                EnableAccessButton.Dispose ();
                EnableAccessButton = null;
            }
        }
    }
}