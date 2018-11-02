using System;
using Foundation;
using Toggl.Daneel.Cells;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Views.Client
{
    public partial class CreateClientViewCell : BaseTableViewCell<SelectableClientViewModel>
    {
        public static readonly NSString Key = new NSString("CreateClientViewCell");
        public static readonly UINib Nib;

        static CreateClientViewCell()
        {
            Nib = UINib.FromName("CreateClientViewCell", NSBundle.MainBundle);
        }

        protected CreateClientViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        protected override void UpdateView()
        {
            TextLabel.Text = $"Create client \"{Item.Name.Trim()}\"";
        }
    }
}

