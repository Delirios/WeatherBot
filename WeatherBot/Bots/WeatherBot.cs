using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using PluralsightBot.Helpers;
using WeatherApp.Domain;
using WeatherBot.BusinessLogic;
using WeatherBot.Services;

namespace WeatherBot.Bots
{
    public class WeatherBot : ActivityHandler
    {

        public IWeatherService _weatherService;
        public BotServices _botServices;

        public WeatherBot(IWeatherService weatherService, BotServices botServices)
        {
            _weatherService = weatherService;
            _botServices = botServices;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);
            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();
            switch (topIntent.intent)
            {
                case "City":
                    await GetWeather(turnContext, cancellationToken);
                    break;
            }
        }

        private async Task GetWeather(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                string imageUrl = "";
                var result = await _botServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);
                var token = result.Entities.FindTokens("CityType").First();
                Regex rgx = new Regex("[^a-zA-Zа-щА-ЩЬьЮюЯяЇїІіЄєҐґ0-9 - ]");
                var cityName = rgx.Replace(token.ToString(), "").Trim();
                var data = await _weatherService.ShowWeatherDataAsync(cityName);

                foreach (var item in data.weather)
                {
                    imageUrl = item.icon;
                }

                var imagePath = Path.Combine(Environment.CurrentDirectory, $"./Resources/Images/{imageUrl}.png");
                var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));
                var card = new HeroCard
                {
                    Title = data.cityname,

                    Text = $"Температура: {data.main.temp}" +
                           Environment.NewLine +
                           $"Відчувається як: { data.main.feels_like }" +
                           Environment.NewLine +
                           $"Швидкість вітру: {data.wind.speed} м/c",

                    Images = new List<CardImage>() { new CardImage($"data:image/png;base64,{imageData}") },

                    Buttons = new List<CardAction>()
                    {
                        new CardAction(ActionTypes.OpenUrl, "Перейти на сайт", null, "Перейти на сайт", "Перейти на сайт",
                            "https://allweather.azurewebsites.net"),
                    },

                };
                var response = MessageFactory.Attachment(card.ToAttachment());
                await turnContext.SendActivityAsync(response, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await turnContext.SendActivityAsync(MessageFactory.Text(e.Message, e.Message), cancellationToken);
            }
        }
    }
}
