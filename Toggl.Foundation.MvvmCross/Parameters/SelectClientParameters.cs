using System;

namespace Toggl.Foundation.MvvmCross.Parameters
{
    public sealed class SelectClientParameters
    {
        public long WorkspaceId { get; set; }

        public static SelectClientParameters WithIds(long workspaceId)
            => new SelectClientParameters
            {
                WorkspaceId = workspaceId,
            };
    }
}
