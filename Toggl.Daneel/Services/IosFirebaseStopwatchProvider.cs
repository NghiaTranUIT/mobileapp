using Firebase.PerformanceMonitoring;
using Toggl.Foundation.Diagnostics;

namespace Toggl.Daneel.Services
{
    public sealed class IosFirebaseStopwatchProvider : BaseStopwatchProvider
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
                firebaseTrace = Performance.StartTrace(Operation.ToString());
            }

            public void Stop()
            {
                firebaseTrace.Stop();
            }
        }
    }
}
