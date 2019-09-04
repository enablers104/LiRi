// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogAndWelcomeBot{T}"/> class.
        /// </summary>
        /// <param name="conversationState">State of the conversation.</param>
        /// <param name="userState">State of the user.</param>
        /// <param name="dialog">The dialog.</param>
        /// <param name="logger">The logger.</param>
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger) : base(conversationState, userState, dialog, logger)
        {
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are added to the conversation when the base behavior of
        /// <see cref="M:Microsoft.Bot.Builder.ActivityHandler.OnConversationUpdateActivityAsync(Microsoft.Bot.Builder.ITurnContext{Microsoft.Bot.Schema.IConversationUpdateActivity},System.Threading.CancellationToken)" /> is used.
        /// If overridden, this could potentially send a greeting message to the user instead of waiting for the user to send a message first.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="membersAdded">A list of all the users that have been added in the conversation update.</param>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeCard = CreateAdaptiveCardAttachment();
                    var response = MessageFactory.Attachment(welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }

        // Load attachment from embedded resource.
        /// <summary>
        /// Creates the adaptive card attachment.
        /// </summary>
        /// <returns></returns>
        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = "CoreBot.Cards.welcomeCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }
    }
}
