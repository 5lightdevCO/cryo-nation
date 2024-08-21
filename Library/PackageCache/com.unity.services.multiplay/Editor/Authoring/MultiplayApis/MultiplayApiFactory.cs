using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Multiplay.Authoring.Core.MultiplayApi;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.Allocations.Apis.Allocations;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.Allocations.Http;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.BuildConfigs.Apis.BuildConfigurations;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.Builds.Apis.Builds;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.Fleets.Apis.Fleets;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.Logs.Apis.Logs;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.Servers;
using Unity.Services.Multiplay.Authoring.Editor.AdminApis.Servers.Apis.Servers;
using Unity.Services.Multiplay.Authoring.Editor.Shared.Clients;
using UnityEditor;

using ICoreAccessTokens = Unity.Services.Core.Editor.IAccessTokens;
using Task = UnityEditor.VersionControl.Task;

namespace Unity.Services.Multiplay.Authoring.Editor.MultiplayApis
{
    class MultiplayApiFactory : IFleetApiFactory, IBuildsApiFactory, IBuildConfigApiFactory, IAllocationApiFactory, IServersApiFactory, ILogsApiFactory
    {
        const string k_StgUrl = "https://staging.services.unity.com";

        readonly ICoreAccessTokens m_AccessTokens;
        readonly IEnvironmentsApi m_EnvironmentsApi;

        public MultiplayApiFactory(ICoreAccessTokens accessTokens, IEnvironmentsApi environmentsApi)
        {
            m_AccessTokens = accessTokens;
            m_EnvironmentsApi = environmentsApi;
        }

        async Task<IFleetApi> IFleetApiFactory.Build()
        {
            var(config, basePath, headers) = await Authenticate();
            return new FleetApi(config,
                new FleetsApiClient(
                    new AdminApis.Fleets.Http.HttpClient(),
                    new AdminApis.Fleets.Configuration(basePath, null, null, headers)));
        }

        async Task<IBuildsApi> IBuildsApiFactory.Build()
        {
            var(config, basePath, headers) = await Authenticate();
            return new BuildsApi(config,
                new BuildsApiClient(
                    new AdminApis.Builds.Http.HttpClient(),
                    new AdminApis.Builds.Configuration(basePath, null, null, headers)));
        }

        async Task<IBuildConfigApi> IBuildConfigApiFactory.Build()
        {
            var(config, basePath, headers) = await Authenticate();
            return new BuildConfigApi(config, //TODO: ++ see this
                new BuildConfigurationsApiClient(
                    new AdminApis.BuildConfigs.Http.HttpClient(),
                    new AdminApis.BuildConfigs.Configuration(basePath, null, null, headers)));
        }

        async Task<IAllocationApi> IAllocationApiFactory.Build()
        {
            var(config, basePath, headers) = await Authenticate();
            return new AllocationApi(config,
                new AllocationsApiClient(
                    new HttpClient(),
                    new AdminApis.Allocations.Configuration(basePath, null, null, headers)));
        }

        async Task<IServersApi> IServersApiFactory.Build()
        {
            var(config, basePath, headers) = await Authenticate();
            return new ServersApi(config, new ServersApiClient(
                new AdminApis.Servers.Http.HttpClient(),
                new Configuration(basePath, null, null, headers)));
        }

        async Task<ILogsApi> ILogsApiFactory.Build()
        {
            var(config, basePath, headers) = await Authenticate();
            return new LogsApi(config, new LogsApiClient(
                new AdminApis.Logs.Http.HttpClient(),
                new AdminApis.Logs.Configuration(basePath, null, null, headers)));
        }

        async Task<(ApiConfig, string, IDictionary<string, string>)> Authenticate()
        {
            if (m_EnvironmentsApi.ActiveEnvironmentId == null)
            {
                throw new EnvironmentNotFoundException("Authentication failed. Please make sure the environment in Project Settings > Environments has been set.");
            }

            var config = new ApiConfig(
                Guid.Parse(CloudProjectSettings.projectId),
                m_EnvironmentsApi.ActiveEnvironmentId.Value
            );

            var gatewayToken = await m_AccessTokens.GetServicesGatewayTokenAsync();
            return (config, CloudEnvironmentConfigProvider.IsStaging() ? k_StgUrl : null, new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {gatewayToken}" }
            });
        }

        record ApiConfig(Guid ProjectId, Guid EnvironmentId) : IMultiplayApiConfig;
    }
}
