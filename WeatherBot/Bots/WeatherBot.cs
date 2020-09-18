using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Rendering;
using AdaptiveCards.Templating;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
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

                var template = new AdaptiveCardTemplate(File.ReadAllText("./Resources/adaptiveCard.json"));
                var expanded = template.Expand(data);

                var attachment = CreateAdaptiveCardUsingJsons(expanded);
                var response = MessageFactory.Attachment(attachment);
                await turnContext.SendActivityAsync(response);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await turnContext.SendActivityAsync(MessageFactory.Text(e.Message, e.Message), cancellationToken);
            }
        }
        private Attachment CreateAdaptiveCardUsingSdk()
        {
            var card = new AdaptiveCard();
            card.Body.Add(new AdaptiveTextBlock() { Text = "Colour", Size = AdaptiveTextSize.Medium, Weight = AdaptiveTextWeight.Bolder });
            card.Body.Add(new AdaptiveChoiceSetInput()
            {
                Id = "Colour",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices = new List<AdaptiveChoice>(new[] {
                    new AdaptiveChoice() { Title = "Red", Value = "RED" },
                    new AdaptiveChoice() { Title = "Green", Value = "GREEN" },
                    new AdaptiveChoice() { Title = "Blue", Value = "BLUE" } })
            });
            card.Body.Add(new AdaptiveTextBlock() { Text = "Registration number:", Size = AdaptiveTextSize.Medium, Weight = AdaptiveTextWeight.Bolder });
            card.Body.Add(new AdaptiveTextInput() { Style = AdaptiveTextInputStyle.Text, Id = "RegistrationNumber" });
            card.Actions.Add(new AdaptiveSubmitAction() { Title = "Submit" });
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        private Attachment CreateAdaptiveCardUsingJson()
        {
            return new Attachment
            {
                
                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(File.ReadAllText("./Resources/adaptiveCard.json"))
            };
        }
        private Attachment CreateAdaptiveCardUsingJsons(string json)
        {
            return new Attachment
            {

                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(json).Card
            };
        }

    }
}
