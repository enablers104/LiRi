using Dapper;
using System.Linq;
using CoreBot.Entities;
using System.Threading.Tasks;
using CoreBot.Data.Interfaces;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace CoreBot.Data
{
    public class StockRepository : BaseRepository, IStockRepository
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StockRepository" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public StockRepository(IConfiguration configuration) : base(configuration["LiriSQLConnection"])
        {
        }

        #endregion

        #region Implemented Members

        /// <summary>
        /// Finds the stock.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Stock>> FindStock()
        {
            return await ExecWithConnAsync(async db => {
                var resultList = await db.QueryAsync<Stock>($"SELECT * FROM dbo.tStock", commandType: System.Data.CommandType.Text).ConfigureAwait(false);
                return resultList.ToList();
            });
        }

        /// <summary>
        /// Finds the stock by filters.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="garment">The garment.</param>
        /// <returns></returns>
        public async Task<List<Stock>> FindStockByFilters(string color, string garment)
        {
            string sql = $"SELECT * FROM dbo.tStock WHERE Color = '{color}' AND Garment = '{garment}'";
            return await ExecWithConnAsync(async db => {
                var resultList = await db.QueryAsync<Stock>(sql, commandType: System.Data.CommandType.Text).ConfigureAwait(false);
                return resultList.ToList();
            });
        }

        /// <summary>
        /// Finds the stock by sku code.
        /// </summary>
        /// <param name="skuCode">The sku code.</param>
        /// <returns></returns>
        public async Task<Stock> FindStockBySkuCode(string skuCode)
        {
            string sql = $"SELECT * FROM dbo.tStock WHERE SkuCode = '{skuCode}'";
            return await ExecWithConnAsync(async db => {
                var resultList = await db.QueryAsync<Stock>(sql, commandType: System.Data.CommandType.Text).ConfigureAwait(false);
                return resultList.FirstOrDefault();
            });
        }

        #endregion
    }
}
