using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Data.Sql;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Data
{
    public class SQLClient
    {
        const string m_ClassName = "LiriStock.DBHelper";
        string m_DBServer;
        string m_ConnectionString;


        //public SQLClient(IConfiguration configuration)
        //{ m_ConnectionString = configuration["LiriSQLConnection"]; }

        public string ConnectionString
        {
            get
            {
                if ( string.IsNullOrEmpty(m_ConnectionString))
                    m_ConnectionString = GetConnectionString();
                return m_ConnectionString;
            }
            set { m_ConnectionString = value; }
        }


        #region Private Methods
        
        /// <summary>
        /// Uses the Configuration class to retrieve the connection string
        /// </summary>
        /// <returns>the connection string for SRR database</returns>
        private string GetConnectionString()
        {
            //object congifstr = ConfigurationManager.GetSection("SRRNetAccessConfiguration");
            //Configuration config = ConfigurationManager.GetSection("SRRNetAccessConfiguration") as Configuration;
            //return config.SRRConnectionString;

            //return ConfigurationManager.ConnectionStrings["DefaultSQlConnection"].ToString();

            //return m_ConnectionString;
            return "Server=tcp:lirisql.database.windows.net,1433;Initial Catalog=LiriStock;Persist Security Info=False;User ID=LiriAdmin;Password=L1r1Adm1n;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Use this method to create a SQLParam for pasing to a SQL stored proc execute, this overcomes a shortfall of the SQLClient object
        /// </summary>
        /// <param name="paramName">The @Param name of the Parameter</param>
        /// <param name="dbType">The database type of the Paramter</param>
        /// <param name="size">The size of the Parameter used for sixed Parameters</param>
        /// <param name="value">The Value to put in teh Parameter as an object</param>
        /// <returns></returns>
        public SqlParameter BuildSQLParam(string paramName, SqlDbType dbType, int size, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            SqlParameter sparam = new SqlParameter(paramName, dbType);
            sparam.Value = value;
            sparam.Direction = direction;

            if (size > 0)
                sparam.Size = size;


            return sparam;
        } 


        /// <summary>
        /// RunSP executes the requested Stored procedure against the configured / set database database with no return values.
        /// </summary>
        /// <param name="storedproc">The Stored proc you wishing to execute</param>
        /// <param name="spparams">A list of SQLParameters (BuildParamaters Helps create params)</param>
        public void RunSP(string storedproc,List<SqlParameter> spparams = null)
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(storedproc, cn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (spparams != null)
            {
                foreach (SqlParameter item in spparams)
                {
                    cmd.Parameters.Add(item);
                }
            }

            try
            {
                cn.Open();
                cmd.ExecuteNonQuery();
                //cn.Close();
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                cn.Close();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedproc"></param>
        /// <param name="spparams"></param>
        /// <returns></returns>
        public Task RunSPAsync(string storedproc, List<SqlParameter> spparams = null)
        {
            return Task.Run(() =>
            {
                SqlConnection cn = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(storedproc, cn);
                cmd.CommandType = CommandType.StoredProcedure;

                if (spparams != null)
                {
                    foreach (SqlParameter item in spparams)
                    {
                        cmd.Parameters.Add(item);
                    }
                }

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    //cn.Close();
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    cn.Close();                    
                }
            });
        }



        /// <summary>
        /// This method executes a Stored proc that has Output params, it returns a command object with the params holding values.
        /// </summary>
        /// <param name="strSP">The Stored proc you wishing to execute</param>
        /// <param name="spparams">A list of SQLParameters (BuildParamaters Helps create params)</param>
        /// <returns>a SQLCommand object with the params holding values</returns>
        public SqlCommand RunSingletonSP(string storedproc, List<SqlParameter> spparams = null)
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(storedproc, cn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (spparams != null)
            {
                foreach (SqlParameter item in spparams)
                {
                    cmd.Parameters.Add(item);
                }
            }

            try
            {
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
                return cmd;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                cn.Close();
            }
        }


        /// <summary>
        /// This method executes a Stored proc that returns a Datatable for reading that data.
        /// </summary>
        /// <param name="storedproc">The Stored proc you wishing to execute</param>
        /// <param name="spparams">A list of SQLParameters (BuildParamaters Helps create params)</param>
        /// <returns>SQLDataReader</returns>
        public DataTable RunSPReturnDT(string storedproc, List<SqlParameter> spparams = null)
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(storedproc, cn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (spparams != null)
            {
                foreach (SqlParameter item in spparams)
                {
                    cmd.Parameters.Add(item);
                }
            }

            try
            {
                cn.Open();
                SqlDataReader SDR = cmd.ExecuteReader();
                DataTable dtSQL = new DataTable();
                dtSQL.Load(SDR);
                cn.Close();
                return dtSQL;

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                cn.Close();
            }
        }



        public Task<DataTable> RunSPReturnDTAsync(string storedproc, List<SqlParameter> spparams = null)
        {
            return Task.Run(() =>
            {
                SqlConnection cn = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(storedproc, cn);
                cmd.CommandType = CommandType.StoredProcedure;

                if (spparams != null)
                {
                    foreach (SqlParameter item in spparams)
                    {
                        cmd.Parameters.Add(item);
                    }
                }

                try
                {
                    cn.Open();
                    SqlDataReader SDR = cmd.ExecuteReader();
                    DataTable dtSQL = new DataTable();
                    dtSQL.Load(SDR);
                    cn.Close();
                    return dtSQL;

                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    cn.Close();
                }
            });
        }



        public int RunSPReturnInteger(string storedproc, List<SqlParameter> spparams = null)
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(storedproc, cn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (spparams != null)
            {
                foreach (SqlParameter item in spparams)
                {
                    cmd.Parameters.Add(item);
                }
            }
            cmd.Parameters.Add(BuildSQLParam("@retval", SqlDbType.Int, 4 ,-1, ParameterDirection.Output));

            try
            {
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();

                return (int)cmd.Parameters["@retval"].Value;

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                cn.Close();
            }

        }



        /// <summary>
        /// This method executes a SQL that returns a SQLDataReader for reading that data.
        /// This should be used for testing only, live should go via Stored Procs.
        /// </summary>
        /// <param name="strSQL">The SQL statement to execute</param>
        /// <param name="spparams">A list of SQLParameters (BuildParamaters Helps create params)</param>
        /// <returns>SQLDataReader</returns>
        public SqlDataReader RunSQLReturnSDR(string strSQL, List<SqlParameter> spparams = null)
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(strSQL, cn);
            cmd.CommandType = CommandType.Text;

            if (spparams != null)
            {
                foreach (SqlParameter item in spparams)
                {
                    cmd.Parameters.Add(item);
                }
            }

            try
            {
                cn.Open();
                SqlDataReader SDR = cmd.ExecuteReader();
                //cn.Close();
                return SDR;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                cn.Close();
            }
        }


        /// <summary>
        /// This method executes a SQL that returns a SQLDataTable for reading that data.
        /// This should be used for testing only, live should go via Stored Procs.
        /// </summary>
        /// <param name="strSQL">The SQL statement to execute</param>
        /// <param name="spparams">A list of SQLParameters (BuildParamaters Helps create params)</param>
        /// <returns>DataTable</returns>
        public DataTable RunSQLReturnDT(string strSQL, List<SqlParameter> spparams = null)
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(strSQL, cn);
            cmd.CommandType = CommandType.Text;

            if (spparams != null)
            {
                foreach (SqlParameter item in spparams)
                {
                    cmd.Parameters.Add(item);
                }
            }

            try
            {
                cn.Open();
                SqlDataReader SDR = cmd.ExecuteReader();
                DataTable dtSQL = new DataTable();
                dtSQL.Load(SDR);
                cn.Close();
                return dtSQL;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                cn.Close();
            }
        }

        public Task<DataTable> RunSQLReturnDTAsync(string strSQL, List<SqlParameter> spparams = null)
        {
            return Task.Run(() =>
            {
                SqlConnection cn = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(strSQL, cn);
                cmd.CommandType = CommandType.Text;

                if (spparams != null)
                {
                    foreach (SqlParameter item in spparams)
                    {
                        cmd.Parameters.Add(item);
                    }
                }

                try
                {
                    cn.Open();
                    SqlDataReader SDR = cmd.ExecuteReader();
                    DataTable dtSQL = new DataTable();
                    dtSQL.Load(SDR);
                    cn.Close();
                    return dtSQL;
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    cn.Close();
                }
            });

        }


        public int RunSQL(string SQL, List<SqlParameter> spparams = null)
        {
            int result = 0;

            SqlConnection cn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(SQL, cn);
            cmd.CommandType = CommandType.Text;

            if (spparams != null)
            {
                foreach (SqlParameter item in spparams)
                {
                    cmd.Parameters.Add(item);
                }
            }

            try
            {
                cn.Open();
                result = cmd.ExecuteNonQuery();
                cn.Close();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                cn.Close();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedproc"></param>
        /// <param name="spparams"></param>
        /// <returns></returns>
        public Task<int> RunSQLAsync(string SQL, List<SqlParameter> spparams = null)
        {
            return Task.Run(() =>
            {
                int result = 0;

                SqlConnection cn = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL, cn);
                cmd.CommandType = CommandType.Text;

                if (spparams != null)
                {
                    foreach (SqlParameter item in spparams)
                    {
                        cmd.Parameters.Add(item);
                    }
                }

                try
                {
                    cn.Open();
                    result = cmd.ExecuteNonQuery();
                    cn.Close();
                    return result;
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    cn.Close();
                }
                return result;
            });
        }





        #endregion



    }
}
