using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSE.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly WeatherForecastStreamService _weatherForecastStreamService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(WeatherForecastStreamService weatherForecastStreamService, ILogger<WeatherForecastController> logger)
        {
            _weatherForecastStreamService = weatherForecastStreamService;
            _logger = logger;
        }

        [HttpGet("test")]
        public string Test()
        {
            return "test";
        }

        [HttpGet]
        public async Task Get( CancellationToken cancellationToken)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.StatusCode = 200;

            while (true && !cancellationToken.IsCancellationRequested)
            {
                while (!_weatherForecastStreamService.HasMessage)
                {
                    await Task.Delay(1000);
                }

                while (_weatherForecastStreamService.HasMessage)
                {
                    var mBytes = _weatherForecastStreamService.Read();

                    await Response.Body.WriteAsync(mBytes, 0, mBytes.Length);
                    await Response.Body.FlushAsync();
                }

            }
        }
    }


    public class WeatherForecastStreamService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly Queue<string> messages = new Queue<string>();

        public bool HasMessage => messages.Count > 0;


        public WeatherForecastStreamService()
        {
            Task.Run(() =>
            {
                while (true)
                {

                    var messages = GetRandom();

                    this.Send(messages);
                    Task.Delay(5000).GetAwaiter().GetResult();
                }
            });
        }

        private void Send(WeatherForecast[] message)
        {
            messages.Enqueue(JsonSerializer.Serialize(message, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }

        public byte[] Read()
        {
            var m = messages.Dequeue();
            return ASCIIEncoding.ASCII.GetBytes($"data:{m}\n\n");
        }

        private WeatherForecast[] GetRandom()
        {

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

    }
}
