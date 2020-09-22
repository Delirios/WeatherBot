using AdaptiveCards;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Helpers;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using WeatherBot.BusinessLogic;
using WeatherBot.Models;

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

        private const string subscriptionKey = "YOUR_SUBSCRIPTION_KEY";

        private const string endpoint = "https://api.cognitive.microsofttranslator.com";


        public async Task<string> Translator(ITurnContext turnContext, CancellationToken cancellation)
        {
            // Input and output languages are defined as parameters.
            string route = "/translate?api-version=3.0&from=uk&to=en";
            //string textToTranslate = "Погода Львів";
            string region = "westeurope";
            object[] body = new object[] { new { Text = turnContext.Activity.Text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var rootList = new List<Translation>();
                string translationResult = "";
               
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                var result = await response.Content.ReadAsStringAsync();

                var root = JsonConvert.DeserializeObject<List<TranslationResult>>(result);

                foreach (var item in root)
                {
                    rootList = item.translations;
                }
                foreach (var item in rootList)
                {
                    translationResult = item.text;
                }

                return translationResult;
            }

        }
    }
}
