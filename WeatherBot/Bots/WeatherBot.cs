using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using WeatherBot.BusinessLogic;
using WeatherBot.Services;

namespace WeatherBot.Bots
{
    public class WeatherBot : ActivityHandler
    {
        public BotServices _botServices;

        public WeatherBot(IWeatherService weatherService, BotServices botServices)
        {
            _botServices = botServices;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var message = "Починаю пошук погоди";
            await turnContext.SendActivityAsync(MessageFactory.Text(message, message));
            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);
            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();
            switch (topIntent.intent)
            {
                case "City":
                    await _botServices.GetWeather(turnContext, cancellationToken);
                    break;
            }
        }
    }
}
