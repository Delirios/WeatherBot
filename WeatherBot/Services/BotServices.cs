using AdaptiveCards;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Helpers;
using Microsoft.Bot.Builder;
using WeatherBot.BusinessLogic;

namespace WeatherBot.Services
{
    public class BotServices
    {
        public IWeatherService _weatherService;
        public LuisRecognizer Dispatch { get; private set; }

        public BotServices(IConfiguration configuration, IWeatherService weatherService)
        {
            _weatherService = weatherService;
            
            
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com");
            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true,
                }
            };
            Dispatch = new LuisRecognizer(recognizerOptions);
        }

        public async Task GetWeather(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var result = await Dispatch.RecognizeAsync(turnContext, cancellationToken);
                var token = result.Entities.FindTokens("CityType").First();
                Regex rgx = new Regex("[^a-zA-Zа-щА-ЩЬьЮюЯяЇїІіЄєҐґ0-9 - ]");
                var cityName = rgx.Replace(token.ToString(), "").Trim();
                var data = await _weatherService.ShowWeatherDataAsync(cityName);

                var template = new AdaptiveCardTemplate(File.ReadAllText("./Resources/adaptiveCard.json"));
                var expanded = template.Expand(data);

                var attachment = CreateAdaptiveCardUsingJson(expanded);
                var response = MessageFactory.Attachment(attachment);
                await turnContext.SendActivityAsync(response);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await turnContext.SendActivityAsync(MessageFactory.Text(e.Message, e.Message), cancellationToken);
            }
        }
        private Attachment CreateAdaptiveCardUsingJson(string json)
        {
            return new Attachment
            {

                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(json).Card
            };
        }

    }
}
