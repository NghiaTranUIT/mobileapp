﻿using System;
using Realms;
using Toggl.PrimeRadiant.Models;

namespace Toggl.PrimeRadiant.Realm
{
    internal partial class RealmTag : RealmObject, IDatabaseTag
    {
        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        public RealmWorkspace RealmWorkspace { get; set; }

        public long WorkspaceId => RealmWorkspace?.Id ?? 0;

        public IDatabaseWorkspace Workspace => RealmWorkspace;

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public bool IsInaccessible => Workspace.IsInaccessible;
    }
}
