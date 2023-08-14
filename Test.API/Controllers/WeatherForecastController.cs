using ImageRenderPackage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Test.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IImageHelper _imageHelper;

        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
            _imageHelper = new ImageHelper(cache);
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public async Task<IActionResult> SaveImage(IFormFile file)
        {
            var result = await _imageHelper.SaveImageToLocalAsync(file, Path.Combine(Directory.GetCurrentDirectory(), "Images"), "packageImage");

            var deleteResutl = await _imageHelper.DeleteImageFromLocalAsync(result.Item2, false);

            await _imageHelper.LoadDeletedImageFromCache(result.Item2);

            return Ok((result.Item2, deleteResutl.Item2));

        }
    }
}