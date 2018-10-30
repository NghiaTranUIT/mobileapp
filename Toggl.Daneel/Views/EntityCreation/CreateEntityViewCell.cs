using System;

using Foundation;
using Toggl.Daneel.Cells;
using UIKit;

namespace Toggl.Daneel.Views.EntityCreation
{
    public partial class CreateEntityViewCell : BaseTableHeaderFooterView<string>
    {
        public static readonly NSString Key = new NSString("CreateEntityViewCell");
        public static readonly UINib Nib;

        static CreateEntityViewCell()
        {
            Nib = UINib.FromName("CreateEntityViewCell", NSBundle.MainBundle);
        }

        protected CreateEntityViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        protected override void UpdateView()
        {
            TextLabel.Text = Item;
        }
    }
}

