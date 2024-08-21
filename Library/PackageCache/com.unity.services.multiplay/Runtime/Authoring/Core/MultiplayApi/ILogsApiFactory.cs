using System.Threading.Tasks;

namespace Unity.Services.Multiplay.Authoring.Core.MultiplayApi
{
    interface ILogsApiFactory
    {
        Task<ILogsApi> Build();
    }
}
