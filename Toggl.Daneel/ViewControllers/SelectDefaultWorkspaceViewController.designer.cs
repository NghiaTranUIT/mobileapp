// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers
{
	[Register ("SelectDefaultWorkspaceViewController")]
	partial class SelectDefaultWorkspaceViewController
	{
		[Outlet]
		UIKit.UITableView WorkspacesTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (WorkspacesTableView != null) {
				WorkspacesTableView.Dispose ();
				WorkspacesTableView = null;
			}
		}
	}
}
