using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using WeatherBot.BusinessLogic;
using WeatherBot.Helpers;
using WeatherBot.Services;

namespace WeatherBot.Bots
{
    public class WeatherBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly BotServices _botServices;
        protected readonly StateService _stateService;
        protected readonly Dialog _dialog;

        public WeatherBot(StateService stateService,BotServices botServices, T dialog)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));
            _dialog = dialog ?? throw new System.ArgumentNullException(nameof(dialog));
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await _dialog.Run(turnContext, _stateService.DialogStateAccessor, cancellationToken);
        }
    }
}
