using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain;

namespace WeatherBot.BusinessLogic
{
    public interface IWeatherService
    {
        Task<Root> ShowWeatherDataAsync(string CityName);
    }
}
