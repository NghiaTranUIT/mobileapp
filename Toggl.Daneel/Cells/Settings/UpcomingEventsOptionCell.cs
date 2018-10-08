using System;
using Foundation;
using UIKit;

namespace Toggl.Daneel.Cells.Settings
{
    public sealed partial class UpcomingEventsOptionCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(UpcomingEventsOptionCell));
        public static readonly UINib Nib;

        static UpcomingEventsOptionCell()
        {
            Nib = UINib.FromName(nameof(UpcomingEventsOptionCell), NSBundle.MainBundle);
        }

        public string Text
        {
            get => TitleLabel.Text;
            set => TitleLabel.Text = value;
        }

        public UpcomingEventsOptionCell(IntPtr handle) : base(handle)
        {
        }

        public override void SetSelected(bool selected, bool animated)
        {
            base.SetSelected(selected, animated);
            SelectedImageView.Hidden = !selected;
        }
    }
}
