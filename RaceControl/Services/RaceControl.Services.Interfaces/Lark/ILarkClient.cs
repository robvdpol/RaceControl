using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.Lark
{
    public interface ILarkClient
    {
        ILarkRequest NewRequest(string collection, string id = null);

        Task<TResponse> GetItemAsync<TResponse>(ILarkRequest request);

        Task<ILarkCollection<TResponse>> GetCollectionAsync<TResponse>(ILarkRequest request);
    }
}