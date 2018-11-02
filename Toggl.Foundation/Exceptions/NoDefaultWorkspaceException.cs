using System;

namespace Toggl.Foundation.Exceptions
{
    public sealed class NoDefaultWorkspaceException : Exception
    {
        public NoDefaultWorkspaceException() : base()
        {
        }

        public NoDefaultWorkspaceException(string message) : base(message)
        {
        }
    }
}
