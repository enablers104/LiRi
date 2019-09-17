using CoreBot.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.BotBuilderSamples.DialogModels;

namespace CoreBot.Data.Interfaces
{
    public interface IStockRepository
    {
        Task<List<Stock>> FindStock();

        Task<List<Stock>> FindStockByFilters(string color, string garment, string size);

        Task<List<Stock>> FindStockBySkuCode(string skuCode);

        Task<List<Stock>> FindStockByFilters(FindStockDetails findStockDetails);
    }
}
