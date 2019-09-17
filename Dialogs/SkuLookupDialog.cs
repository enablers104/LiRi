using AdaptiveCards;
using CoreBot.Data;
using CoreBot.Entities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.DialogModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class SkuLookupDialog : CancelAndHelpDialog
    {
        private const string SkuCodeStepMsgText = "Please enter the SKU code you want to search";
        private const string RouteStockStepMsgText = "Would you like to reroute stock?";
        private readonly StockRepository _stockRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkuLookupDialog" /> class.
        /// </summary>
        /// <param name="bookingDialog">The booking dialog.</param>
        public SkuLookupDialog(BookingDialog bookingDialog, StockRepository stockRepository) : base(nameof(SkuLookupDialog))
        {
            _stockRepo = stockRepository;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                LookupStepAsync,
                RouteStockStepAsync,
                FinalStepAsync,
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Lookups the step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> LookupStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stockDetails = (FindStockDetails)stepContext.Options;

            if (stockDetails.SkuCode == null)
            {
                var promptMessage = MessageFactory.Text(SkuCodeStepMsgText, SkuCodeStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(stockDetails.SkuCode, cancellationToken);
        }

        /// <summary>
        /// Routes the stock step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> RouteStockStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stockDetails = (FindStockDetails)stepContext.Options;

            //got all info needed for DB call AI  (build out AI object)
            StringBuilder getTFGMessageText = new StringBuilder();

            Activity getTFGMessage;

            var stock = _stockRepo.FindStockBySkuCode(stockDetails.SkuCode).Result;
            if (stock.Count == 0)
            {
                getTFGMessageText.AppendLine("Sorry, we DO NOT have stock available.");

                getTFGMessage = MessageFactory.Text(getTFGMessageText.ToString(), getTFGMessageText.ToString(), InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(getTFGMessage, cancellationToken);

                return await stepContext.EndDialogAsync(stockDetails, cancellationToken);
            }
            else
            {
                var messageCard = CreateStockAdaptiveCard(GetFactsInColumns(stock, stockDetails), stockDetails);
                var response = MessageFactory.Attachment(messageCard);
                await stepContext.Context.SendActivityAsync(response, cancellationToken);

                if (stock.Count > 1)
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
            var stockDetails = (FindStockDetails)stepContext.Options;
            var confirmed = false;
            if (stepContext.Result is string result)
            {
                confirmed = ((string)stepContext.Result).ToUpper() == "YES";
            }
            else
            {
                confirmed = (bool)stepContext.Result;
            }

            if (confirmed)
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

            return await stepContext.EndDialogAsync(stockDetails, cancellationToken);
        }

        /// <summary>
        /// Creates the stock adaptive card.
        /// </summary>
        /// <param name="facts">The facts.</param>
        /// <returns></returns>
        private Attachment CreateStockAdaptiveCard(List<AdaptiveColumn> facts, FindStockDetails stockDetails)
        {
            var card = new AdaptiveCard("1.0");

            string FactHeader = "";
            string SearchValue = $"Garment = {stockDetails.Garment}{Environment.NewLine}";

            if (!string.IsNullOrWhiteSpace(stockDetails.Brand))
            {
                SearchValue = $"{SearchValue}Brand = {stockDetails.Brand}{Environment.NewLine}";
            }
            else { FactHeader = $"**Brand** "; }

            if (!string.IsNullOrWhiteSpace(stockDetails.Style))
            {
                SearchValue = $"{SearchValue}Style = {stockDetails.Style}{Environment.NewLine}";
            }
            else { FactHeader = $"{FactHeader} **Style** "; }

            if (!string.IsNullOrWhiteSpace(stockDetails.Color))
            {
                SearchValue = $"{SearchValue}Color = {stockDetails.Color}{Environment.NewLine}";
            }
            else { FactHeader = $"{FactHeader} **Color** "; }

            if (!string.IsNullOrWhiteSpace(stockDetails.Size))
            {
                SearchValue = $"{SearchValue}Size = {stockDetails.Size}{Environment.NewLine}";
            }
            else { FactHeader = $"{FactHeader} **Size**  "; }

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
                                    Text = "Invetory search",
                                    Spacing = AdaptiveSpacing.Medium,
                                    Size = AdaptiveTextSize.Default,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Wrap = true,
                                    MaxLines = 0
                                },
                                new AdaptiveTextBlock
                                {
                                    Text = "Based on:" + Environment.NewLine + SearchValue + "We have the following in stock:",
                                    Size = AdaptiveTextSize.Default,
                                    IsSubtle = true,
                                    Wrap = true,
                                    MaxLines = 0
                                },
                                new AdaptiveColumnSet
                                {
                                    Columns = facts,
                                    Bleed = true,
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

        /// <summary>
        /// Gets the facts in columns2.
        /// </summary>
        /// <param name="stocks">The stocks.</param>
        /// <param name="stockDetails">The stock details.</param>
        /// <returns></returns>
        private List<AdaptiveColumn> GetFactsInColumns(List<Stock> stocks, FindStockDetails stockDetails)
        {
            List<AdaptiveElement> QtyElements = new List<AdaptiveElement>();
            List<AdaptiveElement> BrandElements = new List<AdaptiveElement>();
            List<AdaptiveElement> StyleElements = new List<AdaptiveElement>();
            List<AdaptiveElement> ColourElements = new List<AdaptiveElement>();
            List<AdaptiveElement> SizeElements = new List<AdaptiveElement>();
            List<AdaptiveElement> BranchElements = new List<AdaptiveElement>();
            List<AdaptiveElement> IBTElements = new List<AdaptiveElement>();

            //build the headers items.
            QtyElements.Add(CreateTextBlock("**Quantity**"));
            if (string.IsNullOrWhiteSpace(stockDetails.Brand))
            {
                BrandElements.Add(CreateTextBlock("**Brand**"));
            }
            if (string.IsNullOrWhiteSpace(stockDetails.Style))
            {
                StyleElements.Add(CreateTextBlock("**Style**"));
            }
            if (string.IsNullOrWhiteSpace(stockDetails.Color))
            {
                ColourElements.Add(CreateTextBlock("**Colour**"));
            }
            if (string.IsNullOrWhiteSpace(stockDetails.Size))
            {
                SizeElements.Add(CreateTextBlock("**Size**"));
            }
            BranchElements.Add(CreateTextBlock("**Branch**"));
            IBTElements.Add(CreateTextBlock("**IBT**"));

            //load all the result items
            int item = 0;
            foreach (var stock in stocks)
            {
                QtyElements.Add(CreateTextBlock(stock.Quantity.ToString()));
                if (string.IsNullOrWhiteSpace(stockDetails.Brand))
                {
                    BrandElements.Add(CreateTextBlock(stock.Brand));
                }
                if (string.IsNullOrWhiteSpace(stockDetails.Style))
                {
                    StyleElements.Add(CreateTextBlock(stock.Style));
                }
                if (string.IsNullOrWhiteSpace(stockDetails.Color))
                {
                    ColourElements.Add(CreateTextBlock(stock.Color));
                }
                if (string.IsNullOrWhiteSpace(stockDetails.Size))
                {
                    SizeElements.Add(CreateTextBlock(stock.Size));
                }
                BranchElements.Add(CreateTextBlock(stock.Branch));
                IBTElements.Add(
                    new AdaptiveNumberInput
                    {
                        Id = "qtr_" + item++,
                        Max = stock.Quantity,
                        Min = 0

                    });
            }

            List<AdaptiveColumn> ColumnList = new List<AdaptiveColumn>();
            ColumnList.Add(new AdaptiveColumn
            {
                Items = QtyElements,
                Width = AdaptiveColumnWidth.Auto
            });
            if (string.IsNullOrWhiteSpace(stockDetails.Brand))
            {
                ColumnList.Add(new AdaptiveColumn
                {
                    Items = BrandElements,
                    Width = AdaptiveColumnWidth.Auto
                });
            }
            if (string.IsNullOrWhiteSpace(stockDetails.Style))
            {
                ColumnList.Add(new AdaptiveColumn
                {
                    Items = StyleElements,
                    Width = AdaptiveColumnWidth.Auto

                });
            }
            if (string.IsNullOrWhiteSpace(stockDetails.Color))
            {
                ColumnList.Add(new AdaptiveColumn
                {
                    Items = ColourElements,
                    Width = AdaptiveColumnWidth.Auto

                });
            }
            if (string.IsNullOrWhiteSpace(stockDetails.Size))
            {
                ColumnList.Add(new AdaptiveColumn
                {
                    Items = SizeElements,
                    Width = AdaptiveColumnWidth.Auto
                });
            }
            ColumnList.Add(new AdaptiveColumn
            {
                Items = BranchElements,
                Width = AdaptiveColumnWidth.Auto
            });
            ColumnList.Add(new AdaptiveColumn
            {
                Items = IBTElements,
                Width = "40px"
            });

            return ColumnList;
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
