using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherBot.Services
{
    public class BotServices
    {
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

        public LuisRecognizer Dispatch { get; private set; }

    }
}
