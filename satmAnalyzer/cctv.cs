using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace satmAnalyzer
{
    enum CallbackOptions
    {
        WithoutVideoFrame = 0x0,
        WithVideoFrame = 0x01,
        WithExtendedParams = 0x02,
        WithInformationLayout = 0x04,
        WithCompressedData = 0x08,
        WithoutDecode = 0x10
    } ;

    public partial class cctv : Form
    {
        public cctv()
        {
            InitializeComponent();
        }

        private string ITVCore;

        private bool flag;

        private string expDir;

        private void AnalizeData(string server, string IP, string RaysType)
        {
            /******** ITV Connect or Return *******/
            flag = false;
            ITVCore = IP;
            richTextBox1.Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText("Connecting to " + server + "(" + ITVCore + ")..." + Environment.NewLine);
            });
            if (!(axCamMonitor1.IsConnected() == 1 && axCamMonitor1.GetCurIP() == ITVCore))
            {
                axCamMonitor1.Connect(ITVCore, "", "", "", 0);
                Thread.Sleep(Properties.Settings.Default.ITVDefaultDelay * 1000);
                if (!flag) Thread.Sleep(Properties.Settings.Default.ITVMaxDelay * 1000);
            }
            if (!flag)
            {
                axCamMonitor1.Disconnect();
                richTextBox1.Invoke((MethodInvoker)delegate {
                    richTextBox1.AppendText(server + "(" + ITVCore + ")" + " core not responding!" + Environment.NewLine);
                });
                Log.Write("ITV.Error", server + "(" + ITVCore + ")", "Connection Timeout.");
                return;
            }
            richTextBox1.Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText("Connected to " + axCamMonitor1.GetCurIP() + Environment.NewLine);
            });
            /******** ITV Connect or Return *******/
            Hunt(server, RaysType, IP);
        }

        private void InsertInto(string _owner, string _entry, string _direction, DateTime _startDate, DateTime _endDate, 
            string _ID, string _carID, string _culture, string _WHO, bool _legal, int _enterCam, int _exitCam, int _upCam, Guid _uid)
        {
            using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.satmDB))
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
                using (SqlCommand satmEventCommand = new SqlCommand())
                {
                    satmEventCommand.CommandText = "INSERT INTO [satm].[dbo].[Events] ([owner],[entry],[direction],[startDate]";
                    satmEventCommand.CommandText += ",[endDate],[1cID],[carID],[Culture],[WHO],[legal],[pk])";
                    satmEventCommand.CommandText += " VALUES('" + _owner + "', '" + _entry + "', '" + _direction + "', @sd, @ed, ";
                    satmEventCommand.CommandText += "'" + _ID + "', '" + _carID + "', '" + _culture + "', '" + _WHO + "', @legal, '" + _uid.ToString() + "')";
                    satmEventCommand.Connection = satmSqlConnection;
                    satmEventCommand.Parameters.Add("@sd", SqlDbType.DateTime).Value = _startDate;
                    satmEventCommand.Parameters.Add("@ed", SqlDbType.DateTime).Value = _endDate;
                    if (_legal)
                        satmEventCommand.Parameters.Add("@legal", SqlDbType.Bit).Value = 1;
                    else
                        satmEventCommand.Parameters.Add("@legal", SqlDbType.Bit).Value = 0;
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

                } // Деструктор запроса на добавление события
                using (SqlCommand satmImageCommand = new SqlCommand())
                {
                    satmImageCommand.Connection = satmSqlConnection;
                    satmImageCommand.CommandText = "INSERT INTO [satm].[dbo].[EventImages] ([owner], [pk], [image]) VALUES ('" + _owner + "', '" + _uid.ToString() + "', @image)";
                    satmImageCommand.Parameters.Add("@image", SqlDbType.Image);
                    try
                    {
                        if (File.Exists(expDir + _uid + "-" + _enterCam + ".jpg"))
                        {
                            satmImageCommand.Parameters["@image"].Value = File.ReadAllBytes(expDir + _uid + "-" + _enterCam + ".jpg");
                            satmImageCommand.ExecuteNonQuery();
                            //File.Delete(expDir + _uid + "-" + _enterCam + ".jpg");
                        }
                        if (File.Exists(expDir + _uid + "-" + _exitCam + ".jpg"))
                        {
                            satmImageCommand.Parameters["@image"].Value = File.ReadAllBytes(expDir + _uid + "-" + _exitCam + ".jpg");
                            satmImageCommand.ExecuteNonQuery();
                            //File.Delete(expDir + _uid + "-" + _exitCam + ".jpg");

                        }
                        if (File.Exists(expDir + _uid + "-" + _upCam + ".jpg"))
                        {
                            satmImageCommand.Parameters["@image"].Value = File.ReadAllBytes(expDir + _uid + "-" + _upCam + ".jpg");
                            satmImageCommand.ExecuteNonQuery();
                            //File.Delete(expDir + _uid + "-" + _upCam + ".jpg");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        Log.Write("Database", "InsertImageQuery", satmImageCommand.CommandText);
                        return;
                    } // Ловим ошибки записи фото в БД
                } // Деструктор запроса не добавление изображений
            } // Деструктор соединения БД
        }

        private void Hunt(string Server, string RaysType, string IP)
        { 
            using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.satmDB))
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
                    try
                    {
                        using (SqlDataReader satmEntryReader = satmEntryCommand.ExecuteReader())
                        { 
                            while (satmEntryReader.Read())
                            {
                                using (SqlCommand satmActionCommand = new SqlCommand())
                                {
                                    bool enterRay = false;
                                    bool exitRay = false;
                                    bool truck = false;
                                    string direction = "";
                                    DateTime startDate = DateTime.Parse("1984-06-01");
                                    DateTime endDate = DateTime.Parse("1986-06-08");
                                    satmActionCommand.CommandText = "SELECT objid, action, date, pk FROM [satm].[dbo].[PROTOCOL]";
                                    satmActionCommand.CommandText += " WHERE (owner='" + Server + "') and (processed='false') and (objtype='GRAY') and (objid in ('" + satmEntryReader.GetInt32(3) + "','" + satmEntryReader.GetInt32(4) + "'))";
                                    satmActionCommand.CommandText += " ORDER BY DATE";
                                    satmActionCommand.Connection = satmSqlConnection;
                                    List<string> pk = new List<string>();
                                    pk.Clear();
                                    try
                                    {
                                        using (SqlDataReader satmActionReader = satmActionCommand.ExecuteReader())
                                        {
                                            while (satmActionReader.Read())
                                            {
                                                pk.Add(satmActionReader.GetString(3));
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
                                                else if (satmActionReader.GetString(1) != "ALARM" && satmActionReader.GetString(1) != "DISARM") // Если разомкнулись
                                                {
                                                    if (satmActionReader.GetString(0) == satmEntryReader.GetInt32(3).ToString())
                                                    {
                                                        enterRay = false;
                                                        if (!exitRay) // Если вЫезд  НЕ замкнут то 
                                                        {
                                                            //DateTime.TryParse(satmActionReader.GetString(2), out endDate);
                                                            if (truck) // Был грузовик?
                                                            {
                                                                endDate = satmActionReader.GetDateTime(2);
                                                                int enterDelay = satmEntryReader.GetInt32(8);
                                                                int exitDelay = satmEntryReader.GetInt32(9);
                                                                int cam1, cam2, cam3 = 0;
                                                                bool legal = false;
                                                                string id = "";
                                                                string carID = "";
                                                                string culture = "";
                                                                string who = "";
                                                                truck = false;
                                                                using (SqlCommand satm1cCommand = new SqlCommand())
                                                                {
                                                                    satm1cCommand.CommandText = "select top 1 [1cID], [carID], [Culture], [WHO] from [satm].[dbo].[satm1C]";
                                                                    satm1cCommand.CommandText += " where (owner='" + Server + "') and (Date>='" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                                                                    satm1cCommand.CommandText += " and (Date<='" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "') and (Entry=" + satmEntryReader.GetInt32(1).ToString() + ")";
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

                                                                Guid uid = Guid.NewGuid();
                                                                if (direction == "ввоз")
                                                                {
                                                                    cam1 = satmEntryReader.GetInt32(5);
                                                                    cam2 = satmEntryReader.GetInt32(6);
                                                                } // Если вВоз
                                                                else
                                                                {
                                                                    cam1 = satmEntryReader.GetInt32(6);
                                                                    cam2 = satmEntryReader.GetInt32(5);
                                                                } // Если вЫвоз
                                                                cam3 = satmEntryReader.GetInt32(7);
                                                                if (cam1 != 0)
                                                                    Snapshot(IP, cam1, startDate.AddSeconds(enterDelay), uid);
                                                                if (cam2 != 0)
                                                                    Snapshot(IP, cam2, endDate.AddSeconds((exitDelay * -1)), uid);
                                                                Snapshot(IP, cam3, startDate + new TimeSpan(((endDate - startDate).Ticks) / 2), uid);
                                                                InsertInto(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, cam1, cam2, cam3, uid);
                                                                using (SqlCommand satmProtocolCommand = new SqlCommand())
                                                                {
                                                                    satmProtocolCommand.Connection = satmSqlConnection;
                                                                    satmProtocolCommand.CommandText = "UPDATE [satm].[dbo].[PROTOCOL] SET [processed] = 'true' WHERE pk=@pk and owner='" + Server + "'";
                                                                    satmProtocolCommand.Parameters.Add("@pk", SqlDbType.NVarChar, 50);
                                                                    foreach (string dbpk in pk)
                                                                    {
                                                                        satmProtocolCommand.Parameters["@pk"].Value = dbpk;
                                                                        satmProtocolCommand.ExecuteNonQuery();
                                                                    }
                                                                    pk.Clear();
                                                                } // Деструктор обновления таблици протокола

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
                                                                int cam1, cam2, cam3 = 0;
                                                                bool legal = false;
                                                                string id = "";
                                                                string carID = "";
                                                                string culture = "";
                                                                string who = "";
                                                                truck = false;
                                                                using (SqlCommand satm1cCommand = new SqlCommand())
                                                                {
                                                                    satm1cCommand.CommandText = "select top 1 [1cID], [carID], [Culture], [WHO] from [satm].[dbo].[satm1C]";
                                                                    satm1cCommand.CommandText += " where (owner='" + Server + "') and (Date>='" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                                                                    satm1cCommand.CommandText += " and (Date<='" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "') and (Entry=" + satmEntryReader.GetInt32(1).ToString() + ")";
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
                                                                Guid uid = Guid.NewGuid();
                                                                if (direction == "ввоз")
                                                                {
                                                                    cam1 = satmEntryReader.GetInt32(5);
                                                                    cam2 = satmEntryReader.GetInt32(6);
                                                                } // Если вВоз
                                                                else
                                                                {
                                                                    cam1 = satmEntryReader.GetInt32(6);
                                                                    cam2 = satmEntryReader.GetInt32(5);
                                                                } // Если вЫвоз
                                                                cam3 = satmEntryReader.GetInt32(7);
                                                                if (cam1 != 0)
                                                                    Snapshot(IP, cam1, startDate.AddSeconds(enterDelay), uid);
                                                                if (cam2 != 0)
                                                                    Snapshot(IP, cam2, endDate.AddSeconds((exitDelay * -1)), uid);
                                                                Snapshot(IP, cam3, startDate + new TimeSpan(((endDate - startDate).Ticks) / 2), uid);
                                                                InsertInto(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, cam1, cam2, cam3, uid);
                                                                using (SqlCommand satmProtocolCommand = new SqlCommand())
                                                                {
                                                                    satmProtocolCommand.Connection = satmSqlConnection;
                                                                    satmProtocolCommand.CommandText = "UPDATE [satm].[dbo].[PROTOCOL] SET [processed] = 'true' WHERE pk=@pk and owner='" + Server + "'"; ;
                                                                    satmProtocolCommand.Parameters.Add("@pk", SqlDbType.NVarChar, 50);
                                                                    foreach (string dbpk in pk)
                                                                    {
                                                                        satmProtocolCommand.Parameters["@pk"].Value = dbpk;
                                                                        satmProtocolCommand.ExecuteNonQuery();
                                                                    }
                                                                    pk.Clear();
                                                                } // Деструктор обновления таблици протокола
                                                            
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
                                        Log.Write("Database", "ActionSqlQuery", satmActionCommand.CommandText);
                                        return;
                                    } // Ловим ошибки в запросе парсера
                                } // Запрос парсера
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

        private void Snapshot(string IP, int Camera, DateTime ArchDate, Guid pk)
        { 
            if (!(axCamMonitor1.IsConnected() == 1 && axCamMonitor1.GetCurIP() == IP))
            {
                flag = false;
                ITVCore = IP;
                axCamMonitor1.Connect(ITVCore, "", "", "", 0);
                Thread.Sleep(Properties.Settings.Default.ITVDefaultDelay * 1000);
                if (!flag) Thread.Sleep(Properties.Settings.Default.ITVMaxDelay * 1000);
            }
            if (!flag)
            {
                axCamMonitor1.Disconnect();
                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    richTextBox1.AppendText("(" + ITVCore + ")" + " core not responding!" + Environment.NewLine);
                });
                Log.Write("ITV.Error", "(" + ITVCore + ")", "Connection Timeout.");
                return;
            }
            /*if (File.Exists(expDir + pk + "-" + Camera + ".jpg"))
                File.Delete(expDir + pk + "-" + Camera + ".jpg");*/
            axCamMonitor1.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + Camera + ">");
            Thread.Sleep(Properties.Settings.Default.ITVShootDelay / 2);
            axCamMonitor1.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + Camera + ">,date<" + ArchDate.ToString("dd-MM-yy") + ">,time<" + ArchDate.ToString("HH:mm:ss") + ">");
            Thread.Sleep(Properties.Settings.Default.ITVShootDelay);
            axCamMonitor1.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + Camera + ">,file<" + expDir + pk + "-" + Camera + ".jpg>");
        }

        private void SynchronizeData(string sqlServer, string login, string password, string DB, string server, string IP)
        {
            using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.satmDB))
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
                string SQL = Properties.Settings.Default.synchrProtocol;
                SQL = SQL.Replace("{@SQLServer}", sqlServer);
                SQL = SQL.Replace("{@username}", login);
                SQL = SQL.Replace("{@password}", password);
                SQL = SQL.Replace("{@Database}", DB);
                DateTime starttime = DateTime.Now;
                try
                {
                    richTextBox1.Invoke((MethodInvoker)delegate
                    {
                        richTextBox1.AppendText("Synchronization.Start at " + server + "..." + Environment.NewLine);
                    });
                }
                catch { }
                using (SqlCommand satmSqlCommand = new SqlCommand(SQL, satmSqlConnection))
                {
                    try
                    {
                        int result = satmSqlCommand.ExecuteNonQuery();
                        Log.Write("Synchronization.Result", server + "(" + sqlServer + ")", "PROTOCOL : (" + result.ToString() + " row(s) affected) " + (DateTime.Now - starttime).ToString());
                        richTextBox1.Invoke((MethodInvoker)delegate
                        {
                            richTextBox1.AppendText("PROTOCOL : (" + result.ToString() + " row(s) affected) " + (DateTime.Now - starttime).ToString() + Environment.NewLine);
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        Log.Write("Synchronization.Error", server + "(" + sqlServer + ").PROTOCOL", SQL);
                        try
                        {
                            richTextBox1.Invoke((MethodInvoker)delegate
                            {
                                richTextBox1.AppendText("PROTOCOL : error" + Environment.NewLine);
                            });
                        }
                        catch { }

                    }
                    SQL = Properties.Settings.Default.synchrSAUP;
                    SQL = SQL.Replace("{@SQLServer}", sqlServer);
                    SQL = SQL.Replace("{@username}", login);
                    SQL = SQL.Replace("{@password}", password);
                    SQL = SQL.Replace("{@Database}", DB);
                    satmSqlCommand.CommandText = SQL;
                    try
                    {
                        int result = satmSqlCommand.ExecuteNonQuery();
                        Log.Write("Synchronization.Result", server + "(" + sqlServer + ")", "1C       : (" + result.ToString() + " row(s) affected) " + (DateTime.Now - starttime).ToString());
                        richTextBox1.Invoke((MethodInvoker)delegate
                        {
                            richTextBox1.AppendText("1C                : (" + result.ToString() + " row(s) affected) " + (DateTime.Now - starttime).ToString() + Environment.NewLine);
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                        Log.Write("Synchronization.Error", server + "(" + sqlServer + ").1C", SQL);
                        try
                        {
                            richTextBox1.Invoke((MethodInvoker)delegate
                            {
                                richTextBox1.AppendText("1C                : error" + Environment.NewLine);
                            });
                        }
                        catch { }
                    }
                } // SQL Command
            } // SQL Connection

        }

        private void StartSynchronize()
        {
            List<Core> CoreConfig = new List<Core>();
            using (SqlConnection satmSqlConnection = new SqlConnection(Properties.Settings.Default.satmDB))
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
                using (SqlCommand satmSqlCommand = new SqlCommand())
                {
                    satmSqlCommand.Connection = satmSqlConnection;
                    if ((Environment.GetCommandLineArgs()).Length == 2)
                        satmSqlCommand.CommandText = "SELECT [Name],[IP],[SQLServer],[Username],[Password],[SQLDB], [RaysType] FROM [satm].[dbo].[Cores] WHERE Name='" + (Environment.GetCommandLineArgs())[1].ToString() + "'";
                    else
                        satmSqlCommand.CommandText = "SELECT [Name],[IP],[SQLServer],[Username],[Password],[SQLDB], [RaysType] FROM [satm].[dbo].[Cores]";
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
                        Application.Exit();
                    }
                } // SQL Command
                //using (SqlCommand satmSqlCommand = new SqlCommand(Properties))
            } // SQL Connection
            DateTime sdate = DateTime.Now;
            Log.Write("SATM", "Analyzer", "Start working at " + sdate.ToString());
            foreach (Core core in CoreConfig)
            {
                try
                {
                    this.Text = core.Server;
                }
                catch { }
                SynchronizeData(core.SQLServer, core.username, core.password, core.SQLDB, core.Server, core.IP);
            }
            foreach (Core core in CoreConfig)
            {
                try
                {
                    this.Text = core.Server;
                }
                catch { }
                AnalizeData(core.Server, core.IP, core.RaysType);
            }
            Log.Write("SATM", "Analyzer", "Stop working at " + DateTime.Now.ToString());
            Application.Exit();
        }

        private void cctv_Load(object sender, EventArgs e)
        {
            expDir = Properties.Settings.Default.ImagePath;
            Log.Write("SATM", "Environment", "Export Path: " + expDir);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Thread synchrDB = new Thread(StartSynchronize);
            for (int i =1; i<256; i++)
            {
                axCamMonitor1.SetCallBackOptions(i, (int)(CallbackOptions.WithVideoFrame));
            }
            synchrDB.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }

        private void axCamMonitor1_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            if (e.state == 1)
            {
                Thread goImage = new Thread(_captureFrame);
                goImage.Start();
            }
            else
            {
                flag = false;
                axCamMonitor1.Connect(ITVCore, "", "", "", 0);
            }
        }

        private void _captureFrame()
        {
            Thread.Sleep(1000);
            try
            {
                axCamMonitor1.Invoke((MethodInvoker)delegate
                {
                    axCamMonitor1.ShowCam(1, 1, 1);
                });
            }
            catch
            {
            }
        }

        private void axCamMonitor1_OnVideoFrame(object sender, AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEvent e)
        {
            /*ms = new MemoryStream((byte[])e.frame);
            richTextBox1.AppendText(ms.Length.ToString() + Environment.NewLine);*/
            flag = true;
            //axCamMonitor1.SendRawMessage("CAM|1|ADD_SUBTITLES|command<" + DateTime.Now.ToString() + ">,page<BEGIN>,title_id<1>");
            //axCamMonitor1.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<1>,file<123.jpg>");
            //axCamMonitor1.SendRawMessage("CAM|1|CLEAR_SUBTITLES|title_id<1>");
        }

        private void cctv_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void axCamMonitor1_DblClick(object sender, AxACTIVEXLib._DCamMonitorEvents_DblClickEvent e)
        {
            Application.Exit();
        }
    }

    public class Core
    {
        internal string Server { get; set; }
        internal string IP { get; set; }
        internal string SQLServer { get; set; }
        internal string username { get; set; }
        internal string password { get; set; }
        internal string SQLDB { get; set; }
        internal string RaysType { get; set; }
    }

}
