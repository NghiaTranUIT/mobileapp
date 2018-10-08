using System;

using Foundation;
using UIKit;

namespace Toggl.Daneel.Cells.Calendar
{
    public partial class EditingHourSupplementaryView : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("EditingHourSupplementaryView");
        public static readonly UINib Nib;

        static EditingHourSupplementaryView()
        {
            Nib = UINib.FromName("EditingHourSupplementaryView", NSBundle.MainBundle);
        }

        protected EditingHourSupplementaryView(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public void SetLabel(string label)
        {
            HourLabel.Text = label;
        }
    }
}

