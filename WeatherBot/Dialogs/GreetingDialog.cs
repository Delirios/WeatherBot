using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using WeatherBot.Services;

namespace WeatherBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        private readonly StateService _stateService;

        public GreetingDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService;
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.name"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            string helloMessage =
                "Щоб шукати погоду, придумайте фразу з назвою міста. Наприклад: Please tell me what the weather in (Ваше місто). Ви можете звертатись до бота своїми словами.";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(helloMessage));
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
