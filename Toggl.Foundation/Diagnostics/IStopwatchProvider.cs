namespace Toggl.Foundation.Diagnostics
{
    public interface IStopwatchProvider
    {
        IStopwatch Create(MeasuredOperation operation, bool outputToConsole = false);

        IStopwatch CreateAndStore(MeasuredOperation operation, bool outputToConsole = false);

        IStopwatch Get(MeasuredOperation operation);

        void Remove(MeasuredOperation operation);
    }
}
