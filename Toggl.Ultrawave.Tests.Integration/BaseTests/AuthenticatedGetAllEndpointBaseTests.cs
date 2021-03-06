﻿using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Toggl.Ultrawave.Tests.Integration.BaseTests
{
    public abstract class AuthenticatedGetAllEndpointBaseTests<T> : AuthenticatedGetEndpointBaseTests<List<T>>
    {
        [Fact, LogTestInfo]
        public async Task ReturnsAnEmptyListWhenThereIsNoTimeEntryOnTheServer()
        {
            var (togglClient, user) = await SetupTestUser();

            var timeEntries = await CallEndpointWith(togglClient);

            timeEntries.Should().NotBeNull();
            timeEntries.Should().BeEmpty();
        }
    }
}
