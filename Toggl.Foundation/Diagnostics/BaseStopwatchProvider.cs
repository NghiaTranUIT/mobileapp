using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Toggl.Foundation.Diagnostics
{
    public abstract class BaseStopwatchProvider : IStopwatchProvider
    {
        private readonly Dictionary<MeasuredOperation, IStopwatch> cachedStopwatches;

        protected BaseStopwatchProvider()
        {
            cachedStopwatches = new Dictionary<MeasuredOperation, IStopwatch>();
        }

        public IStopwatch Create(MeasuredOperation operation, bool outputToConsole = false)
        {
            var stopwatch = NativeCreate(operation);

            return outputToConsole
                ? new ConsoleStopwatch(stopwatch)
                : new SingleUseStopwatch(stopwatch);
        }

        public IStopwatch CreateAndStore(MeasuredOperation operation, bool outputToConsole = false)
        {
            var stopwatch = Create(operation, outputToConsole);

            cachedStopwatches[operation] = stopwatch;

            return stopwatch;
        }

        public IStopwatch Get(MeasuredOperation operation)
        {
            if (cachedStopwatches.TryGetValue(operation, out var stopwatch))
            {
                return stopwatch;
            }

            return null;
        }

        public void Remove(MeasuredOperation operation)
        {
            cachedStopwatches.Remove(operation);
        }

        protected abstract IStopwatch NativeCreate(MeasuredOperation operation);

        private class SingleUseStopwatch : IStopwatch
        {
            private bool didStop;
            private bool didStart;

            public MeasuredOperation Operation { get; }

            protected IStopwatch InnerStopwatch { get; }

            public SingleUseStopwatch(IStopwatch innerStopwatch)
            {
                InnerStopwatch = innerStopwatch;
                Operation = innerStopwatch.Operation;
            }

            public virtual void Start()
            {
                if (didStart)
                    throw new InvalidOperationException("You can not start a stopwatch more than once");

                didStart = true;

                InnerStopwatch.Start();
            }

            public virtual void Stop()
            {
                if (!didStart)
                    throw new InvalidOperationException("You need to start a stopwatch before stopping it");

                if (didStop)
                    throw new InvalidOperationException("You can not stop a stopwatch more than once");

                didStop = true;

                InnerStopwatch.Stop();
            }
        }

        private class ConsoleStopwatch : SingleUseStopwatch
        {
            private readonly Stopwatch stopwatch = new Stopwatch();

            public ConsoleStopwatch(IStopwatch innerStopwatch)
                : base(innerStopwatch) { }

            public override void Start()
            {
                stopwatch.Start();
                base.Start();
            }

            public override void Stop()
            {
                base.Stop();
                stopwatch.Stop();

                Console.WriteLine($"Operation {Operation} took {stopwatch.Elapsed} to complete");
            }
        }
    }
}
