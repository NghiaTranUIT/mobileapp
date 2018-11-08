using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    public abstract class SelectableClientBaseViewModel
    {
        public string Name { get; set; }

        public override string ToString() => Name;
    }

    public sealed class SelectableClientViewModel : SelectableClientBaseViewModel
    {
        public long Id { get; }

        public SelectableClientViewModel(long id, string name)
        {
            Ensure.Argument.IsNotNull(Id, nameof(Id));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(name, nameof(name));

            Id = id;
            Name = name;
        }
    }

    public sealed class SelectableClientCreationViewModel : SelectableClientBaseViewModel
    {
        public SelectableClientCreationViewModel(string name)
        {
            Ensure.Argument.IsNotNullOrWhiteSpaceString(name, nameof(name));

            Name = name;
        }
    }
}
