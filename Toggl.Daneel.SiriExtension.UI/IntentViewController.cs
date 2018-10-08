using System;
using CoreGraphics;
using Foundation;
using Intents;
using IntentsUI;
using UIKit;
using CoreFoundation;
using CoreAnimation;
using Toggl.Daneel.Intents;

namespace Toggl.Daneel.SiriExtension.UI
{
    public partial class IntentViewController : UIViewController, IINUIHostedViewControlling
    {
        protected IntentViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        [Export("configureViewForParameters:ofInteraction:interactiveBehavior:context:completion:")]
        public void ConfigureView(
                 NSSet<INParameter> parameters,
                 INInteraction interaction,
                 INUIInteractiveBehavior interactiveBehavior,
                 INUIHostedViewContext context,
                 INUIHostedViewControllingConfigureViewHandler completion)
        {
            switch (interaction.Intent)
            {
                case StartTimerIntent startTimerIntent:
                    var desiredSize = CGSize.Empty;

                    if (interaction.IntentHandlingStatus == INIntentHandlingStatus.Success)
                    {
                        showStartTimerSuccess(startTimerIntent.EntryDescription);
                        desiredSize = new CGSize(200, 60);
                    }

                    completion(true, parameters, desiredSize);
                    break;
                case StopTimerIntent _:
                    desiredSize = CGSize.Empty;

                    if (interaction.IntentHandlingStatus == INIntentHandlingStatus.Success)
                    {
                        var response = interaction.IntentResponse as StopTimerIntentResponse;
                        if (!(response is null))
                        {
                            showStopResponse(response);
                            desiredSize = new CGSize(300, 60);
                        }
                    }
                    completion(true, parameters, desiredSize);
                    break;
                default:
                    completion(false, new NSSet<INParameter>(), CGSize.Empty);
                    break;
            }
        }

        private void showStartTimerSuccess(string description)
        {
            descriptionLabel.Text = string.IsNullOrEmpty(description) ? "No Description" : description;
            timeLabel.Text = "";
            timeFrameLabel.Text = "";

            var start = DateTimeOffset.Now;
            var displayLink = CADisplayLink.Create(() => {
                var passed = DateTimeOffset.Now - start;
                timeLabel.Text = secondsToString(passed.Seconds);
            });
            displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
        }

        private void showStopResponse(StopTimerIntentResponse response)
        {
            descriptionLabel.Text = response.EntryDescription;

            timeLabel.Text = secondsToString(response.EntryDuration.DoubleValue);

            var startTime = DateTimeOffset.FromUnixTimeSeconds(response.EntryStart.LongValue).ToLocalTime();
            var endTime = DateTimeOffset.FromUnixTimeSeconds(response.EntryStart.LongValue + response.EntryDuration.LongValue).ToLocalTime();
            var fromTime = startTime.ToString("HH:mm");
            var toTime = endTime.ToString("HH:mm");
            timeFrameLabel.Text = $"{fromTime} - {toTime}";
        }

        [Export("configureWithInteraction:context:completion:")]
        public void Configure(INInteraction interaction, INUIHostedViewContext context, Action<CGSize> completion)
        {
            throw new NotImplementedException();
        }

        private string secondsToString(Double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}
