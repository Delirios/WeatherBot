using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Streaming.Transport;

namespace WeatherBot.Services
{
    public class StateService
    {
        public ConversationState ConversationState { get; }
        public static string DialogStateId { get; } = $"{nameof(StateService)}.DialogState";
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public StateService(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            InitializeAccessors();
        }

        public void InitializeAccessors()
        {
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);
        }
    }
}
