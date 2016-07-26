using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AP.SATM.Brain
{
    public class Processor
    {
        public static void Main(string[] args)
        {

            Assembly assembly = Assembly.GetEntryAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var id = attribute.Value;
            try
            {
                using (Mutex mutex = new Mutex(false, "Global\\" + id.ToString()))
                {
                    if (!mutex.WaitOne(0, false))
                    {
                        Log.Write("SATM", "Brain", "Application already running. Exiting...");
                        return;
                    }
                    DateTime sdate = DateTime.Now;
                    Log.Write("SATM", "Brain", "Start working at " + sdate.ToString());
                    List<Core> CoreConfig = new List<Core>();
                    using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.databaseConnectionString))
                    {
                        try
                        {
                            satmSqlConnection.Open();
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                            return;
                        }
                        /*Log.Write("AP.SATM.Brain", "RotateProtocol", "Clear data earlier than " + DateTime.Now.AddDays(-99).ToString("yyyy-MM-dd HH:mm:ss"));
                        using (SqlCommand satmRotateProtocol = new SqlCommand("DELETE FROM [satm].[dbo].[PROTOCOL] WHERE date<'" + DateTime.Now.AddDays(-99).ToString("yyyy-MM-dd HH:mm:ss") + "'", satmSqlConnection))
                        {
                            try
                            {
                                satmRotateProtocol.ExecuteNonQuery();
                            }
                            catch { }
                        }
                        using (SqlCommand satmRotateProtocol = new SqlCommand("DELETE FROM [satm].[dbo].[PROTOCOL1C] WHERE date<'" + DateTime.Now.AddDays(-99).ToString("yyyy-MM-dd HH:mm:ss") + "'", satmSqlConnection))
                        {
                            try
                            {
                                satmRotateProtocol.ExecuteNonQuery();
                            }
                            catch { }
                        }*/
                        using (SqlCommand satmSqlCommand = new SqlCommand("SELECT [Name],[IP],[SQLServer],[Username],[Password],[SQLDB], [RaysType] FROM [satm].[dbo].[Cores]", satmSqlConnection))
                        {
                            try
                            {
                                using (SqlDataReader satmDataReader = satmSqlCommand.ExecuteReader())
                                {
                                    while (satmDataReader.Read())
                                    {
                                        CoreConfig.Add(new Core()
                                        {
                                            Server = satmDataReader.GetString(0),
                                            IP = satmDataReader.GetString(1),
                                            SQLServer = satmDataReader.GetString(2),
                                            username = satmDataReader.GetString(3),
                                            password = satmDataReader.GetString(4),
                                            SQLDB = satmDataReader.GetString(5),
                                            RaysType = satmDataReader.GetString(6)
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                                Log.Write("DataBase", "satmDataReader", satmSqlCommand.CommandText);
                            }
                        } // SQL Command
                        //using (SqlCommand satmSqlCommand = new SqlCommand(Properties))
                    } // SQL Connection
                    if (args.Length == 0)
                    {
                        foreach (Core core in CoreConfig)
                        {
                            SynchronizeData(core.SQLServer, core.username, core.password, core.SQLDB, core.Server, core.IP);
                        }
                        foreach (Core core in CoreConfig)
                        {
                            ProccessData(core.Server, core.RaysType);
                        }

                    }
                    else
                    {
                        if (args[0].Contains("sync"))
                        {
                            foreach (Core core in CoreConfig)
                            {
                                SynchronizeData(core.SQLServer, core.username, core.password, core.SQLDB, core.Server, core.IP);
                            }
                        }
                        if (args[0].Contains("parse"))
                        {
                            foreach (Core core in CoreConfig)
                            {
                                ProccessData(core.Server, core.RaysType);
                            }
                        }
                    }
                }
            }
            catch
            {
                Log.Write("SATM", "Brain", "Application already running. Exiting...");
            }
            Log.Write("SATM", "Brain", "Stop working at " + DateTime.Now.ToString());
        }

        static void SynchronizeData(string sqlServer, string login, string password, string DB, string server, string IP)
        {
            using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.databaseConnectionString + "Connect Timeout=300"))
            {
                try
                {
                    satmSqlConnection.Open();
                }
                catch (Exception ex)
                {
                    Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                    return;
                }
                using (SqlCommand rotateData = new SqlCommand(Properties.Settings.Default.rotateDataQuery, satmSqlConnection))
                {
                    try
                    {
                        rotateData.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                    }
                }
                DateTime lastProtocol = DateTime.Now;
                lastProtocol = lastProtocol.AddYears(-1);
                DateTime lastProtocol1c = DateTime.Now;
                lastProtocol1c = lastProtocol1c.AddYears(-1);

                try
                {
                    using (SqlCommand lastActionCommand = new SqlCommand(Properties.Settings.Default.lastProtocolQuery, satmSqlConnection))
                    {
                        lastActionCommand.Parameters.Add("@owner", SqlDbType.NVarChar).Value = server;
                        using (SqlDataReader protocolReader = lastActionCommand.ExecuteReader())
                        {
                            if (protocolReader.Read())
                            {
                                lastProtocol = protocolReader.GetDateTime(0);
                            }
                        }

                        //lastProtocol = (DateTime)lastActionCommand.ExecuteScalar();
                        lastActionCommand.CommandText = Properties.Settings.Default.lastProtocol1cQuery;
                        using (SqlDataReader protocol1cReader = lastActionCommand.ExecuteReader())
                        {
                            if (protocol1cReader.Read())
                            {
                                lastProtocol1c = protocol1cReader.GetDateTime(0);
                            }
                        }
                        //lastProtocol1c = (DateTime)lastActionCommand.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                }

                using (SqlConnection coreSqlConnection = new SqlConnection("Data Source=" + sqlServer + ";User=" + login + ";Password=" + password + ";MultipleActiveResultSets=True;"))
                {
                    try
                    {
                        coreSqlConnection.Open();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        return;
                    }
                    using (SqlCommand updateDateStamp = new SqlCommand())
                    {
                        updateDateStamp.Connection = coreSqlConnection;

                        updateDateStamp.CommandText = "IF EXISTS (SELECT * FROM [" + DB + "].[dbo].SATMSync) BEGIN";
                        updateDateStamp.CommandText += " UPDATE [" + DB + "].[dbo].[SATMSync] SET LastProtocol = @LastProtocol, LastProtocol1C = @LastProtocol1C END";
                        updateDateStamp.CommandText += " ELSE BEGIN INSERT INTO [" + DB + "].[dbo].[SATMSync] (LastProtocol, LastProtocol1C) VALUES (@LastProtocol, @LastProtocol1C) END";
                        updateDateStamp.Parameters.Add("@LastProtocol", SqlDbType.DateTime).Value = lastProtocol;
                        updateDateStamp.Parameters.Add("@LastProtocol1C", SqlDbType.DateTime).Value = lastProtocol1c;
                        try
                        {
                            //Console.WriteLine("{0}\r\n{1} - {2}", updateDateStamp.CommandText, updateDateStamp.Parameters[0].Value, updateDateStamp.Parameters[1].Value);
                            updateDateStamp.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        }
                    }
                }


                string SQL = Properties.Settings.Default.protocolQuery;
                SQL = SQL.Replace("{@SQLServer}", sqlServer);
                SQL = SQL.Replace("{@username}", login);
                SQL = SQL.Replace("{@password}", password);
                SQL = SQL.Replace("{@Database}", DB);
                DateTime starttime = DateTime.Now;
                using (SqlCommand satmSqlCommand = new SqlCommand(SQL, satmSqlConnection))
                {
                    satmSqlCommand.CommandTimeout = 300;
                    try
                    {
                        int result = satmSqlCommand.ExecuteNonQuery();
                        Log.Write("Synchronization.Result", server + "(" + sqlServer + ")", "PROTOCOL : (" + result.ToString() + " row(s) affected) " + (DateTime.Now - starttime).ToString());
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        Log.Write("Synchronization.Error", server + "(" + sqlServer + ").PROTOCOL", SQL);
                    }
                    SQL = Properties.Settings.Default.sapQuery;
                    SQL = SQL.Replace("{@SQLServer}", sqlServer);
                    SQL = SQL.Replace("{@username}", login);
                    SQL = SQL.Replace("{@password}", password);
                    SQL = SQL.Replace("{@Database}", DB);
                    satmSqlCommand.CommandText = SQL;
                    try
                    {
                        int result = satmSqlCommand.ExecuteNonQuery();
                        Log.Write("Synchronization.Result", server + "(" + sqlServer + ")", "1C       : (" + result.ToString() + " row(s) affected) " + (DateTime.Now - starttime).ToString());
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        Log.Write("Synchronization.Error", server + "(" + sqlServer + ").1C", SQL);
                    }
                } // SQL Command
            } // SQL Connection
        }

        static void ProccessData(string Server, string RaysType)
        {
            using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.databaseConnectionString))
            {
                try
                {
                    satmSqlConnection.Open();
                }
                catch (Exception ex)
                {
                    Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                    Log.Write("Database", "satmSqlConnection", satmSqlConnection.ConnectionString);
                    return;
                } // Ловим ошибки подключения
                using (SqlCommand satmEntryCommand = new SqlCommand("select * from [satm].[dbo].[Entries] where core='" + Server + "'", satmSqlConnection))
                {
                    satmEntryCommand.CommandTimeout = Properties.Settings.Default.sqlCommandTimeout;
                    try
                    {
                        using (SqlDataReader satmEntryReader = satmEntryCommand.ExecuteReader())
                        {
                            while (satmEntryReader.Read())
                            {
                                #region Select from protocol
                                using (SqlCommand satmActionCommand = new SqlCommand())
                                {
                                    bool enterRay = false;
                                    bool exitRay = false;
                                    bool truck = false;
                                    string direction = "";
                                    DateTime startDate = DateTime.Parse("1984-06-01");
                                    DateTime endDate = DateTime.Parse("1986-06-08");
                                    satmActionCommand.CommandText = "SELECT objid, action, date, pk FROM [satm].[dbo].[PROTOCOL]";
                                    satmActionCommand.CommandText += " WHERE owner='" + Server + "' and processed='false' and objtype='GRAY' and objid in ('" + satmEntryReader.GetInt32(3) + "','" + satmEntryReader.GetInt32(4) + "')";
                                    satmActionCommand.CommandText += " AND action IN ('ON', 'OFF') ORDER BY DATE";
                                    satmActionCommand.Connection = satmSqlConnection;
                                    satmActionCommand.CommandTimeout = Properties.Settings.Default.sqlCommandTimeout;
                                    //List<string> pk = new List<string>();
                                    List<Event> events = new List<Event>();
                                    #region Try to select from protocol EXECUTEREADER
                                    try
                                    {
                                        using (SqlDataReader satmActionReader = satmActionCommand.ExecuteReader())
                                        {
                                            while (satmActionReader.Read())
                                            {
                                                //pk.Add(satmActionReader.GetString(3));
                                                if (satmActionReader.GetString(1) == RaysType)
                                                {
                                                    if (satmActionReader.GetString(0) == satmEntryReader.GetInt32(3).ToString())
                                                    {
                                                        enterRay = true;
                                                        if (exitRay) // вЫезд замкнут?
                                                            truck = true;
                                                        else // вЫезд не замкнут
                                                        {
                                                            startDate = satmActionReader.GetDateTime(2);
                                                            direction = "ввоз";
                                                        } // вЫезд не замкнут
                                                    } // Если замкнулись на вЪезд
                                                    else if (satmActionReader.GetString(0) == satmEntryReader.GetInt32(4).ToString())
                                                    {
                                                        exitRay = true;
                                                        if (enterRay) // вЪезд замкнут?
                                                            truck = true;
                                                        else // вЪезд не замкнут
                                                        {
                                                            startDate = satmActionReader.GetDateTime(2);
                                                            direction = "вывоз";
                                                        } // вЪезд не замкнут
                                                    } // Если замкнулись на вЫезд
                                                } // Если замкнулись
                                                else //if (satmActionReader.GetString(1) != "ALARM" && satmActionReader.GetString(1) != "DISARM") // Если разомкнулись
                                                {
                                                    if (satmActionReader.GetString(0) == satmEntryReader.GetInt32(3).ToString())
                                                    {
                                                        enterRay = false;
                                                        if (!exitRay) // Если вЫезд  НЕ замкнут то 
                                                        {
                                                            if (truck) // Был грузовик?
                                                            {
                                                                endDate = satmActionReader.GetDateTime(2);
                                                                int enterDelay = satmEntryReader.GetInt32(8);
                                                                int exitDelay = satmEntryReader.GetInt32(9);
                                                                bool legal = false;
                                                                string id = "";
                                                                string carID = "";
                                                                string culture = "";
                                                                string who = "";
                                                                truck = false;
                                                                #region Select 1C actions
                                                                using (SqlCommand satm1cCommand = new SqlCommand())
                                                                {
                                                                    satm1cCommand.CommandText = "select top 1 [1cID], [carID], [Culture], [WHO] from [satm].[dbo].[satm1C]";
                                                                    satm1cCommand.CommandText += " where (owner='" + Server + "') and (Date>='" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                                                                    satm1cCommand.CommandText += " and (Date<='" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "') and (Entry=" + satmEntryReader.GetInt32(1).ToString() + ")";
                                                                    satm1cCommand.CommandTimeout = Properties.Settings.Default.sqlCommandTimeout;
                                                                    satm1cCommand.Connection = satmSqlConnection;
                                                                    try
                                                                    {
                                                                        using (SqlDataReader satm1CReader = satm1cCommand.ExecuteReader())
                                                                        {
                                                                            if (satm1CReader.Read()) // Если есть событие 1С
                                                                            {
                                                                                legal = true;
                                                                                id = satm1CReader.GetString(0);
                                                                                carID = satm1CReader.GetString(1);
                                                                                culture = satm1CReader.GetString(2);
                                                                                who = satm1CReader.GetString(3);
                                                                            } // Если есть событие 1С
                                                                            else // Если событий 1С нет то
                                                                            {
                                                                                legal = false;
                                                                            } // Если событий 1С нет
                                                                        } //Деструктор ридера событий 1С
                                                                    } // Пробуем запрос событий 1С
                                                                    catch (Exception ex)
                                                                    {
                                                                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                                                                        Log.Write("Database", "1CSqlQuery", satm1cCommand.CommandText);
                                                                        legal = false;
                                                                        //return;
                                                                    } // Ловим ошибки в запросе событий 1С
                                                                } // Деструктор запроса событий 1С
                                                                #endregion
                                                                events.Add(new Event(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, Guid.NewGuid()));
                                                                //InsertInto(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, Guid.NewGuid());
                                                            } // Был ли грузовик?
                                                        } // Если вЫезд  НЕ замкнут 
                                                    } // Если разомкнулись на вЪезд
                                                    else if (satmActionReader.GetString(0) == satmEntryReader.GetInt32(4).ToString())
                                                    {
                                                        exitRay = false;
                                                        if (!enterRay) // Если вЪезд  НЕ замкнут то 
                                                        {
                                                            //DateTime.TryParse(satmActionReader.GetString(2), out endDate);
                                                            if (truck) // Был грузовик?
                                                            {
                                                                endDate = satmActionReader.GetDateTime(2);
                                                                int enterDelay = satmEntryReader.GetInt32(8);
                                                                int exitDelay = satmEntryReader.GetInt32(9);
                                                                bool legal = false;
                                                                string id = "";
                                                                string carID = "";
                                                                string culture = "";
                                                                string who = "";
                                                                truck = false;
                                                                #region Select from 1C
                                                                using (SqlCommand satm1cCommand = new SqlCommand())
                                                                {
                                                                    satm1cCommand.CommandText = "select top 1 [1cID], [carID], [Culture], [WHO] from [satm].[dbo].[satm1C]";
                                                                    satm1cCommand.CommandText += " where (owner='" + Server + "') and (Date>='" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                                                                    satm1cCommand.CommandText += " and (Date<='" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "') and (Entry=" + satmEntryReader.GetInt32(1).ToString() + ")";
                                                                    satm1cCommand.CommandTimeout = Properties.Settings.Default.sqlCommandTimeout;
                                                                    satm1cCommand.Connection = satmSqlConnection;
                                                                    try
                                                                    {
                                                                        using (SqlDataReader satm1CReader = satm1cCommand.ExecuteReader())
                                                                        {
                                                                            if (satm1CReader.Read()) // Если есть событие 1С
                                                                            {
                                                                                legal = true;
                                                                                id = satm1CReader.GetString(0);
                                                                                carID = satm1CReader.GetString(1);
                                                                                culture = satm1CReader.GetString(2);
                                                                                who = satm1CReader.GetString(3);
                                                                            } // Если есть событие 1С
                                                                            else // Если событий 1С нет то
                                                                            {
                                                                                legal = false;
                                                                            } // Если событий 1С нет
                                                                        } //Деструктор ридера событий 1С
                                                                    } // Пробуем запрос событий 1С
                                                                    catch (Exception ex)
                                                                    {
                                                                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                                                                        Log.Write("Database", "1CSqlQuery", satm1cCommand.CommandText);
                                                                        legal = false;
                                                                    } // Ловим ошибки в запросе событий 1С
                                                                } // Деструктор запроса событий 1С
                                                                #endregion
                                                                events.Add(new Event(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, Guid.NewGuid()));
                                                                //InsertInto(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, Guid.NewGuid());
                                                            } // Был ли грузовик?
                                                        } // Если вЫезд  НЕ замкнут 
                                                    } // Если разомкнулись на вЫезд
                                                } // Если разомкнулись
                                            } // Пока парсим лучи
                                        } //Деструктор ридера событий
                                    } // Пробуем запрос парсера
                                    catch (Exception ex)
                                    {
                                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                                        Log.Write("Database", ex.Source, satmActionCommand.CommandText);
                                        Console.WriteLine("Connection: " + satmSqlConnection.ConnectionTimeout.ToString() + "\r\nEntry: " + satmEntryCommand.CommandTimeout.ToString() + "\r\nAction: " + satmActionCommand.CommandTimeout.ToString());
                                        Console.WriteLine(satmSqlConnection.ConnectionString);
                                        return;
                                    } // Ловим ошибки в запросе парсера
                                    #endregion
                                    if (events.Count > 0)// && !events[0].begin.ToString("yyyy-MM-dd HH:mm:ss").Contains(events[events.Count - 1].end.ToString("yyyy-MM-dd HH:mm:ss")))
                                    {
                                        //Console.WriteLine("{0}  -  {1}  :::  {2}", events[0].begin.ToString("yyyy-MM-dd HH:mm:ss.fff"), events[events.Count - 1].end.ToString("yyyy-MM-dd HH:mm:ss.fff"), events[0].begin.CompareTo(events[events.Count - 1].end));
                                        Log.Write("AP.SATM.Brain", "ProcessedData", "Got " + events.Count.ToString() + " events on " + Server + "\\" + satmEntryReader.GetString(2));
                                        InsertInto(events);
                                        Log.Write("AP.SATM.Brain", "ProcessedData", "Set processed rows.");
                                        SetProcessedData(Server, satmEntryReader.GetInt32(3), satmEntryReader.GetInt32(4), events[0].begin, events[events.Count - 1].end);
                                        events.Clear();
                                    }
                                    //SetTrueToProtocol(pk, Server);
                                    //pk.Clear();
                                } // Запрос парсера
                                #endregion
                            } // Пока читаем проезды
                        } // Деструктор ридера проездов
                    } // Пробуем читать проезды
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        Log.Write("Database", "EntrySqlQuery", satmEntryCommand.CommandText);
                        return;
                    } // Ловим ошибки в запросе проездов
                } // Деструктор запроса проездов
            } // Деструктор соединения с БД
        }

        static void InsertInto(List<Event> events)
        {
            //Log.Write("Events", "Insert", events.Count + " events processed.");
            using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.databaseConnectionString))
            {
                try
                {
                    satmSqlConnection.Open();
                }
                catch (Exception ex)
                {
                    Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                    Log.Write("Database", "SqlConnection", satmSqlConnection.ConnectionString);
                    return;
                } // Ловим ошибки подключения
                foreach (Event satmEvent in events)
                {
                    using (SqlCommand satmEventCommand = new SqlCommand())
                    {
                        satmEventCommand.CommandText = "INSERT INTO [satm].[dbo].[Events] ([owner],[entry],[direction],[startDate]";
                        satmEventCommand.CommandText += ",[endDate],[1cID],[carID],[Culture],[WHO],[legal],[pk])";
                        satmEventCommand.CommandText += " VALUES('" + satmEvent.owner + "', '" + satmEvent.entry + "', '" + satmEvent.direction + "', @sd, @ed, ";
                        satmEventCommand.CommandText += "'" + satmEvent.id + "', '" + satmEvent.carId + "', '" + satmEvent.culture + "', '" + satmEvent.who + "', @legal, '" + satmEvent.uid.ToString() + "')";
                        satmEventCommand.Connection = satmSqlConnection;
                        satmEventCommand.Parameters.Add("@sd", SqlDbType.DateTime).Value = satmEvent.begin;
                        satmEventCommand.Parameters.Add("@ed", SqlDbType.DateTime).Value = satmEvent.end;
                        if (satmEvent.legal)
                        {
                            satmEventCommand.Parameters.Add("@legal", SqlDbType.Bit).Value = 1;
                            Console.WriteLine(DateTime.Now.ToString() + ": Got legal motion event on " + satmEvent.owner + "\\" + satmEvent.entry + " (" + satmEvent.begin + ")");
                        }
                        else
                        {
                            satmEventCommand.Parameters.Add("@legal", SqlDbType.Bit).Value = 0;
                            Console.WriteLine(DateTime.Now.ToString() + ": Got illegal motion event on " + satmEvent.owner + "\\" + satmEvent.entry + " (" + satmEvent.begin + ")");
                        }
                        try
                        {
                            satmEventCommand.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                            Log.Write("Database", "InsertEventQuery", satmEventCommand.CommandText);
                            return;
                        } // Ловим ошибки записи в таблицу событий
                    } // foreach
                } // Деструктор запроса на добавление события
            } // Деструктор соединения БД
        }

        static void SetProcessedData(string owner, int inRay, int outRay, DateTime beginFrom, DateTime endTo)
        {
            //Log.Write("ProcessedResult", owner, keys.Count + " rows processed.");
            using (SqlConnection satmUpdConnection = new SqlConnection(Properties.Settings.Default.databaseConnectionString))
            {
                try
                {
                    satmUpdConnection.Open();
                }
                catch (Exception connectionEx)
                {
                    Log.Write(connectionEx.TargetSite.DeclaringType.ToString(), connectionEx.TargetSite.Name, connectionEx.Message);
                    return;
                } // Ловим ошибки подключения
                using (SqlCommand satmUpdateProtocol = new SqlCommand())
                {
                    satmUpdateProtocol.Connection = satmUpdConnection;
                    satmUpdateProtocol.CommandTimeout = 300;
                    satmUpdateProtocol.CommandText = "UPDATE [satm].[dbo].[PROTOCOL] SET processed='true' ";
                    satmUpdateProtocol.CommandText += " WHERE owner='" + owner + "' and processed='false' and objid in ('" + inRay.ToString() + "','" + outRay.ToString() + "') ";
                    satmUpdateProtocol.CommandText += " and date>=@beginFrom and date<=@endTo";
                    satmUpdateProtocol.Parameters.Add("@beginFrom", SqlDbType.DateTime).Value = beginFrom;
                    satmUpdateProtocol.Parameters.Add("@endTo", SqlDbType.DateTime).Value = endTo;
                    try
                    {
                        satmUpdateProtocol.ExecuteNonQuery();
                        //Console.WriteLine(satmUpdateProtocol.CommandText.Replace("@beginFrom", satmUpdateProtocol.Parameters[0].Value.ToString()).Replace("@endTo",satmUpdateProtocol.Parameters[1].Value.ToString()));
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        Log.Write("AP.SATM.Brain", "SetProcessedData", satmUpdateProtocol.CommandText);
                        return;
                    } // Ловим ошибки подключения
                } // Деструктор зпроса
            } // деструктора соединения
        }

    }

    class Log
    {
        private static object sync = new object();
        public static void Write(string exDeclaringType, string exName, string exMessage)
        {
            try
            {
                // Путь .\\Log
                string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToLog, string.Format("{0}_{1:dd.MM.yyy}.log",
                AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}.{2}()] {3}\r\n",
                DateTime.Now, exDeclaringType, exName, exMessage);
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
                Console.WriteLine(fullText);
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }
    }

    class Core
    {
        internal string Server { get; set; }
        internal string IP { get; set; }
        internal string SQLServer { get; set; }
        internal string username { get; set; }
        internal string password { get; set; }
        internal string SQLDB { get; set; }
        internal string RaysType { get; set; }
    }

    class Event
    {
        internal string owner { get; private set; }
        internal string entry { get; private set; }
        internal string direction { get; private set; }
        internal DateTime begin { get; private set; }
        internal DateTime end { get; private set; }
        internal string id { get; private set; }
        internal string carId { get; private set; }
        internal string culture { get; private set; }
        internal string who { get; private set; }
        internal bool legal { get; private set; }
        internal Guid uid { get; private set; }
        internal Event(string enumOwner, string enumEntry, string enumDirection,
            DateTime enumBegin, DateTime enumEnd, string enumId, string enumCarId, string enumCulture,
            string enumWho, bool enumLegal, Guid enumUid)
        {
            owner = enumOwner;
            entry = enumEntry;
            direction = enumDirection;
            begin = enumBegin;
            end = enumEnd;
            id = enumId;
            carId = enumCarId;
            culture = enumCulture;
            who = enumWho;
            legal = enumLegal;
            uid = enumUid;
        }
    }
}
