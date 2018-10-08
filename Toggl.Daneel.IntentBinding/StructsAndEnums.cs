using System;
using ObjCRuntime;

namespace Toggl.Daneel.Intents
{
    // [Watch (5,0), iOS (12,0)]
    [Native]
    public enum ShowReportIntentResponseCode : long
    {
        Unspecified = 0,
        Ready,
        ContinueInApp,
        InProgress,
        Success,
        Failure,
        FailureRequiringAppLaunch
    }

    // [Watch (5,0), iOS (12,0)]
    [Native]
    public enum ShowReportPeriodReportPeriod : long
    {
        Unknown = 0,
        Today = 1,
        Yesterday = 2,
        ThisWeek = 3,
        LastWeek = 4,
        ThisMonth = 5,
        LastMonth = 6,
        ThisYear = 7
    }

    // [Watch (5,0), iOS (12,0)]
    [Native]
    public enum ShowReportPeriodIntentResponseCode : long
    {
        Unspecified = 0,
        Ready,
        ContinueInApp,
        InProgress,
        Success,
        Failure,
        FailureRequiringAppLaunch
    }

    // [Watch (5,0), iOS (12,0)]
    [Native]
    public enum StartTimerIntentResponseCode : long
    {
        Unspecified = 0,
        Ready,
        ContinueInApp,
        InProgress,
        Success,
        Failure,
        FailureRequiringAppLaunch,
        FailureNoApiToken = 100
    }

    // [Watch (5,0), iOS (12,0)]
    [Native]
    public enum StopTimerIntentResponseCode : long
    {
        Unspecified = 0,
        Ready,
        ContinueInApp,
        InProgress,
        Success,
        Failure,
        FailureRequiringAppLaunch,
        FailureNoRunningEntry = 100,
        FailureNoApiToken
    }
}
