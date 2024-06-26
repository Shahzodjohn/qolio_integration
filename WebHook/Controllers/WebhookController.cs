﻿using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
using WebHook.interfaces;

namespace WebHook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        [HttpGet("get-jivo-data")]
        public async Task<IActionResult> GetJivoData()
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
