using Com.Google.Firebase.Perf;
using Com.Google.Firebase.Perf.Metrics;
using Toggl.Foundation.Diagnostics;

namespace Toggl.Giskard.Services
{
    public sealed class AndroidFirebaseStopwatchProvider : BaseStopwatchProvider
    {
        protected override IStopwatch NativeCreate(MeasuredOperation operation)
            => new AndroidFirebaseStopwatch(operation);

        private class AndroidFirebaseStopwatch : IStopwatch
        {
            private readonly Trace firebaseTrace;

            public MeasuredOperation Operation { get; }

            public AndroidFirebaseStopwatch(MeasuredOperation operation)
            {
                Operation = operation;

                firebaseTrace = FirebasePerformance.Instance.NewTrace(operation.ToString());
            }

            public void Start()
            {
                firebaseTrace.Start();
            }

            public void Stop()
            {
                firebaseTrace.Stop();
            }
        }
    }
}
