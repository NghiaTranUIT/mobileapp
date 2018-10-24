using System.Collections.Generic;
using Foundation;
using Toggl.Daneel.ExtensionKit.Analytics;
using Toggl.Foundation.Analytics;

namespace Toggl.Daneel
{
    public class SiriTrackableEvent: ITrackableEvent
    {
        public string EventName => internalTrackingEvent.EventName;

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();

            if (internalTrackingEvent.Parameters != null)
            {
                foreach (var item in internalTrackingEvent.Parameters)
                {
                    dict.Add((NSString)item.Key, (NSString)item.Value);
                }
            }

            return dict;
        }

        private SiriTrackingEvent internalTrackingEvent;
        private NSDateFormatter dateFormatter = new NSDateFormatter()
        {
            DateFormat = "yyyy-MM-dd'T'HH:mm:ssZ",
            TimeZone = NSTimeZone.FromGMT(0)
        };

        public SiriTrackableEvent(SiriTrackingEvent internalTrackingEvent)
        {
            this.internalTrackingEvent = internalTrackingEvent;
        }
    }
}