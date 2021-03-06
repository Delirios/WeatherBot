﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using WeatherBot.BusinessLogic;
using WeatherBot.Services;

namespace WeatherBot.Dialogs
{
    public class WeatherDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly IWeatherService _weatherService;
        private readonly StateService _stateService;

        public WeatherDialog(string dialogId, BotServices botServices, IWeatherService weatherService, StateService stateService) : base(dialogId)
        {
            _stateService = stateService;
            _weatherService = weatherService;
            _botServices = botServices;
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(WeatherDialog)}.mainFlow", waterfallSteps));
            InitialDialogId = $"{nameof(WeatherDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var message = "Починаю пошук погоди";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(message, message));

            await GetWeather(stepContext.Context, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        public async Task GetWeather(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _botServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);
                Regex rgx = new Regex("[^a-zA-Zа-щА-ЩЬьЮюЯяЇїІіЄєҐґ0-9 - ]");
                var token = result.Entities.FindTokens("CityType").First();

                var cityName = rgx.Replace(token.ToString(), "").Trim();
                var data = await _weatherService.ShowWeatherDataAsync(cityName);

                var template = new AdaptiveCardTemplate(File.ReadAllText("./Resources/adaptiveCard.json"));
                var expanded = template.Expand(data);

                var attachment = _botServices.CreateAdaptiveCardUsingJson(expanded);
                var response = MessageFactory.Attachment(attachment);
                await turnContext.SendActivityAsync(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await turnContext.SendActivityAsync(MessageFactory.Text(e.Message, e.Message), cancellationToken);
            }
        }

    }
}
