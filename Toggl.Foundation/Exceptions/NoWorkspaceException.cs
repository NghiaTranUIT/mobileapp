using System;
namespace Toggl.Foundation.Exceptions
{
    public class NoWorkspaceException : Exception
    {
        public NoWorkspaceException()
        {
        }

        public NoWorkspaceException(string message) : base(message)
        {
        }
    }
}
