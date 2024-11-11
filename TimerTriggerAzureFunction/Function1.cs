using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TimerTriggerAzureFunction
{
    public class TimerTriggerFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _client = new HttpClient();

        public TimerTriggerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerFunction>();
        }

        [Function("TimerTriggerFunction")]
        public async Task Run([TimerTrigger("0/30 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            try
            {
                var response = await _client.GetAsync("https://dog.ceo/api/breeds/image/random/5");

                if (response.IsSuccessStatusCode)
                {
                    var contentString = await response.Content.ReadAsStringAsync();
                    var dogImageResponse = JsonSerializer.Deserialize<DogImageResponse>(contentString);

                    if (dogImageResponse.Status.Equals("success"))
                    {
                        _logger.LogInformation($"Fetched {dogImageResponse.Message.Count()} dog images.");
                        foreach (var image in dogImageResponse.Message)
                        {
                            _logger.LogInformation(image); // Log each image URL
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to fetch dog images. API response status: {dogImageResponse.Status}");
                    }
                }
                else
                {
                    _logger.LogError($"Error fetching dog images. HTTP Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
            }
        }
    }

    public class DogImageResponse
    {
        public string Status { get; set; }
        public List<string> Message { get; set; }
    }
}