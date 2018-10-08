using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Toggl.Daneel.Extensions;
using Toggl.Multivac;
using UIKit;

namespace Toggl.Daneel.Views.Calendar
{
    public abstract class CalendarCollectionViewAutoScrollHelper : NSObject
    {
        protected UICollectionView CollectionView { get; }
        protected CalendarCollectionViewLayout Layout { get; }

        protected CGPoint LastPoint { get; set; }

        protected float TopAutoScrollLine => (float)CollectionView.ContentOffset.Y + autoScrollZoneHeight;
        protected float BottomAutoScrollLine => (float)(CollectionView.ContentOffset.Y + CollectionView.Frame.Height - autoScrollZoneHeight);

        private bool shouldStopAutoScrollUp => isAutoScrollingUp && CollectionView.IsAtTop();
        private bool shouldStopAutoScrollDown => isAutoScrollingDown && CollectionView.IsAtBottom();

        private bool isAutoScrollingUp;
        private bool isAutoScrollingDown;
        private CADisplayLink displayLink;

        private const int autoScrollZoneHeight = 50;
        private const int autoScrollsPerSecond = 8;
        private readonly float autoScrollAmount;

        protected UISelectionFeedbackGenerator selectionFeedback = new UISelectionFeedbackGenerator();
        protected UIImpactFeedbackGenerator impactFeedback = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);

        protected CalendarCollectionViewAutoScrollHelper(
            UICollectionView collectionView,
            CalendarCollectionViewLayout layout)
        {
            Ensure.Argument.IsNotNull(layout, nameof(layout));
            Ensure.Argument.IsNotNull(collectionView, nameof(collectionView));

            Layout = layout;
            CollectionView = collectionView;

            autoScrollAmount = CalendarCollectionViewLayout.HourHeight / 4;
        }

        public CalendarCollectionViewAutoScrollHelper(IntPtr handle) : base(handle)
        {
        }

        protected void StartAutoScrollUp(Action<CGPoint> updateEntryAction)
        {
            isAutoScrollingUp = true;
            if (displayLink == null)
                createDisplayLinkWithScrollAmount(-autoScrollAmount - 10, updateEntryAction);
        }

        protected void StartAutoScrolDown(Action<CGPoint> updateEntryAction)
        {
            isAutoScrollingDown = true;
            if (displayLink == null)
                createDisplayLinkWithScrollAmount(autoScrollAmount, updateEntryAction);
        }

        protected void StopAutoScroll()
        {
            if (displayLink == null) return;

            displayLink.Paused = true;
            displayLink.Dispose();
            displayLink = null;
            isAutoScrollingUp = false;
            isAutoScrollingDown = false;
        }

        private void createDisplayLinkWithScrollAmount(float scrollAmount, Action<CGPoint> updateEntryAction)
        {
            displayLink = CADisplayLink.Create(() =>
            {
                if (shouldStopAutoScrollUp || shouldStopAutoScrollDown)
                {
                    StopAutoScroll();
                    return;
                }

                var point = LastPoint;
                point.Y += scrollAmount;
                var targetOffset = CollectionView.ContentOffset;
                targetOffset.Y += scrollAmount;
                CollectionView.SetContentOffset(targetOffset, false);
                updateEntryAction(point);
            });

            displayLink.PreferredFramesPerSecond = autoScrollsPerSecond;
            displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
        }
    }
}
