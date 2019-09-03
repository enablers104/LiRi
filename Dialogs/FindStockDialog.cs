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

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class FindStockDialog : CancelAndHelpDialog
    {
        private const string GarmentStepMsgText = "What Garment are you looking for?";
        private const string ColorStepMsgText = "What Colour are you looking for?";
        private const string SizeStepMsgText = "What Size are you looking for?";

        /// <summary>
        /// Initializes a new instance of the <see cref="FindStockDialog"/> class.
        /// </summary>
        public FindStockDialog() : base(nameof(FindStockDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GarmentStepAsync,
                ColorStepAsync,
                SizeStepAsync,
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
        /// Finals the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var StockDetails = (FindStockDetails)stepContext.Options;

            StockDetails.Size = (string)stepContext.Result;

            return await stepContext.EndDialogAsync(StockDetails, cancellationToken);
        }
    }
}
