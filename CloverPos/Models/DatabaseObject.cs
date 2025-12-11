using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverPos.Models
{
    class DatabaseObject
    {
        string constr = ConfigurationManager.AppSettings.Get("LiquorAppsConnectionString");

        public DataTable GetDataTable(string StoredProcedure, List<SqlParameter> sqlParams)
        {
            DataTable dtResult = new DataTable();

            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = StoredProcedure;
                        cmd.CommandTimeout = 3600;
                        foreach (SqlParameter par in sqlParams)
                        {
                            cmd.Parameters.Add(par);
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(dtResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }
    }
}
