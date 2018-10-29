namespace Toggl.Foundation.Diagnostics
{
    public interface IStopwatch
    {
        void Start();

        void Stop();

        MeasuredOperation Operation { get; }
    }
}
