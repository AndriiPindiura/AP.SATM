using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.IO;

namespace satmIGPU
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool connected;

        private Thread startWork;
        private Thread imgInsert;
        private List<Core> CoreConfig;
        private bool working;


        private void Form1_Load(object sender, EventArgs e)
        {
            working = true;
            startWork = new Thread(GetCoreConfig);
            startWork.Start();
            imgInsert = new Thread(InsertImage);
            imgInsert.Start();
        }

        private void InsertImage()
        {
            while (working)
            {
                Thread.Sleep(10000);
                foreach (Core core in CoreConfig)
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
                        using (SqlCommand satmSqlCommand = new SqlCommand())
                        {
                            satmSqlCommand.Connection = satmSqlConnection;
                            satmSqlCommand.CommandText = "SELECT pk  FROM Events ev ";
                            satmSqlCommand.CommandText += " where not exists (select pk from EventImages img where ev.pk=img.pk) and ";
                            satmSqlCommand.CommandText += " (ev.owner='" + core.server + "') order by ev.startDate";
                            try
                            {
                                using (SqlDataReader satmDataReader = satmSqlCommand.ExecuteReader())
                                {
                                    while (satmDataReader.Read())
                                    {
                                        string uid = satmDataReader.GetString(0);
                                        foreach (string img in Directory.GetFiles(Properties.Settings.Default.igmPath, core.server + "-" + uid + "*"))
                                        {
                                            //Log.Write("InsertImage","Found File", img);
                                            using (SqlCommand satmImageCommand = new SqlCommand())
                                            {
                                                satmImageCommand.Connection = satmSqlConnection;
                                                satmImageCommand.CommandText = "INSERT INTO [satm].[dbo].[EventImages] ([owner], [pk], [image]) VALUES ('" + core.server + "', '" + uid + "', @image)";
                                                satmImageCommand.Parameters.Add("@image", SqlDbType.Image);
                                                try
                                                {
                                                    satmImageCommand.Parameters["@image"].Value = File.ReadAllBytes(img);
                                                    satmImageCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                                                    Log.Write("Database", "InsertImageQuery", satmImageCommand.CommandText);
                                                    //return;
                                                } // Ловим ошибки записи фото в БД
                                            } // Деструктор запроса не добавление изображений
                                        } // foreach file in dir
                                    } // while read
                                } // using reader
                            } // try
                            catch (Exception ex)
                            {
                                Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                                Log.Write("DataBase", "satmDataReader", satmSqlCommand.CommandText);
                            } //try
                        } // SQL Command
                    } // using SQL connection
                } //foreach core
            } // while true
        }

        private void GetCoreConfig()
        {
            CoreConfig = new List<Core>();
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
                using (SqlCommand satmSqlCommand = new SqlCommand("SELECT [core],[entryDescription],[enterCamera],[exitCamera],[upCamera],[enterDelay],[exitDelay] FROM [satm].[dbo].[Entries]", satmSqlConnection))
                {
                    try
                    {
                        using (SqlDataReader satmDataReader = satmSqlCommand.ExecuteReader())
                        {
                            while (satmDataReader.Read())
                            {
                                CoreConfig.Add(new Core()
                                {
                                    server = satmDataReader.GetString(0),
                                    entry = satmDataReader.GetString(1),
                                    enterCamera = satmDataReader.GetInt32(2),
                                    exitCamera = satmDataReader.GetInt32(3),
                                    upCamera = satmDataReader.GetInt32(4),
                                    enterDelay = satmDataReader.GetInt32(5),
                                    extiDelay = satmDataReader.GetInt32(6)
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

            foreach (Core core in CoreConfig)
                if (!working)
                    break;
                else
                    ProcessEntry(core.server, core.entry, core.enterCamera, core.exitCamera, core.upCamera, core.enterDelay, core.extiDelay);
            working = true;
            imgInsert.Join();
            Application.Exit();
        }

        private void axConnect(object arg)
        {
            string ip = (string)arg;
            //Thread.Sleep(200);
            //MessageBox.Show(ip);
            if (axCamMonitor1.IsConnected() == 0)
                axCamMonitor1.Connect(ip, "", "", "", 0);
            else
            {
                if (axCamMonitor1.GetCurIP() != ip)
                {
                    axCamMonitor1.Disconnect();
                    axCamMonitor1.Connect(ip, "", "", "", 0);
                }
                else
                    connected = true;
            }
            if (axCamMonitor2.IsConnected() == 0)
                axCamMonitor2.Connect(ip, "", "", "", 0);
            else
            {
                if (axCamMonitor2.GetCurIP() != ip)
                {
                    axCamMonitor2.Disconnect();
                    axCamMonitor2.Connect(ip, "", "", "", 0);
                }
                else
                    connected = true;
            }
            if (axCamMonitor3.IsConnected() == 0)
                axCamMonitor3.Connect(ip, "", "", "", 0);
            else
            {
                if (axCamMonitor3.GetCurIP() != ip)
                {
                    axCamMonitor3.Disconnect();
                    axCamMonitor3.Connect(ip, "", "", "", 0);
                }
                else
                    connected = true;
            }

        }

        private void ProcessEntry(string server, string entry, int enterCamera, int exitCamera, int upCamera, int enterDelay, int exitDelay )
        {
            string coreIP = String.Empty;
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
                using (SqlCommand satmSqlCommand = new SqlCommand("SELECT [IP] FROM [satm].[dbo].[Cores] WHERE [Name]='" + server + "'", satmSqlConnection))
                {
                    try
                    {
                        using (SqlDataReader satmDataReader = satmSqlCommand.ExecuteReader())
                        {
                            while (satmDataReader.Read())
                            {
                                coreIP = satmDataReader.GetString(0);
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
            IPAddress ip = null;
            if (IPAddress.TryParse(coreIP, out ip))
            {
                axConnect(coreIP);
                for (int i = 1; i < 21; i++)
                {
                    //Log.Write("axCamMonitor", "Connect", "Try connect to " + coreIP + ", count: " + i);
                    if (connected)
                        break;
                    Thread.Sleep(1000);
                }
                if (connected)
                {
                    connected = false;
                    Thread.Sleep(2000);
                    axCamMonitor1.ShowCam(enterCamera, 1, 1);
                    axCamMonitor2.ShowCam(exitCamera, 1, 1);
                    axCamMonitor3.ShowCam(upCamera, 1, 1);
                    //Thread.Sleep(2000);
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
                            satmSqlCommand.CommandText = "SELECT direction, startDate, endDate ,pk  FROM Events ev ";
                            satmSqlCommand.CommandText += " where not exists (select pk from EventImages img where ev.pk=img.pk) and ";
                            satmSqlCommand.CommandText += " (ev.owner='" + server + "') and (ev.entry='" + entry + "') ";
                            satmSqlCommand.CommandText += " order by ev.startDate";
                            try
                            {
                                using (SqlDataReader satmDataReader = satmSqlCommand.ExecuteReader())
                                {
                                    while (satmDataReader.Read())
                                    {
                                        if (!working)
                                            break;
                                        //coreIP = satmDataReader.GetString(0);
                                        axCamMonitor1.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + enterCamera + ">");
                                        axCamMonitor2.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + exitCamera + ">");
                                        axCamMonitor3.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + upCamera + ">");
                                        Thread.Sleep(3000);
                                        DateTime eventStart = satmDataReader.GetDateTime(1) + new TimeSpan(0, 0, enterDelay);
                                        DateTime eventEnd = satmDataReader.GetDateTime(2) - new TimeSpan(0, 0, exitDelay);
                                        DateTime eventMiddle = eventStart + new TimeSpan(((eventEnd - eventStart).Ticks) / 2);
                                        //MessageBox.Show("SDate " + satmDataReader[1].ToString() + ":" + eventStart.ToString() + "\r\nEDate " + satmDataReader[2].ToString() + ":" + eventEnd.ToString() + "\r\nMDate" + eventMiddle.ToString());

                                        if (satmDataReader.GetString(0) == "ввоз")
                                        {
                                            axCamMonitor1.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + enterCamera + ">,date<" + eventStart.ToString("dd-MM-yy") + ">,time<" + eventStart.ToString("HH:mm:ss") + ">");
                                            axCamMonitor2.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitCamera + ">,date<" + eventEnd.ToString("dd-MM-yy") + ">,time<" + eventEnd.ToString("HH:mm:ss") + ">");
                                            axCamMonitor3.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + upCamera + ">,date<" + eventMiddle.ToString("dd-MM-yy") + ">,time<" + eventMiddle.ToString("HH:mm:ss") + ">");
                                        }
                                        else
                                        {
                                            axCamMonitor2.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + exitCamera + ">,date<" + eventStart.ToString("dd-MM-yy") + ">,time<" + eventStart.ToString("HH:mm:ss") + ">");
                                            axCamMonitor1.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + enterCamera + ">,date<" + eventEnd.ToString("dd-MM-yy") + ">,time<" + eventEnd.ToString("HH:mm:ss") + ">");
                                            axCamMonitor3.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + upCamera + ">,date<" + eventMiddle.ToString("dd-MM-yy") + ">,time<" + eventMiddle.ToString("HH:mm:ss") + ">");
                                        }
                                        Thread.Sleep(3000);
                                        axCamMonitor1.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + enterCamera + ">,file<" + Properties.Settings.Default.igmPath + server + "-" + satmDataReader.GetString(3) + "-" + enterCamera + ".jpg>");
                                        axCamMonitor2.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + exitCamera + ">,file<" + Properties.Settings.Default.igmPath + server + "-" + satmDataReader.GetString(3) + "-" + exitCamera + ".jpg>");
                                        axCamMonitor3.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + upCamera + ">,file<" + Properties.Settings.Default.igmPath + server + "-" + satmDataReader.GetString(3) + "-" + upCamera + ".jpg>");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex.TargetSite.DeclaringType.ToString(), ex.TargetSite.Name, ex.Message);
                                Log.Write("DataBase", "satmDataReader", satmSqlCommand.CommandText);
                            }
                        } // SQL Command

                        Thread.Sleep(1000);
                    } // using SqlConnection
                } // if connected
                else
                {
                    Log.Write("axCamMonitor", "Connect", "Connection timeout! " + coreIP);
                    return;
                } // if !connected
            } // if legalIP
            else
            {
                Log.Write("satmIGPU", "ProcessEntry", "Illegal core ip address " + coreIP + "!");
                return;
            } // if !legalIP
        }

        private void axCamMonitor1_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            if (e.state == 1)
                connected = true;
            else
                connected = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            working = false;
            this.Text = "Waiting for close! Don't kill proccess!";
            startWork.Join();
            imgInsert.Join();
        }


    }
}
