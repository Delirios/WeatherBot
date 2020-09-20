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

    protected readonly Dialog _dialog;


    public WeatherBot(IWeatherService weatherService, BotServices botServices, Dialog dialog)
    {
        _botServices = botServices;
        _dialog = dialog;
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        await _dialog.Run(turnContext, cancellationToken);
    }
    }
}
