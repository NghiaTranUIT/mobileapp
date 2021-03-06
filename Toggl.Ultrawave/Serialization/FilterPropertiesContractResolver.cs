﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Toggl.Ultrawave.Serialization
{
    internal sealed class FilterPropertiesContractResolver : DefaultContractResolver
    {
        private readonly IList<IPropertiesFilter> filters;

        public FilterPropertiesContractResolver(IList<IPropertiesFilter> filters)
        {
            this.filters = filters;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            => filters
                .Where(filter => filter != null)
                .Aggregate(
                    base.CreateProperties(type, memberSerialization),
                    (acc, propertiesFilter) => propertiesFilter.Filter(acc));
    }
}
