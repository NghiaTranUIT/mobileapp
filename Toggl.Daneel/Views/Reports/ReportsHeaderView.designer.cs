// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.Views.Reports
{
	[Register ("ReportsHeaderView")]
	partial class ReportsHeaderView
	{
		[Outlet]
		UIKit.UIView BarChartCardView { get; set; }

		[Outlet]
		UIKit.UIView BarChartContainerView { get; set; }

		[Outlet]
		UIKit.UIStackView BarsStackView { get; set; }

		[Outlet]
		UIKit.UILabel BillableLegendLabel { get; set; }

		[Outlet]
		UIKit.UILabel BillablePercentageLabel { get; set; }

		[Outlet]
		Toggl.Daneel.Views.Reports.PercentageView BillablePercentageView { get; set; }

		[Outlet]
		UIKit.UILabel BillableTitleLabel { get; set; }

		[Outlet]
		UIKit.UILabel ClockedHoursTitleLabel { get; set; }

		[Outlet]
		UIKit.UIView ColorsLegendContainerView { get; set; }

		[Outlet]
		UIKit.UIView EmptyStateView { get; set; }

		[Outlet]
		UIKit.UILabel EndDateLabel { get; set; }

		[Outlet]
		UIKit.UILabel HalfHoursLabel { get; set; }

		[Outlet]
		UIKit.UIStackView HorizontalLegendStackView { get; set; }

		[Outlet]
		UIKit.UIView LoadingOverviewView { get; set; }

		[Outlet]
		Toggl.Daneel.Views.Reports.LoadingPieChartView LoadingPieChartView { get; set; }

		[Outlet]
		UIKit.UILabel MaximumHoursLabel { get; set; }

		[Outlet]
		UIKit.UILabel NonBillableLegendLabel { get; set; }

		[Outlet]
		UIKit.UIView OverviewCardView { get; set; }

		[Outlet]
		Toggl.Daneel.Views.Reports.PieChartView PieChartView { get; set; }

		[Outlet]
		UIKit.UILabel StartDateLabel { get; set; }

		[Outlet]
		UIKit.UIImageView TotalDurationGraph { get; set; }

		[Outlet]
		UIKit.UILabel TotalDurationLabel { get; set; }

		[Outlet]
		UIKit.UILabel TotalTitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BarChartCardView != null) {
				BarChartCardView.Dispose ();
				BarChartCardView = null;
			}

			if (BarChartContainerView != null) {
				BarChartContainerView.Dispose ();
				BarChartContainerView = null;
			}

			if (BarsStackView != null) {
				BarsStackView.Dispose ();
				BarsStackView = null;
			}

			if (BillableLegendLabel != null) {
				BillableLegendLabel.Dispose ();
				BillableLegendLabel = null;
			}

			if (BillablePercentageLabel != null) {
				BillablePercentageLabel.Dispose ();
				BillablePercentageLabel = null;
			}

			if (BillablePercentageView != null) {
				BillablePercentageView.Dispose ();
				BillablePercentageView = null;
			}

			if (BillableTitleLabel != null) {
				BillableTitleLabel.Dispose ();
				BillableTitleLabel = null;
			}

			if (ClockedHoursTitleLabel != null) {
				ClockedHoursTitleLabel.Dispose ();
				ClockedHoursTitleLabel = null;
			}

			if (EmptyStateView != null) {
				EmptyStateView.Dispose ();
				EmptyStateView = null;
			}

			if (EndDateLabel != null) {
				EndDateLabel.Dispose ();
				EndDateLabel = null;
			}

			if (HalfHoursLabel != null) {
				HalfHoursLabel.Dispose ();
				HalfHoursLabel = null;
			}

			if (HorizontalLegendStackView != null) {
				HorizontalLegendStackView.Dispose ();
				HorizontalLegendStackView = null;
			}

			if (LoadingOverviewView != null) {
				LoadingOverviewView.Dispose ();
				LoadingOverviewView = null;
			}

			if (LoadingPieChartView != null) {
				LoadingPieChartView.Dispose ();
				LoadingPieChartView = null;
			}

			if (MaximumHoursLabel != null) {
				MaximumHoursLabel.Dispose ();
				MaximumHoursLabel = null;
			}

			if (NonBillableLegendLabel != null) {
				NonBillableLegendLabel.Dispose ();
				NonBillableLegendLabel = null;
			}

			if (OverviewCardView != null) {
				OverviewCardView.Dispose ();
				OverviewCardView = null;
			}

			if (PieChartView != null) {
				PieChartView.Dispose ();
				PieChartView = null;
			}

			if (StartDateLabel != null) {
				StartDateLabel.Dispose ();
				StartDateLabel = null;
			}

			if (TotalDurationGraph != null) {
				TotalDurationGraph.Dispose ();
				TotalDurationGraph = null;
			}

			if (TotalDurationLabel != null) {
				TotalDurationLabel.Dispose ();
				TotalDurationLabel = null;
			}

			if (TotalTitleLabel != null) {
				TotalTitleLabel.Dispose ();
				TotalTitleLabel = null;
			}

			if (ColorsLegendContainerView != null) {
				ColorsLegendContainerView.Dispose ();
				ColorsLegendContainerView = null;
			}
		}
	}
}
