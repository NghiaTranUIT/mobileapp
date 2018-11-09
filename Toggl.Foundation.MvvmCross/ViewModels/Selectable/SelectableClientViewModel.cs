using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    public abstract class SelectableClientBaseViewModel
    {
        public string Name { get; set; }
        public bool Selected { get; set; }

        public SelectableClientBaseViewModel(string name, bool selected)
        {
            Ensure.Argument.IsNotNullOrWhiteSpaceString(name, nameof(name));
            Name = name;
            Selected = selected;
        }

        public override string ToString() => Name;
    }

    public sealed class SelectableClientViewModel : SelectableClientBaseViewModel
    {
        public long Id { get; }

        public SelectableClientViewModel(long id, string name, bool selected)
            : base(name, selected)
        {
            Ensure.Argument.IsNotNull(Id, nameof(Id));
            Id = id;
        }
    }

    public sealed class SelectableClientCreationViewModel : SelectableClientBaseViewModel
    {
        public SelectableClientCreationViewModel(string name)
            : base(name, false)
        {
        }
    }
}
