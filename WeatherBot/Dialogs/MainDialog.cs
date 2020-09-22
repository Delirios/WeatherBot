using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using WeatherBot.BusinessLogic;
using WeatherBot.Services;

namespace WeatherBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly IWeatherService _weatherService;
        private readonly StateService _stateService;

        public MainDialog(BotServices botServices, IWeatherService weatherService, StateService stateService) : base(nameof(MainDialog))
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

            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting",_stateService));
            AddDialog(new WeatherDialog($"{nameof(MainDialog)}.weather", _botServices, _weatherService, _stateService));
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));


            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = await _botServices.Translator(stepContext.Context, cancellationToken);
            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.\
            stepContext.Context.Activity.Text = result; 
            Regex rgx = new Regex("[^a-zA-Zа-щА-ЩЬьЮюЯяЇїІіЄєҐґ0-9 - ]");
            var res = rgx.Replace(result, "").Trim();

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            // Top intent tell us which cognitive service to use.
            var TopIntent = recognizerResult.GetTopScoringIntent();

            switch (TopIntent.intent)
            {
                case "GreetingIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null,
                        cancellationToken);
                case "CityIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.weather", null,
                        cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Я не розумію, що ви маєте на увазі"), cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
