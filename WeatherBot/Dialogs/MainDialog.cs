using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.CognitiveServices.Speech.Audio;
using WeatherBot.BusinessLogic;
using WeatherBot.Services;

namespace WeatherBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly IWeatherService _weatherService;
        private readonly StateService _stateService;
        private readonly RecognitionServices _recognitionServices;

        public MainDialog(BotServices botServices, IWeatherService weatherService, StateService stateService, RecognitionServices recognitionServices) : base(nameof(MainDialog))
        {
            _recognitionServices = recognitionServices;
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
            string result = "";
            if (stepContext.Context.Activity.Attachments != null)
            {
                try
                {
                    var message = "Розпізнавання голосового повідомлення";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(message, message));
                    result = await _recognitionServices.VoiceMessageRecognitionAsync(stepContext.Context, cancellationToken);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            else
            {
                result = await _recognitionServices.Translator(stepContext.Context, cancellationToken);
            }

            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.\
            Regex rgx = new Regex("[^a-zA-Zа-щА-ЩЬьЮюЯяЇїІіЄєҐґ0-9 - ]");
            var modifiedResult = rgx.Replace(result, "").Trim();
            stepContext.Context.Activity.Text = modifiedResult;

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
