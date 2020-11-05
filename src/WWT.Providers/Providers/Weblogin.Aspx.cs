using System;
using System.Data;
using System.Data.SqlClient;

namespace WWT.Providers
{
    public abstract partial class LoginWebUser : RequestProvider
    {
        protected readonly WwtOptions _options;

        public LoginWebUser(WwtOptions options)
        {
            _options = options;
        }

        internal SqlConnection GetConnectionLogging()
        {
            string connStr = null;
            connStr = _options.LoggingConn;
            SqlConnection myConnection = null;
            myConnection = new SqlConnection(connStr);
            return myConnection;
        }

        public string PostFeedback(string GUID, byte type)
        {
            // type 1 = Windows client
            // type 2 = Web Client

            string strErrorMsg;
            SqlConnection myConnection5 = GetConnectionLogging();

            try
            {
                myConnection5.Open();

                SqlCommand Cmd = null;
                Cmd = new SqlCommand();
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandTimeout = 20;
                Cmd.Connection = myConnection5;

                Cmd.CommandText = "spLoginUser";

                SqlParameter CustParm = new SqlParameter("@pUserGUID", SqlDbType.VarChar);
                CustParm.Value = GUID.ToUpper();
                Cmd.Parameters.Add(CustParm);

                SqlParameter CustParm2 = new SqlParameter("@pCLientType", SqlDbType.TinyInt);
                CustParm2.Value = type;
                Cmd.Parameters.Add(CustParm2);



                Cmd.ExecuteNonQuery();


            }
            catch (InvalidCastException)
            { }

            catch (Exception ex)
            {
                //throw ex.GetBaseException();
                strErrorMsg = ex.Message;
                return strErrorMsg;
            }
            finally
            {
                if (myConnection5.State == ConnectionState.Open)
                {
                    myConnection5.Close();
                }
            }
            return "ok";

        }
    }
}
