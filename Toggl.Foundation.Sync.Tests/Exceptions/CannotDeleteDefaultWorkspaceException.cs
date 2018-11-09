using System;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.Tests.Exceptions
{
    public class CannotDeleteDefaultWorkspaceException : Exception
    {
        private const string defaultMessage =
            "It was not possible to delete the default workspace although it was missing in the desired server state.";

        public CannotDeleteDefaultWorkspaceException(Exception apiException)
            : base(defaultMessage, apiException)
        {
        }
    }
}
