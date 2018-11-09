using Microsoft.Reactive.Testing;
using System.Linq;

namespace Toggl.Foundation.Tests.Extensions
{
    public static class ITestableObserverExtensions
    {
        public static T LastValue<T>(this ITestableObserver<T> observer)
            => observer.Messages.Last().Value.Value;
    }
}
