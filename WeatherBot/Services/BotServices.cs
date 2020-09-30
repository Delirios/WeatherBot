using AdaptiveCards;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace WeatherBot.Services
{
    public class BotServices
    {
        public LuisRecognizer Dispatch { get; private set; }

        public BotServices(IConfiguration configuration)
        {
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

        public Attachment CreateAdaptiveCardUsingJson(string json)
        {
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(json).Card
            };
        }

    }
}
