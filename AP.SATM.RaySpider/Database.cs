using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;

namespace AP.SATM.RaySpider
{
    class Database
    {
        internal List<IntellectOutput> Buffer;

        public Database()
        {
            Buffer = new List<IntellectOutput>();
        }

        internal void InsertIntoProtocol(SqlConnection sqlConnection, IntellectOutput dataToWrite)
        {
            EventLog log = new EventLog();
            IntellectOutput dm = dataToWrite;
            log.Source = "SATM Rays";
            log.Log = "Application";
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = Properties.Settings.Default.InsertEvent;
                sqlCommand.Parameters.Add("@date", SqlDbType.DateTime).Value = dm.date;
                sqlCommand.Parameters.Add("@objid", SqlDbType.Char).Value = dm.objId;
                sqlCommand.Parameters.Add("@action", SqlDbType.Char).Value = (dm.action ? "ON" : "OFF");
                sqlCommand.Parameters.Add("@owner", SqlDbType.Char).Value = dm.owner;
                try
                {
                    sqlCommand.ExecuteNonQuery();
                    //Buffer.Remove(dataToWrite);
                }
                catch (Exception ex)
                {
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                    if (Properties.Settings.Default.Debug)
                    {
                        Log.Write(String.Format("Failed to insert: {0}-{1}-{2}-{3}", dm.owner, dm.objId, dm.action, dm.date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)));
                    }
                    Buffer.Add(new IntellectOutput { owner = dm.owner, objId = dm.objId, action = dm.action, date = dm.date });
                }
            }
        }

        internal void UpdateFromBuffer()
        {
            List<IntellectOutput> events = Buffer;
            using (SqlConnection sql = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                try
                {
                    sql.Open();
                }
                catch
                {
                    return;
                }
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = sql;
                    sqlCommand.CommandText = Properties.Settings.Default.InsertEvent;
                    sqlCommand.Parameters.Add("@date", SqlDbType.DateTime);
                    sqlCommand.Parameters.Add("@objid", SqlDbType.Char);
                    sqlCommand.Parameters.Add("@action", SqlDbType.Char);
                    sqlCommand.Parameters.Add("@owner", SqlDbType.Char);
                    Log.Write("Buffered events: " + Buffer.Count.ToString());

                    for (int i = events.Count - 1; i > -1; i--)
                    {
                        sqlCommand.Parameters["@date"].Value = events[i].date;
                        sqlCommand.Parameters["@objid"].Value = events[i].objId;
                        sqlCommand.Parameters["@action"].Value = (events[i].action ? "ON" : "OFF");
                        sqlCommand.Parameters["@owner"].Value = events[i].owner;
                        try
                        {
                            sqlCommand.ExecuteNonQuery();
                            if (Properties.Settings.Default.Debug)
                            {
                                Log.Write(String.Format("buffer event updated: {0};{1};{2};{3}", events[i].owner, events[i].objId, events[i].action, events[i].date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)));
                            }
                            Buffer.RemoveAt(i);
                        }
                        catch
                        {
                            if (Properties.Settings.Default.Debug)
                            {
                                Log.Write(String.Format("Failed to insert: {0};{1};{2};{3}", events[i].owner, events[i].objId, events[i].action, events[i].date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)));
                            }
                            return;
                        }
                        //
                    } //foreach
                    Buffer.Clear();
                }//using sqlcommand
            }

        }

        internal bool SelectLastState(SqlConnection sqlConnection, string owner, string rayId)
        {
            EventLog log = new EventLog();
            log.Source = "SATM Rays";
            log.Log = "Application";
            using (SqlCommand selectState = new SqlCommand())
            {
                selectState.Connection = sqlConnection;
                selectState.CommandText = Properties.Settings.Default.SelectState;
                selectState.Parameters.Add("@owner", SqlDbType.Char).Value = owner;
                selectState.Parameters.Add("@objid", SqlDbType.Char).Value = rayId;
                try
                {
                    return ((string)selectState.ExecuteScalar() == "ON");
                }
                catch (Exception ex)
                {
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                    return false;
                }
            }

        }
    }
}
