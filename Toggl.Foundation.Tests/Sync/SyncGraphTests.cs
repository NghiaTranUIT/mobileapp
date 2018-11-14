using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reflection;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Sync;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Xunit;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class SyncGraphTests
    {
        public sealed class TheSyncGraph
        {
            private readonly StateMachineEntryPoints entryPoints;
            private readonly List<object> states;
            private readonly Dictionary<IStateResult, (object State, Type ParameterType)> transitions;

            public TheSyncGraph()
            {
                var configurator = new Configurator();
                entryPoints = new StateMachineEntryPoints();

                TogglSyncManager.ConfigureTransitions(
                    configurator,
                    Substitute.For<ITogglDatabase>(),
                    Substitute.For<ITogglApi>(),
                    Substitute.For<ITogglDataSource>(),
                    Substitute.For<IRetryDelayService>(),
                    Substitute.For<IScheduler>(),
                    Substitute.For<ITimeService>(),
                    Substitute.For<IAnalyticsService>(),
                    entryPoints,
                    Substitute.For<IObservable<Unit>>(),
                    Substitute.For<ISyncStateQueue>()
                );

                transitions = configurator.Transitions;
                states = configurator.AllDistinctStatesInOrder;
            }

            [Fact, LogIfTooSlow]
            public void HasNoLooseEnds()
            {
                var stateResults = getAllStateResults();

                foreach (var (state, namedResults) in stateResults)
                {
                    foreach (var (result, name) in namedResults)
                    {
                        if (transitions.ContainsKey(result))
                            continue;

                        throw new Exception($"No transition found for state result {state.GetType().Name}.{name}");
                    }
                }
            }

            private List<(object, List<(IStateResult Result, string Name)>)> getAllStateResults()
            {
                return states
                    .Append(entryPoints)
                    .Select(state => (state, state.GetType()
                        .GetProperties()
                        .Where(isStateResultProperty)
                        .Select(p => ((IStateResult)p.GetValue(state), p.Name))
                        .ToList()))
                    .ToList();
            }

            private static bool isStateResultProperty(PropertyInfo p)
            {
                return typeof(IStateResult).IsAssignableFrom(p.PropertyType);
            }
        }

        private sealed class Configurator : ITransitionConfigurator
        {
            public List<object> AllDistinctStatesInOrder { get; } = new List<object>();

            public Dictionary<IStateResult, (object State, Type ParameterType)> Transitions { get; }
                = new Dictionary<IStateResult, (object, Type)>();

            public void ConfigureTransition(IStateResult result, ISyncState state)
            {
                addToListIfNew(state);
                Transitions.Add(result, (state, null));
            }

            public void ConfigureTransition<T>(StateResult<T> result, ISyncState<T> state)
            {
                addToListIfNew(state);
                Transitions.Add(result, (state, typeof(T)));
            }

            private void addToListIfNew(object state)
            {
                if (AllDistinctStatesInOrder.Contains(state))
                    return;

                AllDistinctStatesInOrder.Add(state);
            }
        }

    }
}
