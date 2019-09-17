// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using CoreBot.Data;
using Microsoft.BotBuilderSamples.DialogModels;
using System.Text;


namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly FlightBookingRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        private readonly StockRepository _stockRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDialog" /> class.
        /// </summary>
        /// <param name="luisRecognizer">The luis recognizer.</param>
        /// <param name="bookingDialog">The booking dialog.</param>
        /// <param name="findStockDialog">The find stock dialog.</param>
        /// <param name="accountDialog">The account dialog.</param>
        /// <param name="stockRepository">The stock repository.</param>
        /// <param name="logger">The logger.</param>
        public MainDialog(FlightBookingRecognizer luisRecognizer, BookingDialog bookingDialog, FindStockDialog findStockDialog, HRDialog hrDialog, AccountDialog accountDialog, SkuLookupDialog skuLookupDialog, StockRepository stockRepository, ILogger<MainDialog> logger) : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            _stockRepo = stockRepository;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);
            AddDialog(findStockDialog);
            AddDialog(accountDialog);
            AddDialog(hrDialog);
            AddDialog(skuLookupDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Introes the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "Hello! I am LIRI your friendly BOT. How can I help you today?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        /// <summary>
        /// Acts the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<LiriModel>(stepContext.Context, cancellationToken);

            switch (luisResult.TopIntent().intent)
            {
                case LiriModel.Intent.BookIBT:
                    await ShowWarningForUnsupportedBranches(stepContext.Context, luisResult, cancellationToken);

                    // Initialize BookingDetails with any entities we may have found in the response.
                    var bookingDetails = new BookingDetails()
                    {
                        // Get destination and origin from the composite entities arrays.
                        Destination = luisResult.ToEntities.Branch,
                        Origin = luisResult.FromEntities.Branch,
                        TravelDate = luisResult.TravelDate,
                    };

                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

                case LiriModel.Intent.TFGAccount:

                    // Initialize BookingDetails with any entities we may have found in the response.
                    var findAccountDetails = new FindAccountDetails()
                    {
                        // Get destination and origin from the composite entities arrays.
                        CellphoneNumber = luisResult.phonenumber,
                        FirstName = luisResult.FirstName,
                        IdentityNumber = luisResult.IDNumber,
                        LastName = luisResult.LastName,
                        Title = luisResult.Title,
                    };

                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(AccountDialog), findAccountDetails, cancellationToken);

                case LiriModel.Intent.GetWeather:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getWeatherMessageText = "TODO: get weather flow here";
                    var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
                    break;
                case LiriModel.Intent.TFGLegals:

                    var legal = stepContext.Result;
                    string msgResponse = "HINT: For Legal enquiry (Terms and Conditions or Privacy Statement)";
                    if (legal.ToString().ToLower().Contains("terms"))
                    {
                        msgResponse = "[Click for terms and conditions](https://www.tfg.co.za/terms-and-conditions)";

                    }
                    if(legal.ToString().ToLower().Contains("privacy"))
                    {
                        msgResponse = "[Click for privacy statement](https://www.tfg.co.za/privacy-statement)";
                    }
                    var LegalMessage = MessageFactory.Text(msgResponse, msgResponse, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(LegalMessage, cancellationToken);
                    break;

                case LiriModel.Intent.TFGStock:
                    // Initialize StockDetails with any entities we may have found in the response.
                    var stockDetails = new FindStockDetails()
                    {
                        // Get Stock Entities entities arrays.
                        Garment = luisResult.Garment,
                        Color = luisResult.Color,
                        Brand = luisResult.Brand,
                        Size = luisResult.Size                
                    };

                    return await stepContext.BeginDialogAsync(nameof(FindStockDialog), stockDetails, cancellationToken);

                case LiriModel.Intent.TFGSkuLookup:
                    var stock = new FindStockDetails()
                    {
                        SkuCode = luisResult.SkuCode
                    };

                    return await stepContext.BeginDialogAsync(nameof(SkuLookupDialog), stock, cancellationToken);

                case LiriModel.Intent.TFGHR:
                    var findHRDetails = new FindHRDetails()
                    {
                        QueryType = luisResult.QueryType,
                        IdentificationNumber = luisResult.EmployeeNumber
                    };

                    return await stepContext.BeginDialogAsync(nameof(HRDialog), findHRDetails, cancellationToken);

                case LiriModel.Intent.Cancel:
                    StringBuilder cancelMessageText = new StringBuilder();
                    cancelMessageText.AppendLine($"Thank you for using LiRi the friendly BOT.");
                    cancelMessageText.AppendLine($"Good bye!");
                    var goodByeMessage = MessageFactory.Text(cancelMessageText.ToString(), cancelMessageText.ToString(), InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(goodByeMessage, cancellationToken);
                    break;
                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        /// <summary>
        /// Shows the warning for unsupported branches.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="luisResult">The luis result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private static async Task ShowWarningForUnsupportedBranches(ITurnContext context, LiriModel luisResult, CancellationToken cancellationToken)
        {
            var unsupportedBranches = new List<string>();

            var fromEntities = luisResult.FromEntities;
            if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Branch))
            {
                unsupportedBranches.Add(fromEntities.From);
            }

            var toEntities = luisResult.ToEntities;
            if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Branch))
            {
                unsupportedBranches.Add(toEntities.To);
            }

            if (unsupportedBranches.Any())
            {
                var messageText = $"Sorry but the following branches are not supported: {string.Join(',', unsupportedBranches)}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await context.SendActivityAsync(message, cancellationToken);
            }
        }

        /// <summary>
        /// Finals the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is BookingDetails result)
            {
                // Now we have all the booking details call the booking service.
                // If the call to the booking service was successful tell the user.
                var timeProperty = new TimexProperty(result.TravelDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"You have requested stock from {result.Origin} to {result.Destination} on {travelDateMsg}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
