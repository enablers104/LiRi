using System;
using AdaptiveCards;
using System.Threading;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.BotBuilderSamples.DialogModels;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class AccountDialog : CancelAndHelpDialog
    {
        private const string TitleStepMsgText = "What is your title?";
        private const string FirstNameStepMsgText = "What is your first name?";
        private const string LastNameStepMsgText = "What is your last name?";
        private const string IdentityNumberStepMsgText = "What is your identity number?";
        private const string CellNumberStepMsgText = "What is your cellphone number?";
        private const string FinalConfirmationMsgText = "Please confirm the details below:";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDialog"/> class.
        /// </summary>
        /// <param name="bookingDialog">The booking dialog.</param>
        public AccountDialog(BookingDialog bookingDialog) : base(nameof(AccountDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                TitleStepAsync,
                FirstNameStepAsync,
                LastNameStepAsync,
                IdentityNumberStepAsync,
                CellNumberStepAsync,
                ConfirmationStepAsync,
                FinalStepAsync,
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Titles the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> TitleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountDetails = (FindAccountDetails)stepContext.Options;

            if (string.IsNullOrWhiteSpace(accountDetails.Title))
            {
                var promptMessage = MessageFactory.Text(TitleStepMsgText, TitleStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(accountDetails.Title, cancellationToken);
        }

        /// <summary>
        /// Firsts the name step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FirstNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountDetails = (FindAccountDetails)stepContext.Options;
            accountDetails.Title = (string)stepContext.Result;

            if (string.IsNullOrWhiteSpace(accountDetails.FirstName))
            {
                var promptMessage = MessageFactory.Text(FirstNameStepMsgText, FirstNameStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(accountDetails.FirstName, cancellationToken);
        }

        /// <summary>
        /// Lasts the name step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> LastNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountDetails = (FindAccountDetails)stepContext.Options;
            accountDetails.FirstName = (string)stepContext.Result;

            if (string.IsNullOrWhiteSpace(accountDetails.LastName))
            {
                var promptMessage = MessageFactory.Text(LastNameStepMsgText, LastNameStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(accountDetails.LastName, cancellationToken);
        }

        /// <summary>
        /// Identities the number step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> IdentityNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountDetails = (FindAccountDetails)stepContext.Options;
            accountDetails.LastName = (string)stepContext.Result;

            if (string.IsNullOrWhiteSpace(accountDetails.IdentityNumber))
            {
                var promptMessage = MessageFactory.Text(IdentityNumberStepMsgText, IdentityNumberStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(accountDetails.IdentityNumber, cancellationToken);
        }

        /// <summary>
        /// Cells the number step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> CellNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountDetails = (FindAccountDetails)stepContext.Options;
            accountDetails.IdentityNumber = (string)stepContext.Result;

            if (string.IsNullOrWhiteSpace(accountDetails.CellphoneNumber))
            {
                var promptMessage = MessageFactory.Text(CellNumberStepMsgText, CellNumberStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(accountDetails.CellphoneNumber, cancellationToken);
        }

        /// <summary>
        /// Confirmations the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountDetails = (FindAccountDetails)stepContext.Options;
            var messageCard = CreateAccountDetailsAdaptiveCard(accountDetails);
            var response = MessageFactory.Attachment(messageCard);
            await stepContext.Context.SendActivityAsync(response, cancellationToken);

            var promptMessage = MessageFactory.Text(FinalConfirmationMsgText, FinalConfirmationMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        /// <summary>
        /// Finals the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountDetails = (FindAccountDetails)stepContext.Options;
            bool confirmed = false;
            if(stepContext.Result is string result)
            {
                confirmed = ((string)stepContext.Result).ToUpper() == "YES";
            }
            else
            {
                confirmed = (bool)stepContext.Result;
            }

            if (confirmed)
            {
                var messageText = $"Your reference number is : {Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            return await stepContext.EndDialogAsync(accountDetails, cancellationToken);
        }

        /// <summary>
        /// Creates the account details adaptive card.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Attachment CreateAccountDetailsAdaptiveCard(FindAccountDetails accountDetails)
        {
            var card = new AdaptiveCard("1.0");
            List<AdaptiveElement> adaptiveElements = new List<AdaptiveElement>()
            {
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveImage
                                {
                                    Url = new Uri("https://github.com/enablers104/chatbot/blob/master/Images/Logo.png?raw=true"),
                                    Size = AdaptiveImageSize.Small
                                },
                                new AdaptiveTextBlock
                                {
                                    Text = "Basic information",
                                    Spacing = AdaptiveSpacing.Medium,
                                    Size = AdaptiveTextSize.Default,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Wrap = true,
                                    MaxLines = 0
                                },
                                new AdaptiveFactSet
                                {
                                    Facts = new List<AdaptiveFact>
                                    {
                                        new AdaptiveFact
                                        {
                                            Title = "Title",
                                            Value = $"**{accountDetails.Title}**"
                                        },
                                        new AdaptiveFact
                                        {
                                            Title = "FirstName",
                                            Value = $"**{accountDetails.FirstName}**"
                                        },
                                        new AdaptiveFact
                                        {
                                            Title = "LastName",
                                            Value = $"**{accountDetails.LastName}**"
                                        },
                                        new AdaptiveFact
                                        {
                                            Title = "IdentityNumber",
                                            Value = $"**{accountDetails.IdentityNumber}**"
                                        },
                                        new AdaptiveFact
                                        {
                                            Title = "CellphoneNumber",
                                            Value = $"**{accountDetails.CellphoneNumber}**"
                                        }
                                    }
                                }
                            },
                            Separator = true
                        }
                    }
                }
            };

            AdaptiveContainer container = new AdaptiveContainer
            {
                Items = adaptiveElements
            };

            card.Body.Add(container);

            var attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return attachment;
        }
    }
}
