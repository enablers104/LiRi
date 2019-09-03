using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.BotBuilderSamples.DialogModels;
using System.Text;
using CoreBot.Data;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class FindStockDialog : CancelAndHelpDialog
    {
        private const string GarmentStepMsgText = "What Germent are you looking for?";
        private const string ColorStepMsgText = "What Colour are you looking for?";
        private const string SizeStepMsgText = "What Size are you looking for?";
        private const string RouteStockStepMsgText = "Would you like to reroute stock?";

        private readonly StockRepository _stockRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindStockDialog"/> class.
        /// </summary>
        public FindStockDialog(BookingDialog bookingDialog, StockRepository stockRepository) : base(nameof(FindStockDialog))
        {
            _stockRepo = stockRepository;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GarmentStepAsync,
                ColorStepAsync,
                SizeStepAsync,
                RouteStockStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Garments the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GarmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var StockDetails = (FindStockDetails)stepContext.Options;

            if (StockDetails.Garment == null)
            {
                var promptMessage = MessageFactory.Text(GarmentStepMsgText, GarmentStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(StockDetails.Garment, cancellationToken);
        }

        /// <summary>
        /// Colors the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ColorStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var StockDetails = (FindStockDetails)stepContext.Options;

            StockDetails.Garment = (string)stepContext.Result;

            if (StockDetails.Color == null)
            {
                var promptMessage = MessageFactory.Text(ColorStepMsgText, ColorStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(StockDetails.Color, cancellationToken);
        }

        /// <summary>
        /// Sizes the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> SizeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var StockDetails = (FindStockDetails)stepContext.Options;

            StockDetails.Color = (string)stepContext.Result;

            if (StockDetails.Size == null)
            {
                var promptMessage = MessageFactory.Text(SizeStepMsgText, SizeStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(StockDetails.Size, cancellationToken);
        }

        /// <summary>
        /// Routes the stock step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> RouteStockStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var StockDetails = (FindStockDetails)stepContext.Options;
            StockDetails.Size = (string)stepContext.Result;

            //do call here and redirect if we must relocate stock

            //got all info needed for DB call AI  (build out AI object)
            StringBuilder getTFGMessageText = new StringBuilder();

            Activity getTFGMessage;

            //grow this out
            var stockList = _stockRepo.FindStockByFilters(StockDetails).Result;
            if (stockList.Count == 0)
            {
                getTFGMessageText.AppendLine("Sorry, we DO NOT stock available.");

                getTFGMessage = MessageFactory.Text(getTFGMessageText.ToString(), getTFGMessageText.ToString(), InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(getTFGMessage, cancellationToken);

                return await stepContext.EndDialogAsync(StockDetails, cancellationToken);
            }
            else
            {
                getTFGMessageText.AppendLine("We have the following in stock: ");
                foreach (var stockItem in stockList)
                {
                    getTFGMessageText.AppendLine($"{stockItem.Quantity} at {stockItem.Branch}");
                }

                getTFGMessage = MessageFactory.Text(getTFGMessageText.ToString(), getTFGMessageText.ToString(), InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(getTFGMessage, cancellationToken);

                if (stockList.Count > 1)
                {
                    getTFGMessage = MessageFactory.Text(RouteStockStepMsgText, RouteStockStepMsgText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = getTFGMessage }, cancellationToken);
                }
            }

            return await stepContext.NextAsync("No", cancellationToken);
        }

        /// <summary>
        /// Finals the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var StockDetails = (FindStockDetails)stepContext.Options;
            var doroute = (string)stepContext.Result;
            if(doroute.ToUpper() == "YES" || doroute.ToUpper() == "Y")
            {
                // Initialize BookingDetails with any entities we may have found in the response.
                var bookingDetails = new BookingDetails()
                {
                    // Get destination and origin from the composite entities arrays.
                    Destination = "Parow",
                };

                // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);
            }
            return await stepContext.EndDialogAsync(StockDetails, cancellationToken);
        }
    }
}
