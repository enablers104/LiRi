using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CoreBot.Data
{
    public abstract class BaseRepository
    {
        private readonly string _connString;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository"/> class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        protected BaseRepository(string connString)
        {
            _connString = connString;            
        }

        /// <summary>
        /// Executes the with connection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getRequestData">The get request data.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// </exception>
        protected T ExecWithConn<T>(Func<IDbConnection, T> getRequestData)
        {
            try
            {
                using (var connection = new SqlConnection(_connString))
                {
                    connection.Open();
                    return getRequestData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.ExecWithConn() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception($"{GetType().FullName}.ExecWithConn() - Error : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executes with connection async.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getData">The get data.</param>
        /// <returns></returns>
        /// <exception cref="Exception">{GetType().FullName}.
        /// or
        /// {GetType().FullName}.</exception>
        protected async Task<T> ExecWithConnAsync<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using (var connection = new SqlConnection(_connString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.ExecWithConnAsync() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception($"{GetType().FullName}.ExecWithConnAsync() - Error : {ex.Message}", ex);
            }
        }
    }
}
