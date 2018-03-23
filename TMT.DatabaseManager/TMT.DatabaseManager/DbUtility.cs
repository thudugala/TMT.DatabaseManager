using System;
using System.Data.Common;

namespace TMT.DatabaseManager
{
    public class DbUtility
    {
        public static DbCommandBuilder GetCommandBuilder(Type commandBuilderType, DbDataAdapter adapter)
        {
            return Activator.CreateInstance(commandBuilderType, adapter) as DbCommandBuilder;
        }

        public static DbDataAdapter GetDataAdapter(Type adapterType, string selectCommandText, DbConnection connection)
        {
            return Activator.CreateInstance(adapterType, selectCommandText, connection) as DbDataAdapter;
        }

        public static DbConnection GetDbConnection(DatabaseConnectionDetail connectionDetail)
        {
            if (string.IsNullOrWhiteSpace(connectionDetail.ConnectionString))
            {
                throw new ArgumentNullException(Properties.Resources.ERROR_ConnectionStringEmpty);
            }
            if (string.IsNullOrWhiteSpace(connectionDetail.LogInUserId))
            {
                throw new ArgumentNullException(Properties.Resources.ERROR_UserIdEmpty);
            }
            if (string.IsNullOrWhiteSpace(connectionDetail.LogInPassword))
            {
                throw new ArgumentNullException(Properties.Resources.ERROR_PasswordEmpty);
            }

            var formatedString = string.Format(connectionDetail.ConnectionString,
                                 connectionDetail.DatabaseServerName,
                                 connectionDetail.DatabaseServerPort,
                                 connectionDetail.LogInUserId,
                                 connectionDetail.LogInPassword,
                                 connectionDetail.DatabaseName);

            return Activator.CreateInstance(connectionDetail.ConnectionType, formatedString) as DbConnection;
        }

        public static Exception GetDbException(Exception ex, bool checkNumber = true)
        {
            if (ex is DbException myEx)
            {
                try
                {
                    var exceptionNumber = myEx.GetType().GetProperty("Number")?.GetValue(myEx, null);
                    if (exceptionNumber != null)
                    {
                        var number = Convert.ToInt32(exceptionNumber);
                        if (number == 0 && checkNumber)
                        {
                            return new Exception(Properties.Resources.ERROR_UserIdOrPasswordIncorrect);
                        }
                        return new Exception($"ERROR CODE-{number}: {myEx.Message}", myEx);
                    }
                    return myEx;
                }
                catch
                {
                    return ex;
                }
            }
            return ex;
        }
    }
}