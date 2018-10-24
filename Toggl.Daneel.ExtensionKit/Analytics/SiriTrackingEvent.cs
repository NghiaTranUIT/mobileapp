using System.Collections.Generic;
using System.Linq;
using Foundation;
using Toggl.Multivac.Models;

namespace Toggl.Daneel.ExtensionKit.Analytics
{
    [Register("SiriTrackingEvent")]
    public class SiriTrackingEvent: NSObject, INSCoding
    {
        private static readonly string eventNameEncodeKey = nameof(eventNameEncodeKey);
        private static readonly string parametersEncodeKey = nameof(parametersEncodeKey);

        private static readonly string SiriStartTimerEventName = "SiriStartTimer";
        private static readonly string SiriStopTimerEventName = "SiriStopTimer";
        private static readonly string SiriIntentErrorEventName = "SiriIntentError";

        public readonly NSString EventName;
        public readonly NSDictionary<NSString, NSString> Parameters;

        public SiriTrackingEvent(NSString eventName, NSDictionary<NSString, NSString> parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }

        public static SiriTrackingEvent Error(string message)
        {
            var dict = new Dictionary<string, string>
            {
                ["Message"] = message
            };

            var nativeDict = NSDictionary<NSString, NSString>
                .FromObjectsAndKeys(dict.Values.ToArray(), dict.Keys.ToArray());

            return new SiriTrackingEvent(new NSString(SiriIntentErrorEventName), nativeDict);
        }

        public static SiriTrackingEvent StartTimer(ITimeEntry te)
        {
            var dict = new Dictionary<string, string>
            {
                ["HasEmptyDescription"] = string.IsNullOrEmpty(te.Description).ToString(),
                ["HasProject"] = (te.ProjectId != null).ToString(),
                ["HasTask"] = (te.TaskId != null).ToString(),
                ["NumberOfTags"] = te.TagIds.Count().ToString(),
                ["IsBillable"] = te.Billable.ToString()
            };

            var nativeDict = NSDictionary<NSString, NSString>
                .FromObjectsAndKeys(dict.Values.ToArray(), dict.Keys.ToArray());

            return new SiriTrackingEvent(new NSString(SiriStartTimerEventName), nativeDict);
        }

        public static SiriTrackingEvent StopTimer()
        {
            return new SiriTrackingEvent(new NSString(SiriStopTimerEventName), null);
        }

        #region INSCoding
        [Export("initWithCoder:")]
        public SiriTrackingEvent(NSCoder coder)
        {
            EventName = (NSString) coder.DecodeObject(eventNameEncodeKey);

            var dict = (NSDictionary) coder.DecodeObject(parametersEncodeKey);
            if (dict != null)
            {
                Parameters = NSDictionary<NSString, NSString>.FromObjectsAndKeys(dict.Values, dict.Keys);
            }
        }

        [Export ("encodeWithCoder:")]
        public void EncodeTo(NSCoder encoder)
        {
            encoder.Encode(EventName, eventNameEncodeKey);
            encoder.Encode(Parameters, parametersEncodeKey);
        }
        #endregion
    }
}