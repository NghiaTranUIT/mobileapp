using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using FoundationResources = Toggl.Foundation.Resources;

namespace Toggl.Daneel.ViewControllers.Settings
{
    [MvxChildPresentation]
    public sealed partial class CalendarSettingsViewController : ReactiveViewController<CalendarSettingsViewModel>
    {
        private const int tableViewHeaderHeight = 106;

        public CalendarSettingsViewController() : base(nameof(CalendarSettingsViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.Title = FoundationResources.CalendarSettingsTitle;

            var header = CalendarSettingsTableViewHeader.Create();
            UserCalendarsTableView.TableHeaderView = header;
            header.TranslatesAutoresizingMaskIntoConstraints = false;
            header.HeightAnchor.ConstraintEqualTo(tableViewHeaderHeight).Active = true;
            header.WidthAnchor.ConstraintEqualTo(UserCalendarsTableView.WidthAnchor).Active = true;
            header.SetCalendarPermissionStatus(ViewModel.PermissionGranted);

            var source = new SelectUserCalendarsTableViewSource(UserCalendarsTableView, ViewModel.Calendars);
            source.SectionHeaderBackgroundColor = Color.Settings.Background.ToNativeColor();
            UserCalendarsTableView.Source = source;

            this.Bind(header.EnableCalendarAccessTapped, ViewModel.RequestAccessAction);
            this.Bind(source.ItemSelected, ViewModel.SelectCalendarAction);
        }
    }
}

