using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    public abstract class SelectableClientBaseViewModel
    {
        public string Name { get; set; }

        public SelectableClientBaseViewModel(string name)
        {
            Ensure.Argument.IsNotNullOrWhiteSpaceString(name, nameof(name));
            Name = name;
        }

        public override string ToString() => Name;
    }

    public sealed class SelectableClientViewModel : SelectableClientBaseViewModel
    {
        public long Id { get; }

        public bool Selected { get; set; }

        public SelectableClientViewModel(long id, string name)
            : base(name)
        {
            Ensure.Argument.IsNotNull(Id, nameof(Id));
            Id = id;
        }
    }

    public sealed class SelectableClientCreationViewModel : SelectableClientBaseViewModel
    {
        public SelectableClientCreationViewModel(string name) : base(name)
        {
        }
    }
}
