using System;
using System.Data;
using System.Data.SqlClient;

namespace AP.SATM.RaysDebugger
{
    internal class Database
    {
        static internal bool InsertIntoProtocol(SqlConnection sqlConnection, IntellectOutput dataToWrite)
        {
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "INSERT INTO [intellect].[dbo].[protocol] ([objtype],[objid],[action],[region_id],[param0],[param1],[param2],[param3],[date],[time],[time2],[owner],[pk],[user_param_double])";
                sqlCommand.CommandText += "VALUES('GRAY',@objid,@action,'','','','','',@date,@date,@date,@owner,NEWID(), NULL)";
                sqlCommand.Parameters.Add("@date", SqlDbType.DateTime).Value = dataToWrite.date;
                sqlCommand.Parameters.Add("@objid", SqlDbType.Char).Value = dataToWrite.objId;
                sqlCommand.Parameters.Add("@action", SqlDbType.Char).Value = (dataToWrite.action ? "ON" : "OFF");
                sqlCommand.Parameters.Add("@owner", SqlDbType.Char).Value = dataToWrite.owner;
                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;
        }

        static internal bool SelectLastState(SqlConnection sqlConnection, string owner, string rayId)
        {
            using (SqlCommand selectState = new SqlCommand())
            {
                selectState.Connection = sqlConnection;
                selectState.CommandText = "SELECT TOP 1 [action] FROM[intellect].[dbo].[PROTOCOL] WHERE objtype = 'GRAY' AND owner = @owner and objid = @objid ORDER BY date DESC";
                selectState.Parameters.Add("@owner", SqlDbType.Char).Value = owner;
                selectState.Parameters.Add("@objid", SqlDbType.Char).Value = rayId;
                try
                {
                    return ((string)selectState.ExecuteScalar() == "ON");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

        }
    }
}
