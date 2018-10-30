using System;

using Foundation;
using UIKit;

namespace Toggl.Daneel.Views.EntityCreation
{
    public partial class CreateEntityViewCell : UITableViewCell
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
    }
}

