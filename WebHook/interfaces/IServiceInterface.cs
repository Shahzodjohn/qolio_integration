using System.Text.Json;

namespace WebHook.interfaces
{
    public interface IServiceInterface
    {
        Task SaveToDb(JsonElement json);
    }
}
