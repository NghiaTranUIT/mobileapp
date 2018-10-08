using System;

namespace Toggl.Foundation.Exceptions
{
    public sealed class NotAuthorizedException : Exception
    {
        public NotAuthorizedException(string message) : base(message)
        {
        }
    }
}
