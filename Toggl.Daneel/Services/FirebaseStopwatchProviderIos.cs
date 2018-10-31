using Firebase.PerformanceMonitoring;
using Toggl.Foundation.Diagnostics;

namespace Toggl.Daneel.Services
{
    public sealed class FirebaseStopwatchProviderIos : BaseStopwatchProvider
    {
        protected override IStopwatch NativeCreate(MeasuredOperation operation)
            => new IosFirebaseStopwatch(operation);

        private class IosFirebaseStopwatch : IStopwatch
        {
            private Trace firebaseTrace;

            public MeasuredOperation Operation { get; }

            public IosFirebaseStopwatch(MeasuredOperation operation)
            {
                Operation = operation;
            }

            public void Start()
            {
                #if USE_ANALYTICS
                firebaseTrace = Performance.StartTrace(Operation.ToString());
                #endif
            }

            public void Stop()
            {
                #if USE_ANALYTICS
                firebaseTrace.Stop();
                #endif
            }
        }
    }
}
