using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
using WebHook.interfaces;

namespace WebHook.Controllers
{
    [ApiController]
    [Route("[controller/hook]")]
    public class WebhookController : ControllerBase
    {
        public async Task<IActionResult> GetJivoData(JsonElement json)
        {
            Log.Information("New conversation...");

            Console.WriteLine("New conversation...");
            return Ok();
            //var requestBody = await context.Request.ReadFromJsonAsync<JsonElement>();

            //using (var serviceScope = app.Services.CreateScope())
            //{
            //    var services = serviceScope.ServiceProvider;

            //    var myDependency = services.GetRequiredService<IServiceInterface>();

            //    await myDependency.SaveToDb(requestBody);
            //}
        }
    }
}
