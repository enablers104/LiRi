using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.DialogModels;
using Microsoft.BotBuilderSamples.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class HRDialog : CancelAndHelpDialog
    {
        private const string IdentificationStepMsgText = "Please enter your ID or Employee Number";
        private const string QueryStepMsgText = "What would you like to query?";

        /// <summary>
        /// Initializes a new instance of the <see cref="HRDialog"/> class.
        /// </summary>
        /// <param name="bookingDialog">The booking dialog.</param>
        public HRDialog(BookingDialog bookingDialog) : base(nameof(HRDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                QueryTypeAsync,
                RouteDecisionAsync,
                FinalStepAsync,
            }));;
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Queries the type asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> QueryTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var hrDetails = (FindHRDetails)stepContext.Options;

            if (string.IsNullOrWhiteSpace(hrDetails.QueryType))
            {
                var promptMessage = MessageFactory.Text(QueryStepMsgText, QueryStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(hrDetails.QueryType, cancellationToken);
        }

        /// <summary>
        /// Routes the decision asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> RouteDecisionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var hrDetails = (FindHRDetails)stepContext.Options;

            hrDetails.QueryType = (string)stepContext.Result;

            if (!string.IsNullOrWhiteSpace(hrDetails.QueryType))
            {
                if (IsLeaveBalance(hrDetails.QueryType))
                {
                    var promptMessage = MessageFactory.Text(IdentificationStepMsgText, IdentificationStepMsgText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (IsJobListing(hrDetails.QueryType))
                {
                    var messageCard = CreateJobListAdaptiveCard();
                    var message = MessageFactory.Attachment(messageCard);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                    return await stepContext.EndDialogAsync(hrDetails, cancellationToken);
                }
                else
                {
                    var messageText = $"I could not help with your query. Please try LEAVE BALANCE OR JOBS.";
                    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                    return await stepContext.EndDialogAsync(hrDetails, cancellationToken);
                }
            }
            return await stepContext.NextAsync(hrDetails.QueryType, cancellationToken);
        }

        /// <summary>
        /// Finals the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var hrDetails = (FindHRDetails)stepContext.Options;
            var messageText = $"You have {new Random().Next(1, 12)} leave days.";
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(message, cancellationToken);
            return await stepContext.EndDialogAsync(hrDetails, cancellationToken);
        }

        /// <summary>
        /// Determines whether [is leave balance] [the specified query].
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>
        ///   <c>true</c> if [is leave balance] [the specified query]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLeaveBalance(string query)
        {
            bool result = false;
            string queryList = "LEAVE, BALANCE, DAYS";
            string[] values = query.Split(' ');
            foreach(var val in values)
            {
                result = queryList.Contains(val.ToUpper());
                if (result)
                    break;
            }
            return result;
        }

        /// <summary>
        /// Determines whether [is job listing] [the specified query].
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>
        ///   <c>true</c> if [is job listing] [the specified query]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsJobListing(string query)
        {
            string queryList = "Careers, Career, Job, Jobs, Opportunities, Work";
            bool success = queryList.ToUpper().Contains(query.ToUpper());

            return success;
        }

        /// <summary>
        /// Creates the stock adaptive card.
        /// </summary>
        /// <returns></returns>
        private Attachment CreateJobListAdaptiveCard()
        {
            var card = new AdaptiveCard("1.0");
            List<AdaptiveFact> facts = new List<AdaptiveFact>
            {
                new AdaptiveFact
                {
                    Title = "[Senior Manager Business Development Manufacturing](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9754&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Value = "[Senior Manager: QA and Testing](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=8012&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Value = "[Service Desk Team Manager](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9555&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Software Developer - eCommerce](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9829&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Test Analyst](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9462&localeCode=en-us)",
                },
                new AdaptiveFact
                {
                    Title = "[Scrum Master - Intelligent Automation](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9372&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Development Team Manager](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9354&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Front-end Developer](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9341&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Graphic Designer](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9343&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Senior Software Engineer-Manufacturing & Logistics](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9075&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Team Lead – Finance Systems Operations](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9001&localeCode=en-us)"
                },
                new AdaptiveFact
                {
                    Title = "[Senior Manager: QA and Testing](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=8012&localeCode=en-us)"
                }
            };

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
                                    Text = "Job listing",
                                    Spacing = AdaptiveSpacing.Medium,
                                    Size = AdaptiveTextSize.Default,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Wrap = true,
                                    MaxLines = 0
                                }
                                ,CreateTextBlock("[Senior Manager Business Development Manufacturing](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9754&localeCode=en-us)")                                
                                ,CreateTextBlock("[Senior Manager: QA and Testing](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=8012&localeCode=en-us)")                                
                                ,CreateTextBlock("[Service Desk Team Manager](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9555&localeCode=en-us)")                                
                                ,CreateTextBlock("[Software Developer - eCommerce](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9829&localeCode=en-us)")                                
                                ,CreateTextBlock("[Test Analyst](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9462&localeCode=en-us)")                                
                                ,CreateTextBlock("[Scrum Master - Intelligent Automation](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9372&localeCode=en-us)")                                
                                ,CreateTextBlock("[Development Team Manager](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9354&localeCode=en-us)")                                
                                ,CreateTextBlock("[Front-end Developer](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9341&localeCode=en-us)")                                
                                ,CreateTextBlock("[Graphic Designer](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9343&localeCode=en-us)")                                
                                ,CreateTextBlock("[Senior Software Engineer-Manufacturing & Logistics](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9075&localeCode=en-us)")                                
                                ,CreateTextBlock("[Team Lead – Finance Systems Operations](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=9001&localeCode=en-us)")                                
                                ,CreateTextBlock("[Senior Manager: QA and Testing](https://careers.peopleclick.eu.com/careerscp/client_thefoschinigroup/external/jobDetails/jobDetail.html?jobPostId=8012&localeCode=en-us)")                                
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

        /// <summary>
        /// Creates the text block.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private AdaptiveTextBlock CreateTextBlock(string text)
        {
            return new AdaptiveTextBlock
            {
                Text = text,
                Size = AdaptiveTextSize.Default,
                IsSubtle = true,
                Wrap = true,
                MaxLines = 0
            };
        }
    }
}
