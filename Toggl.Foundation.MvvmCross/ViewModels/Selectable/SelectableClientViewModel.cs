using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    public sealed class SelectableClientViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsCreation { get; set; }

        public SelectableClientViewModel(long id, string name, bool isCreation)
        {
            Ensure.Argument.IsNotNull(Id, nameof(Id));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(name, nameof(name));

            Id = id;
            Name = name;
            IsCreation = isCreation;
        }

        public override string ToString() => Name;
    }
}
