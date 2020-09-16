using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeatherApp.Domain;

namespace WeatherBot.BusinessLogic
{
    public class WeatherService : IWeatherService
    {
        public async Task<Root> GetWeatherDataAsync(string url)
        {
            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse = await webRequest.GetResponseAsync();
            using (Stream stream = webResponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream);
                string responceFromServer = await reader.ReadToEndAsync();
                Root result = JsonConvert.DeserializeObject<Root>(responceFromServer);

                return result;
            }
        }

        public async Task<Root> ShowWeatherDataAsync(string CityName)
        {
            try
            {
                string url = "https://weatherservicewebapi.azurewebsites.net/api/Weather/" + CityName;

                Root root = new Root();
                {
                    root = await GetWeatherDataAsync(url);
                }
                return root;
            }
            catch
            {
                throw new Exception("Некоректно введене місто");
            }
        }
    }
}
