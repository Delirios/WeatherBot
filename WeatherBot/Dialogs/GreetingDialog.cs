using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace WeatherBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {

        public GreetingDialog(string dialogId) : base(dialogId)
        {
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                GreetingStepAsync,
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.name"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> GreetingStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            string helloMessage =
                "Щоб шукати погоду, придумайте фразу з назвою міста. Наприклад: Please tell me what the weather in (Ваше місто). Ви можете звертатись до бота своїми словами.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(helloMessage));
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
