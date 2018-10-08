// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.SiriExtension.UI
{
	[Register ("IntentViewController")]
	partial class IntentViewController
	{
		[Outlet]
		UIKit.UILabel descriptionLabel { get; set; }

		[Outlet]
		UIKit.UILabel timeFrameLabel { get; set; }

		[Outlet]
		UIKit.UILabel timeLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (timeLabel != null) {
				timeLabel.Dispose ();
				timeLabel = null;
			}

			if (descriptionLabel != null) {
				descriptionLabel.Dispose ();
				descriptionLabel = null;
			}

			if (timeFrameLabel != null) {
				timeFrameLabel.Dispose ();
				timeFrameLabel = null;
			}
		}
	}
}
