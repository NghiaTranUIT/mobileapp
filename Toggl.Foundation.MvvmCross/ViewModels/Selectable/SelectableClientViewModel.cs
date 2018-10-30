using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    public sealed class SelectableClientViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public SelectableClientViewModel(long id, string name)
        {
            Ensure.Argument.IsNotNull(Id, nameof(Id));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(name, nameof(name));

            Id = id;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
