using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Net;
using System.Data.SqlClient;
using System.Security;
using System.IdentityModel.Claims;
using System.ComponentModel;
using System.Timers;


namespace AP.SATM.Heart
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)] //, ConcurrencyMode=ConcurrencyMode.Multiple)]
    public class Core : ICore
    {
        private EventLog log;
        private List<OperationContext> activeSession;
        private List<string> aliveSession;

        private void EoFSession(object sender, EventArgs e)
        {
            //SessionStorage Storage = new SessionStorage();
            EventLog.WriteEntry("SATM Core", "Close Session ID: " + ((IContextChannel)sender).SessionId);
            SessionStorage.Deactivate(((IContextChannel)sender).SessionId);

        }

        public Core()
        {
            log = new EventLog();
            log.Source = "SATM Core";
            log.Log = "Application";
            activeSession = new List<OperationContext>();
            aliveSession = new List<string>();
            Timer heartbeat = new Timer(30000);
            heartbeat.Elapsed += heartbeat_Elapsed;
            heartbeat.Enabled = true;
        }

        void heartbeat_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (OperationContext session in activeSession)
            {
                //if (aliveSession.Exists(x => x == session.SessionId))
                if (!aliveSession.Contains(session.SessionId))
                //if (session.Channel.State == CommunicationState.Opened)
                {
                    if (session.Channel.State == CommunicationState.Opened)
                    {
                        EventLog.WriteEntry("SATM Core", "HeartBeat close inactive session: " + session.SessionId);
                        session.Channel.Close();
                    }
                    activeSession.Remove(session);
                }

            }
            aliveSession.Clear();
        }

        List<Events> AnanlyzeProtocol(SqlConnection satmSqlConnection, string core, string entries, DateTime dateFrom, DateTime dateTo)
        {
            List<Events> events = new List<Events>();
            string coreIp = "error";
            string raysType = "error";
            using (SqlCommand satmCoreSql = new SqlCommand("SELECT TOP 1 [IP],[RaysType] FROM [Cores] WHERE [NAME] = '" + core + "'", satmSqlConnection))
            {
                try
                {
                    coreIp = (string)satmCoreSql.ExecuteScalar();
                    using (SqlDataReader coreDataReader = satmCoreSql.ExecuteReader())
                    {
                        if (coreDataReader.Read())
                        {
                            coreIp = coreDataReader.GetString(0);
                            raysType = coreDataReader.GetString(1);
                        }
                    }

                }
                catch (Exception ex)
                {
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                    log.WriteEntry(satmCoreSql.CommandText, EventLogEntryType.Information);
                    return events;
                }
                log.WriteEntry(satmCoreSql.CommandText, EventLogEntryType.Information);
            }

            using (SqlCommand satmEntryCommand = new SqlCommand("select * from [satm].[dbo].[Entries] where core='" + core + "' and [entryDescription] in (" + entries + ")", satmSqlConnection))
            {
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
                                satmActionCommand.CommandText = "SELECT objid, action, date FROM [satm].[dbo].[PROTOCOL]";
                                satmActionCommand.CommandText += " WHERE owner='" + core + "' and objid in ('" + satmEntryReader.GetInt32(3) + "','" + satmEntryReader.GetInt32(4) + "')";
                                satmActionCommand.CommandText += " AND date>='" + dateFrom.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                satmActionCommand.CommandText += " AND date<='" + dateTo.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                satmActionCommand.CommandText += " AND action IN ('ON', 'OFF') ORDER BY DATE";
                                satmActionCommand.Connection = satmSqlConnection;
                                log.WriteEntry(satmActionCommand.CommandText, EventLogEntryType.Information);
                                #region Try to select from protocol EXECUTEREADER
                                using (SqlDataReader satmActionReader = satmActionCommand.ExecuteReader())
                                {
                                    while (satmActionReader.Read())
                                    {
                                        //pk.Add(satmActionReader.GetString(3));
                                        if (satmActionReader.GetString(1) == raysType)
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
                                                            satm1cCommand.CommandText += " where (owner='" + core + "') and (Date>='" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                                                            satm1cCommand.CommandText += " and (Date<='" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "') and (Entry=" + satmEntryReader.GetInt32(1).ToString() + ")";
                                                            //satm1cCommand.CommandTimeout = Properties.Settings.Default.sqlCommandTimeout;
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
                                                                log.WriteEntry(ex.Message, EventLogEntryType.Error);
                                                                log.WriteEntry(satm1cCommand.CommandText, EventLogEntryType.Information);
                                                                legal = false;
                                                                //return;
                                                            } // Ловим ошибки в запросе событий 1С
                                                        } // Деструктор запроса событий 1С
                                                        #endregion
                                                        Events item = new Events();
                                                        item.owner = core;
                                                        item.entry = satmEntryReader.GetString(2);
                                                        item.direction = direction;
                                                        item.startDate = startDate;
                                                        item.endDate = endDate;
                                                        item.ttn = id;
                                                        item.carID = carID;
                                                        item.culture = culture;
                                                        item.who = who;
                                                        item.legal = legal;
                                                        item.uid = Guid.NewGuid().ToString();
                                                        item.user = "";
                                                        item.ip = coreIp;
                                                        if (item.direction == "ввоз")
                                                        {
                                                            item.enterCam = satmEntryReader.GetInt32(5);
                                                            item.exitCam = satmEntryReader.GetInt32(6);
                                                        }
                                                        else
                                                        {
                                                            item.enterCam = satmEntryReader.GetInt32(6);
                                                            item.exitCam = satmEntryReader.GetInt32(5);
                                                        }
                                                        item.enterTime = item.startDate.AddSeconds(satmEntryReader.GetInt32(8));
                                                        item.exitTime = item.endDate.AddSeconds(satmEntryReader.GetInt32(9) * -1);
                                                        item.upCam = satmEntryReader.GetInt32(7);
                                                        item.upTime = item.startDate.AddTicks((item.endDate - item.startDate).Ticks / 2);
                                                        events.Add(item);
                                                        //log.WriteEntry("Got Event", EventLogEntryType.Information);
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
                                                            satm1cCommand.CommandText += " where (owner='" + core + "') and (Date>='" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                                                            satm1cCommand.CommandText += " and (Date<='" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "') and (Entry=" + satmEntryReader.GetInt32(1).ToString() + ")";
                                                            //satm1cCommand.CommandTimeout = Properties.Settings.Default.sqlCommandTimeout;
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
                                                                log.WriteEntry(ex.Message, EventLogEntryType.Error);
                                                                log.WriteEntry(satm1cCommand.CommandText, EventLogEntryType.Information);
                                                                legal = false;
                                                            } // Ловим ошибки в запросе событий 1С
                                                        } // Деструктор запроса событий 1С
                                                        #endregion
                                                        Events item = new Events();
                                                        item.owner = core;
                                                        item.entry = satmEntryReader.GetString(2);
                                                        item.direction = direction;
                                                        item.startDate = startDate;
                                                        item.endDate = endDate;
                                                        item.ttn = id;
                                                        item.carID = carID;
                                                        item.culture = culture;
                                                        item.who = who;
                                                        item.legal = legal;
                                                        item.uid = Guid.NewGuid().ToString();
                                                        item.user = "";
                                                        item.ip = coreIp;
                                                        if (item.direction == "ввоз")
                                                        {
                                                            item.enterCam = satmEntryReader.GetInt32(5);
                                                            item.exitCam = satmEntryReader.GetInt32(6);
                                                        }
                                                        else
                                                        {
                                                            item.enterCam = satmEntryReader.GetInt32(6);
                                                            item.exitCam = satmEntryReader.GetInt32(5);
                                                        }
                                                        item.enterTime = item.startDate.AddSeconds(satmEntryReader.GetInt32(8));
                                                        item.exitTime = item.endDate.AddSeconds(satmEntryReader.GetInt32(9) * -1);
                                                        item.upCam = satmEntryReader.GetInt32(7);
                                                        item.upTime = item.startDate.AddTicks((item.endDate - item.startDate).Ticks / 2);
                                                        events.Add(item);
                                                        //log.WriteEntry("Got Event", EventLogEntryType.Information);
                                                        //InsertInto(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, Guid.NewGuid());
                                                    } // Был ли грузовик?
                                                } // Если вЫезд  НЕ замкнут 
                                            } // Если разомкнулись на вЫезд
                                        } // Если разомкнулись
                                    } // Пока парсим лучи
                                } //Деструктор ридера событий
                                #endregion
                            } // Запрос парсера
                            #endregion
                        } // Пока читаем проезды
                    } // Деструктор ридера проездов
                } // Пробуем читать проезды
                catch (Exception ex)
                {
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                    log.WriteEntry(satmEntryCommand.CommandText, EventLogEntryType.Information);
                    return events;
                } // Ловим ошибки в запросе проездов
            } // Деструктор запроса проездов
            return events;
        }

        public bool SignIn()
        {
            activeSession.Add(OperationContext.Current);
            OperationContext.Current.Channel.Closed += new EventHandler(EoFSession);
            OperationContext.Current.Channel.Faulted += new EventHandler(EoFSession);
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets == null)
                throw new SecurityException("No claimset service configured wrong");
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0)
                throw new SecurityException("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            if (SessionStorage.IsActive(cert.Thumbprint, OperationContext.Current.SessionId))
            {
                throw new ApplicationException("User is already connected!");
            }
            System.Diagnostics.EventLog.WriteEntry("SATM Core", "New Session. User: " + cert.Thumbprint + " ID: " + OperationContext.Current.SessionId);
            SessionStorage.Activate(OperationContext.Current.SessionId, cert.Thumbprint);
            return true;
        }

        public bool SignOut()
        {
            System.Diagnostics.EventLog.WriteEntry("SATM Core", "SignOut Calling");
            return false;
        }

        public void HeartBeat()
        {
            aliveSession.Add(OperationContext.Current.SessionId);
        }

        public string ARM()
        {
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets == null)
                throw new SecurityException("No claimset service configured wrong");
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0)
                throw new SecurityException("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            using (SqlConnection satmConnection = new SqlConnection(Properties.Settings.Default.satmConnectionString))
            {
                try
                {
                    satmConnection.Open();
                }
                catch (Exception dbex)
                {
                    log.WriteEntry(dbex.Message, System.Diagnostics.EventLogEntryType.Error);
                    log.WriteEntry(satmConnection.ConnectionString, EventLogEntryType.Information);
                    throw new ApplicationException("Відсутня відповідь від бази данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                }
                using (SqlCommand satmUserSql = new SqlCommand())
                {
                    satmUserSql.Connection = satmConnection;
                    satmUserSql.CommandText = "SELECT [fullname] FROM [Users] WHERE [certificate] = '" + cert.Subject.Replace("CN=", "") + "'";
                    try
                    {
                        return (string)satmUserSql.ExecuteScalar();
                    }
                    catch (Exception sqlex)
                    {
                        log.WriteEntry(sqlex.Message, EventLogEntryType.Error);
                        log.WriteEntry(satmUserSql.CommandText, EventLogEntryType.Information);
                        throw new ApplicationException("Помилка при виконнанні запиту до бази данних облікових записів, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                    }
                }// using sql query
            } // using sql connection


        }

        private string SafeGetString(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            else
                return string.Empty;
        }

        public List<Entries> GetEntries()
        {
            List<Entries> entries = new List<Entries>();
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets == null)
                throw new SecurityException("No claimset service configured wrong");
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0)
                throw new SecurityException("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            using (SqlConnection satmConnection = new SqlConnection(Properties.Settings.Default.satmConnectionString))
            {
                try
                {
                    satmConnection.Open();
                }
                catch (Exception dbex)
                {
                    log.WriteEntry(dbex.Message, System.Diagnostics.EventLogEntryType.Error);
                    log.WriteEntry(satmConnection.ConnectionString, EventLogEntryType.Information);
                    throw new ApplicationException("Відсутня відповідь від бази данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                }
                using (SqlCommand satmEntriesSql = new SqlCommand())
                {
                    satmEntriesSql.Connection = satmConnection;
                    satmEntriesSql.CommandText = "SELECT [core],[entryDescription] FROM [Entries] WHERE [objid] IN (SELECT [objectid] FROM [UserRights] WHERE [tree] = 'Entries' AND [gid] IN (SELECT [gid] FROM [Users] WHERE [certificate] = '" + cert.Subject.Replace("CN=", "") + "'))";
                    try
                    {
                        using (SqlDataReader entryReader = satmEntriesSql.ExecuteReader())
                        {
                            while (entryReader.Read())
                            {
                                entries.Add(new Entries { core = SafeGetString(entryReader, 0), entryDescription = SafeGetString(entryReader, 1) });
                            } //while reader.read
                        } // using datareader
                    }
                    catch (Exception sqlex)
                    {
                        log.WriteEntry(sqlex.Message, EventLogEntryType.Error);
                        log.WriteEntry(satmEntriesSql.CommandText, EventLogEntryType.Information);
                        throw new ApplicationException("Помилка при виконнанні запиту до бази данних налаштувань, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                    }
                }// using sql query
            } // using sql connection
            return entries;
        }

        public List<Cores> GetCores()
        {
            List<Cores> cores = new List<Cores>();
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets == null)
                throw new SecurityException("No claimset service configured wrong");
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0)
                throw new SecurityException("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            using (SqlConnection satmConnection = new SqlConnection(Properties.Settings.Default.satmConnectionString))
            {
                try
                {
                    satmConnection.Open();
                }
                catch (Exception dbex)
                {
                    log.WriteEntry(dbex.Message, System.Diagnostics.EventLogEntryType.Error);
                    log.WriteEntry(satmConnection.ConnectionString, EventLogEntryType.Information);
                    throw new ApplicationException("Відсутня відповідь від бази данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                }
                using (SqlCommand satmCoresSql = new SqlCommand())
                {
                    satmCoresSql.Connection = satmConnection;
                    satmCoresSql.CommandText = "SELECT [Name],[Description],[IP] FROM [Cores] WHERE [objid] IN (SELECT [objectid] FROM [UserRights] WHERE [tree] = 'Cores' AND [gid] IN (SELECT [gid] FROM [Users] WHERE [certificate] = '" + cert.Subject.Replace("CN=", "") + "'))";
                    try
                    {
                        using (SqlDataReader coreReader = satmCoresSql.ExecuteReader())
                        {
                            while (coreReader.Read())
                            {
                                cores.Add(new Cores { Name = SafeGetString(coreReader, 0), Description = SafeGetString(coreReader, 1), IP = SafeGetString(coreReader, 2) });
                            } //while reader.read
                        } // using datareader
                    }
                    catch (Exception sqlex)
                    {
                        log.WriteEntry(sqlex.Message, EventLogEntryType.Error);
                        log.WriteEntry(satmCoresSql.CommandText, EventLogEntryType.Information);
                        throw new ApplicationException("Помилка при виконнанні запиту до бази данних налаштувань, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                    }
                }// using sql query
            } // using sql connection
            return cores;
        }

        public List<Events> GetEvents(List<Entries> entries, DateTime from, DateTime to, bool raw)
        {
            List<Events> events = new List<Events>();
            List<string> selectedCores = entries.Select(x => x.core).Distinct().ToList();
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets == null)
                throw new SecurityException("No claimset service configured wrong");
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0)
                throw new SecurityException("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            using (SqlConnection satmConnection = new SqlConnection(Properties.Settings.Default.satmConnectionString))
            {
                try
                {
                    satmConnection.Open();
                }
                catch (Exception dbex)
                {
                    log.WriteEntry(dbex.Message, System.Diagnostics.EventLogEntryType.Error);
                    log.WriteEntry(satmConnection.ConnectionString, EventLogEntryType.Information);
                    throw new ApplicationException("Відсутня відповідь від бази данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                }


                foreach (string core in selectedCores)
                {
                    List<string> rights = new List<string>();
                    using (SqlCommand satmUserSql = new SqlCommand("SELECT DISTINCT [Core] FROM [Entries] WHERE [objid] IN (SELECT [objectid] FROM [UserRights] WHERE [tree] = 'Entries' AND [gid] IN (SELECT [gid] FROM [Users] WHERE [certificate] = '" + cert.Subject.Replace("CN=", "") + "'))", satmConnection))
                    {
                        try
                        {
                            using (SqlDataReader satmUserReader = satmUserSql.ExecuteReader())
                            {
                                while (satmUserReader.Read())
                                    rights.Add(SafeGetString(satmUserReader, 0));
                            } // usin userreader

                        }
                        catch (Exception ex)
                        {
                            log.WriteEntry(ex.Message, EventLogEntryType.Error);
                            log.WriteEntry(satmUserSql.CommandText, EventLogEntryType.Information);
                            throw new ApplicationException("Помилка при виконнанні запиту до облікових данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                        }
                    }// using CheckRights
                    if (!rights.Exists(x => x.EndsWith(core)))
                        break;

                    string selectedEntries = string.Empty;
                    foreach (Entries entry in entries.FindAll(x => x.core.Contains(core)))
                        selectedEntries += String.Format("'{0}',", entry.entryDescription);
                    selectedEntries = selectedEntries.Substring(0, selectedEntries.Length - 1);
                    using (SqlCommand satmEventsSql = new SqlCommand())
                    {
                        satmEventsSql.Connection = satmConnection;
                        satmEventsSql.CommandText = "SELECT * FROM [Events] WHERE ";
                        satmEventsSql.CommandText += "[owner] = '" + core + "' AND ";
                        satmEventsSql.CommandText += "[entry] in (" + selectedEntries + ") AND ";
                        satmEventsSql.CommandText += "[startDate] >= '" + from.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        satmEventsSql.CommandText += "AND [endDate] <= '" + to.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        if (raw)
                            satmEventsSql.CommandText += "AND [Events].[user] IS NULL ";
                        satmEventsSql.CommandText += "ORDER BY [owner],[startDate]";
                        try
                        {
                            using (SqlDataReader satmEventsReader = satmEventsSql.ExecuteReader())
                            {
                                while (satmEventsReader.Read())
                                {
                                    Events item = new Events();
                                    item.owner = satmEventsReader.GetString(0);
                                    item.entry = satmEventsReader.GetString(1);
                                    item.direction = satmEventsReader.GetString(2);
                                    item.startDate = satmEventsReader.GetDateTime(3);
                                    item.endDate = satmEventsReader.GetDateTime(4);
                                    item.ttn = SafeGetString(satmEventsReader, 5);//satmEventsReader.GetString(5);
                                    item.carID = SafeGetString(satmEventsReader, 6);//satmEventsReader.GetString(6);
                                    item.culture = SafeGetString(satmEventsReader, 7);//satmEventsReader.GetString(7);
                                    item.who = SafeGetString(satmEventsReader, 8);//satmEventsReader.GetString(8);
                                    item.legal = satmEventsReader.GetBoolean(9);
                                    item.uid = satmEventsReader.GetString(10);
                                    item.user = SafeGetString(satmEventsReader, 11);//satmEventsReader.GetString(11);
                                    try
                                    {
                                        using (SqlCommand satmCoreSql = new SqlCommand("SELECT [IP] FROM [Cores] WHERE [NAME] = '" + item.owner + "'", satmConnection))
                                        {
                                            item.ip = (string)satmCoreSql.ExecuteScalar();
                                        }
                                        using (SqlCommand satmEntriesSql = new SqlCommand("SELECT [enterCamera], [exitCamera], [upCamera], [enterDelay], [exitDelay] FROM [Entries] WHERE [core] = '" + item.owner + "' and [entryDescription] = '" + item.entry + "'", satmConnection))
                                        {
                                            using (SqlDataReader satmEntryReader = satmEntriesSql.ExecuteReader())
                                            {
                                                satmEntryReader.Read();
                                                if (item.direction == "ввоз")
                                                {
                                                    item.enterCam = satmEntryReader.GetInt32(0);
                                                    item.exitCam = satmEntryReader.GetInt32(1);
                                                    //item.enterTime = item.startDate.AddSeconds(satmEntryReader.GetInt32(3));
                                                    //item.exitTime = item.endDate.AddSeconds(satmEntryReader.GetInt32(4) * -1);
                                                }
                                                else
                                                {
                                                    item.enterCam = satmEntryReader.GetInt32(1);
                                                    item.exitCam = satmEntryReader.GetInt32(0);
                                                    //item.enterTime = item.endDate.AddSeconds(satmEntryReader.GetInt32(3) * -1);
                                                    //item.exitTime = item.startDate.AddSeconds(satmEntryReader.GetInt32(4));
                                                }
                                                item.enterTime = item.startDate.AddSeconds(satmEntryReader.GetInt32(3));
                                                item.exitTime = item.endDate.AddSeconds(satmEntryReader.GetInt32(4) * -1);
                                                item.upCam = satmEntryReader.GetInt32(2);
                                                item.upTime = item.startDate.AddTicks((item.endDate - item.startDate).Ticks / 2);
                                            }
                                        }
                                    }
                                    catch (Exception coresex)
                                    {
                                        log.WriteEntry(coresex.Message, EventLogEntryType.Error);
                                        throw new ApplicationException("Помилка при виконнанні запиту до бази данних налаштувань, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                                    }
                                    events.Add(item);
                                } // while reader.read()
                            } // using EventReader
                        }
                        catch (Exception sqlex)
                        {
                            log.WriteEntry(sqlex.Message, EventLogEntryType.Error);
                            log.WriteEntry(satmEventsSql.CommandText, EventLogEntryType.Information);
                            throw new ApplicationException("Помилка при виконнанні запиту до бази данних подій, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                        }

                    } // using SqlQuery (Events)
                } // foreach CORE in list
            } // using SqlConnection
            return events;
        }

        public List<Events> GetEventsQuery(List<Entries> entries, DateTime from, DateTime to, bool raw)
        {
            List<Events> events = new List<Events>();
            List<string> selectedCores = entries.Select(x => x.core).Distinct().ToList();
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets == null)
                throw new SecurityException("No claimset service configured wrong");
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0)
                throw new SecurityException("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;

            using (SqlConnection satmConnection = new SqlConnection(Properties.Settings.Default.satmConnectionString))
            {
                try
                {
                    satmConnection.Open();
                }
                catch (Exception dbex)
                {
                    log.WriteEntry(dbex.Message, System.Diagnostics.EventLogEntryType.Error);
                    log.WriteEntry(satmConnection.ConnectionString, EventLogEntryType.Information);
                    throw new ApplicationException("Відсутня відповідь від бази данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                }


                foreach (string core in selectedCores)
                {
                    List<string> rights = new List<string>();
                    using (SqlCommand satmUserSql = new SqlCommand("SELECT DISTINCT [Core] FROM [Entries] WHERE [objid] IN (SELECT [objectid] FROM [UserRights] WHERE [tree] = 'Entries' AND [gid] IN (SELECT [gid] FROM [Users] WHERE [certificate] = '" + cert.Subject.Replace("CN=", "") + "'))", satmConnection))
                    {
                        try
                        {
                            using (SqlDataReader satmUserReader = satmUserSql.ExecuteReader())
                            {
                                while (satmUserReader.Read())
                                    rights.Add(SafeGetString(satmUserReader, 0));
                            } // usin userreader

                        }
                        catch (Exception ex)
                        {
                            log.WriteEntry(ex.Message, EventLogEntryType.Error);
                            log.WriteEntry(satmUserSql.CommandText, EventLogEntryType.Information);
                            throw new ApplicationException("Помилка при виконнанні запиту до облікових данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                        }
                    }// using CheckRights
                    if (!rights.Exists(x => x.EndsWith(core)))
                        break;

                    string selectedEntries = string.Empty;
                    foreach (Entries entry in entries.FindAll(x => x.core.Contains(core)))
                        selectedEntries += String.Format("'{0}',", entry.entryDescription);
                    selectedEntries = selectedEntries.Substring(0, selectedEntries.Length - 1);
                    foreach (Events analyzeEvent in AnanlyzeProtocol(satmConnection, core, selectedEntries, from, to))
                        events.Add(analyzeEvent);
                } // foreach CORE in list
            } // using SqlConnection

            return events;
        }




        public void UpdateEvent(string core, string uid)
        {
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets == null)
                throw new SecurityException("No claimset service configured wrong");
            if (OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0)
                throw new SecurityException("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            using (SqlConnection satmConnection = new SqlConnection(Properties.Settings.Default.satmConnectionString))
            {
                try
                {
                    satmConnection.Open();
                }
                catch (Exception dbex)
                {
                    log.WriteEntry(dbex.Message, System.Diagnostics.EventLogEntryType.Error);
                    log.WriteEntry(satmConnection.ConnectionString, EventLogEntryType.Information);
                    throw new ApplicationException("Відсутня відповідь від бази данних, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                }
                using (SqlCommand satmUpdateSql = new SqlCommand("UPDATE [Events] SET [user] = '" + cert.Subject.Replace("CN=", "") + "' WHERE pk='" + uid + "' AND [owner] = '" + core + "'", satmConnection))
                {
                    try
                    {
                        satmUpdateSql.ExecuteNonQuery();
                    }
                    catch (Exception sqlex)
                    {
                        log.WriteEntry(sqlex.Message, EventLogEntryType.Error);
                        log.WriteEntry(satmUpdateSql.CommandText, EventLogEntryType.Information);
                        throw new ApplicationException("Помилка при виконнанні запиту до бази данних налаштувань, подальша робота неможлива!\r\nЗвернітся до служби підтримки!");
                    }
                }// using sql query

            } // using SQLConnection
        }
    }

    public class Events
    {
        public string owner { get; set; }
        public string entry { get; set; }
        public string direction { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string ttn { get; set; }
        public string carID { get; set; }
        public string culture { get; set; }
        public string who { get; set; }
        public bool legal { get; set; }
        public string uid { get; set; }
        public string user { get; set; }
        public string ip { get; set; }
        public int upCam { get; set; }
        public DateTime upTime { get; set; }
        public int enterCam { get; set; }
        public DateTime enterTime { get; set; }
        public int exitCam { get; set; }
        public DateTime exitTime { get; set; }
    }

    public class Entries
    {
        public string core { get; set; }
        public string entryDescription { get; set; }
    }

    public class Cores
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IP { get; set; }
    }

}
