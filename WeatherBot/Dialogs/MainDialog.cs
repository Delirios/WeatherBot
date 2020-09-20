using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using WeatherBot.Services;

namespace WeatherBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly BotServices _botServices;

        public MainDialog(BotServices botServices) : base(nameof(MainDialog))
        {

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

            AddDialog(new GreetingDialog($"{nameof(GreetingDialog)}.greeting", _botServices));
            AddDialog(new WeatherDialog($"{nameof(WeatherDialog)}.weather", _botServices));

            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
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
