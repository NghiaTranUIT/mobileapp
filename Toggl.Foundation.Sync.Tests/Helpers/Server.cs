using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Foundation.Sync.Tests.Extensions;
using Toggl.Foundation.Sync.Tests.State;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Sync.Tests
{
    public sealed class Server
    {
        public ITogglApi Api { get; }

        public ServerState InitialServerState { get; }

        private Server(ITogglApi api, ServerState initialServerState)
        {
            Api = api;
            InitialServerState = initialServerState;
        }

        public static async Task<Server> Create()
        {
            IUser user = null;
            do
            {
                if (user != null) await Task.Delay(TimeSpan.FromSeconds(1));
                user = await Ultrawave.Tests.Integration.User.Create();
            } while (user.DefaultWorkspaceId.HasValue == false);

            var credentials = Credentials.WithApiToken(user.ApiToken);
            var api = Ultrawave.Tests.Integration.Helper.TogglApiFactory.TogglApiWith(credentials);

            var defaultWorkspace = await api.Workspaces.GetById(user.DefaultWorkspaceId.Value);
            var preferences = await api.Preferences.Get();

            var initialServerState = new ServerState(
                user,
                workspaces: new[] { defaultWorkspace },
                preferences: preferences);

            return new Server(api, initialServerState);
        }

        public async Task<ServerState> PullCurrentState()
        {
            var user = await Api.User.Get();
            var clients = await Api.Clients.GetAll();
            var projects = await Api.Projects.GetAll();
            var preferences = await Api.Preferences.Get();
            var tags = await Api.Tags.GetAll();
            var tasks = await Api.Tasks.GetAll();
            var timeEntries = await Api.TimeEntries.GetAll();
            var workspaces = await Api.Workspaces.GetAll();

            return new ServerState(
                user, clients, projects, preferences, tags, tasks, timeEntries, workspaces);
        }

        public async Task<ServerState> Push(ServerState state)
        {
            var user = state.User;
            var clients = (IEnumerable<IClient>)state.Clients;
            var projects = (IEnumerable<IProject>)state.Projects;
            var preferences = state.Preferences;
            var tags = (IEnumerable<ITag>)state.Tags;
            var tasks = (IEnumerable<ITask>)state.Tasks;
            var timeEntries = (IEnumerable<ITimeEntry>)state.TimeEntries;
            var workspaces = (IEnumerable<IWorkspace>)state.Workspaces;

            // push workspaces
            workspaces = await workspaces.Select(workspace
                    => Api.Workspaces.Create(workspace)
                        .Do(serverWorkspace =>
                        {
                            user = workspace.Id == user.DefaultWorkspaceId
                                ? user.With(defaultWorkspaceId: serverWorkspace.Id)
                                : user;
                            clients = clients.Select(client => client.WorkspaceId == workspace.Id
                                ? client.With(workspaceId: serverWorkspace.Id)
                                : client);
                            projects = projects.Select(project => project.WorkspaceId == workspace.Id
                                ? project.With(workspaceId: serverWorkspace.Id)
                                : project);
                            tags = tags.Select(tag => tag.WorkspaceId == workspace.Id
                                ? tag.With(workspaceId: serverWorkspace.Id)
                                : tag);
                            tasks = tasks.Select(task => task.WorkspaceId == workspace.Id
                                ? task.With(workspaceId: serverWorkspace.Id)
                                : task);
                            timeEntries = timeEntries.Select(timeEntry => timeEntry.WorkspaceId == workspace.Id
                                ? timeEntry.With(workspaceId: serverWorkspace.Id)
                                : timeEntry);
                        }))
                .Merge()
                .ToList();

            // push user
            user = await Api.User.Update(user)
                .Do(serverUser =>
                {
                    tasks = tasks.Select(task => task.UserId == user.Id
                        ? task.With(userId: serverUser.Id)
                        : task);
                });

            // push preferences
            preferences = await Api.Preferences.Update(preferences);

            // push tags
            tags = await tags.Select(tag
                    => Api.Tags.Create(tag)
                        .Do(serverTag =>
                        {
                            timeEntries = timeEntries.Select(timeEntry => timeEntry.TagIds.Contains(tag.Id)
                                ? timeEntry.With(tagIds:
                                    New<IEnumerable<long>>.Value(timeEntry.TagIds.Select(id => id == tag.Id ? serverTag.Id : id)))
                                : timeEntry);
                        }))
                .Merge()
                .ToList();

            // push clients
            clients = await clients.Select(client
                    => Api.Clients.Create(client)
                        .Do(serverClient =>
                        {
                            projects = projects.Select(project => project.ClientId == client.Id
                                ? project.With(clientId: serverClient.Id)
                                : project);
                        }))
                .Merge()
                .ToList();

            // push projects
            projects = await projects.Select(project
                    => Api.Projects.Create(project)
                        .Do(serverProject =>
                        {
                            tasks = tasks.Select(task => task.ProjectId == project.Id
                                ? task.With(projectId: serverProject.Id)
                                : task);
                            timeEntries = timeEntries.Select(timeEntry => timeEntry.ProjectId == project.Id
                                ? timeEntry.With(projectId: serverProject.Id)
                                : timeEntry);
                        }))
                .Merge()
                .ToList();

            // push tasks
            tasks = await tasks.Select(task
                    => Api.Tasks.Create(task)
                        .Do(serverTask =>
                        {
                            timeEntries = timeEntries.Select(timeEntry => timeEntry.TaskId == task.Id
                                ? timeEntry.With(taskId: serverTask.Id)
                                : timeEntry);
                        }))
                .Merge()
                .ToList();

            // push time entries
            timeEntries = await timeEntries.Select(Api.TimeEntries.Create).Merge().ToList();

            return new ServerState(
                user, clients, projects, preferences, tags, tasks, timeEntries, workspaces);
        }
    }
}
