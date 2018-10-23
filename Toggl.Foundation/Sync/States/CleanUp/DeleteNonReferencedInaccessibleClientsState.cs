using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States.CleanUp
{
    public sealed class DeleteNonReferencedInaccessibleClientsState : DeleteInaccessibleEntityState<IThreadSafeClient, IDatabaseClient>
    {
        private readonly IDataSource<IThreadSafeProject, IDatabaseProject> projectsDataSource;

        public DeleteNonReferencedInaccessibleClientsState(
            IDataSource<IThreadSafeClient, IDatabaseClient> clientsDataSource,
            IDataSource<IThreadSafeProject, IDatabaseProject> projectsDataSource
        ) : base(clientsDataSource)
        {
            Ensure.Argument.IsNotNull(projectsDataSource, nameof(projectsDataSource));
            this.projectsDataSource = projectsDataSource;
        }

        protected override IObservable<bool> SuitableForDeletion(IThreadSafeClient client)
            => projectsDataSource.GetAll(
                    project => isReferenced(client, project),
                    includeInaccessibleEntities: true)
                .Select(references => references.None());

        private bool isReferenced(IClient client, IProject project)
            => project.Id == client.Id;
    }
}
