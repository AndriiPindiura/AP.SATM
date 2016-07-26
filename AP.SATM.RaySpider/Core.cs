using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AP.SATM.RaySpider
{
    class Core
    {
        private bool work;
        private static EventLog log;
        private Database database;

        public Core()
        {
            log = new EventLog();
            log.Source = "SATM Rays";
            log.Log = "Application";
        }

        internal void StopCaptureEvents()
        {
            work = false;
            WriteCSV();
        }

        internal void UpdateDb()
        {
            database.UpdateFromBuffer();
        }

        /*private static void GetEventsTcp(Object StateInfo)
        {
            new EventsTCP((TcpClient)StateInfo);
        }*/

        internal void WriteCSV()
        {
            try
            {

                string pathToCsv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "buffer");
                if (!Directory.Exists(pathToCsv))
                    Directory.CreateDirectory(pathToCsv); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToCsv, string.Format("{0}_{1:dd.MM.yyy}.csv",
                AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
                /*Type itemType = typeof(T);
                var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .OrderBy(p => p.Name);*/
                using (var writer = new StreamWriter(filename))
                {
                    foreach (IntellectOutput rayEvent in database.Buffer)
                    {
                        writer.WriteLine(String.Format("{0};{1};{2};{3}", rayEvent.owner, rayEvent.objId, rayEvent.action ? "ON" : "OFF", rayEvent.date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)));
                    }
                    /*writer.WriteLine(string.Join(";", props.Select(p => p.Name)));

                    foreach (var item in items)
                    {
                        writer.WriteLine(string.Join(";", props.Select(p => p.GetValue(item, null))));
                    }*/
                }
            }
            catch (Exception ex)
            {
                log.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }

        private void TraceWrite(IntellectOutput rayEvent)
        {
            try
            {

                string pathToCsv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trace");
                if (!Directory.Exists(pathToCsv))
                    Directory.CreateDirectory(pathToCsv); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToCsv, string.Format("{0}_{1:dd.MM.yyy}.csv",
                AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
                File.AppendAllText(filename, String.Format("{0};{1};{2};{3}\r\n", rayEvent.owner, rayEvent.objId, rayEvent.action ? "ON" : "OFF", rayEvent.date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), Encoding.GetEncoding("Windows-1251"));
            }
            catch
            { }
        }

        private void InputWrite(string data, string device)
        {
            try
            {

                string pathToCsv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input");
                if (!Directory.Exists(pathToCsv))
                    Directory.CreateDirectory(pathToCsv); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToCsv, string.Format("{0}_{1:dd.MM.yyy}.csv",
                device, DateTime.Now));
                File.AppendAllText(filename, String.Format("[{0:dd.MM.yyy HH:mm:ss.fff}],{1}\r\n", DateTime.Now, data.Trim()));
            }
            catch
            { }
        }

        internal static ManualResetEvent clientConnected = new ManualResetEvent(false);

        private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            try
            {

                TcpListener listener = (TcpListener)ar.AsyncState;
                Socket client = listener.EndAcceptSocket(ar);
                clientConnected.Set();
                //Thread tcpClient = new Thread(new ParameterizedThreadStart(GetEventsTcp));
                //tcpClient.Start(client);

                #region Without Thread
                IntellectOutput dm = new IntellectOutput();
                dm.owner = Properties.Settings.Default.Owner;
                string clientIP;
                byte[] bytes = new byte[10];

                //client.Receive(bytes);
                NetworkStream stream = new NetworkStream(client);
                if (client.Connected)
                {
                    // ((IPEndPoint)client.RemoteEndPoint).
                    clientIP = (((IPEndPoint)client.RemoteEndPoint).Address).ToString();
                    if (Variables.Config.Any(x => x.deviceIp == (clientIP)))
                    {
                        
                        int i;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            InputWrite(Encoding.ASCII.GetString(bytes), clientIP);
                            foreach (IOConfiguration ray in Variables.Config.Where(x => (x.deviceIp == clientIP)))
                            {
                                dm.date = DateTime.Now;
                                dm.objId = ray.rayId.ToString();
                                dm.action = (Encoding.UTF8.GetString(bytes, ray.ioId - 1, 1) == "1") ? true : false;
                                if (Properties.Settings.Default.Debug)
                                {
                                    Log.Write(String.Format("Ray #{0} is {1} was {2} {3}", dm.objId, dm.action ? "ON" : "OFF", ray.ioState ? "ON" : "OFF", (dm.action != ray.ioState)));
                                }
                                if (ray.ioState != dm.action)
                                {
                                    TraceWrite(dm);
                                    Variables.Config.Where(x => (x.deviceIp == clientIP && x.ioId == ray.ioId)).First().ioState = dm.action;
                                    using (SqlConnection sql = new SqlConnection(Properties.Settings.Default.ConnectionString))
                                    {
                                        //sql.Open();

                                        try
                                        {
                                            sql.Open();
                                        }
                                        catch (Exception ex)
                                        {
                                            log.WriteEntry(ex.Message, EventLogEntryType.Error);
                                        }

                                        database.InsertIntoProtocol(sql, dm);
                                    }// using SQLConnection

                                } // if (ray.ioState != dm.action)
                            } // foreach
                        } // while
                    } // if (config.Any(x => x.deviceIp == (clientIP)))
                }
                //client.GetStream().Close();
                client.Close();
            }
            catch (Exception ex)
            {
                log.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
            #endregion


        }

        private void DoAcceptUdpClientCallback(IAsyncResult ar)
        {
            try
            {
                UdpClient udpClient = (UdpClient)ar.AsyncState;
                IPEndPoint udpSender = new IPEndPoint(IPAddress.Any, 0);
                clientConnected.Set();
                using (SqlConnection sql = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    sql.Open();

                    IntellectOutput dm = new IntellectOutput();
                    dm.owner = Properties.Settings.Default.Owner;

                    byte[] bytes = new byte[1024];
                    bytes = udpClient.EndReceive(ar, ref udpSender);
                    //bytes = udpClient.Receive(ref udpSender);
                    string clientIP = udpSender.Address.ToString();
                    if (Variables.Config.Any(x => x.deviceIp == (clientIP)))
                    {
                        foreach (IOConfiguration ray in Variables.Config.Where(x => (x.deviceIp == clientIP)))
                        {
                            dm.date = DateTime.Now;
                            dm.objId = ray.rayId.ToString();
                            dm.action = (Encoding.UTF8.GetString(bytes, ray.ioId - 1, 1) == "1") ? true : false;
                            if (Properties.Settings.Default.Debug)
                            {
                                Log.Write(String.Format("Ray #{0} is {1} was {2} {3}", dm.objId, dm.action ? "ON" : "OFF", ray.ioState ? "ON" : "OFF", (dm.action != ray.ioState)));
                            }
                            if (ray.ioState != dm.action)
                            {
                                TraceWrite(dm);
                                Variables.Config.Where(x => (x.deviceIp == clientIP && x.ioId == ray.ioId)).First().ioState = dm.action;
                                database.InsertIntoProtocol(sql, dm);
                            } // if (ray.ioState != dm.action)
                        } // foreach

                    } // if (config.Any(x => x.deviceIp == (clientIP)))
                }// using
            }
            catch (Exception ex)
            {
                log.WriteEntry(ex.Message, EventLogEntryType.Error);
                log.WriteEntry(Properties.Settings.Default.ConnectionString, EventLogEntryType.Information);
                //return;
            }

        }

        public void StartCaptureEvents()
        {
            database = new Database();
            IntellectOutput dm = new IntellectOutput();
            dm.owner = Properties.Settings.Default.Owner;
            using (SqlConnection sql = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                try
                {
                    sql.Open();
                }
                catch (Exception ex)
                {
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                    log.WriteEntry(Properties.Settings.Default.ConnectionString, EventLogEntryType.Information);
                    return;
                }
                try
                {
                    foreach (string rayConfig in Properties.Settings.Default.Config)
                    {
                        string[] cfg = rayConfig.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        Variables.Config.Add(new IOConfiguration { deviceIp = cfg[0], ioId = Int32.Parse(cfg[1]), rayId = Int32.Parse(cfg[2]), ioState = database.SelectLastState(sql, dm.owner, cfg[2]) });
                    }
                }
                catch (Exception ex)
                {
                    log.WriteEntry("Configuration load error:", EventLogEntryType.Error);
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                }
            } // using SQLConnection
            /*string clientIP;
            Byte[] bytes = new Byte[256];*/
            work = true;
            #region TCP Mode
            if (Properties.Settings.Default.Protocol.ToUpper().Contains("TCP"))
            {
                TcpListener tcpServer = new TcpListener(IPAddress.Any, Properties.Settings.Default.Port);
                try
                {
                    tcpServer.Start();
                }
                catch (Exception ex)
                {
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                }
                while (work)
                {
                    //Thread.Sleep(5);
                    tcpServer.BeginAcceptSocket(new AsyncCallback(DoAcceptTcpClientCallback), tcpServer);
                    //    .BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), tcpServer);
                    clientConnected.WaitOne();
                    clientConnected.Reset();
                    /*if (!captureServer.Pending())
                    {
                        continue;
                    }*/
                    /*var tcpThread = new Thread(() =>
                    {*/
                    /*
                    TcpClient client = captureServer.AcceptTcpClient();
                    Thread tcpClient = new Thread(new ParameterizedThreadStart(GetEventsTcp));
                    tcpClient.Start(client);
                    */
                    //using (TcpClient client = captureServer.AcceptTcpClient())
                    //{
                    /*NetworkStream stream = client.GetStream();
                    clientIP = (((IPEndPoint)client.Client.RemoteEndPoint).Address).ToString();
                    if (config.Any(x => x.deviceIp == (clientIP)))
                    {
                        int i;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            foreach (IOConfiguration ray in config.Where(x => (x.deviceIp == clientIP)))
                            {
                                dm.date = DateTime.Now;
                                dm.objId = ray.rayId.ToString();
                                dm.action = (Encoding.UTF8.GetString(bytes, ray.ioId - 1, 1) == "1") ? true : false;
                                if (Properties.Settings.Default.Debug)
                                {
                                    Log.Write(String.Format("Ray #{0} is {1} was {2} {3}", dm.objId, dm.action ? "ON" : "OFF", ray.ioState ? "ON" : "OFF", (dm.action != ray.ioState)));
                                }
                                if (ray.ioState != dm.action)
                                {
                                    config.Where(x => (x.deviceIp == clientIP && x.ioId == ray.ioId)).First().ioState = dm.action;
                                    if (buffer.Count > 0)
                                    {
                                        if (buffer.Count > 10240)
                                        {
                                            buffer.Clear();
                                        }
                                        foreach (IntellectOutput recentEvent in buffer)
                                        {
                                            if (dbTools.InsertIntoProtocol(sql, recentEvent))
                                            {
                                                buffer.Remove(recentEvent);
                                            }
                                        }
                                    }
                                    if (!dbTools.InsertIntoProtocol(sql, dm))
                                    {
                                        buffer.Add(dm);
                                    }
                                } // if (ray.ioState != dm.action)
                            } // foreach
                        } // while
                    } // if (config.Any(x => x.deviceIp == (clientIP)))
                    client.GetStream().Close();
                    client.Close();*/
                    //} // using TcpClient
                    /* }); // Thread
                     tcpThread.Start();*/
                } // while WORKFLAG
                tcpServer.Stop();
            }// if TCP
            #endregion
            #region UDP Mode
            else
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, Properties.Settings.Default.Port);
                UdpClient udpClient = new UdpClient(ipep);
                //IPEndPoint udpSender = new IPEndPoint(IPAddress.Any, 0);
                /*using (SqlConnection sql = new SqlConnection(Properties.Settings.Default.ConnectionString))
                {
                    try
                    {
                        sql.Open();
                    }
                    catch (Exception ex)
                    {
                        log.WriteEntry(ex.Message, EventLogEntryType.Error);
                        log.WriteEntry(Properties.Settings.Default.ConnectionString, EventLogEntryType.Information);
                        return;
                    }*/

                while (work)
                {
                    udpClient.BeginReceive(new AsyncCallback(DoAcceptUdpClientCallback), udpClient);
                    clientConnected.WaitOne();
                    clientConnected.Reset();
                    /*Thread.Sleep(5);
                    if (udpClient.Available <= 0)
                    {
                        continue;
                    }*/
                    /*bytes = udpClient.Receive(ref udpSender);
                    clientIP = udpSender.Address.ToString();
                    if (Variables.Config.Any(x => x.deviceIp == (clientIP)))
                    {
                        foreach (IOConfiguration ray in Variables.Config.Where(x => (x.deviceIp == clientIP)))
                        {
                            dm.date = DateTime.Now;
                            dm.objId = ray.rayId.ToString();
                            dm.action = (Encoding.UTF8.GetString(bytes, ray.ioId - 1, 1) == "1") ? true : false;
                            if (Properties.Settings.Default.Debug)
                            {
                                Log.Write(String.Format("Ray #{0} is {1} was {2} {3}", dm.objId, dm.action ? "ON" : "OFF", ray.ioState ? "ON" : "OFF", (dm.action != ray.ioState)));
                            }
                            if (ray.ioState != dm.action)
                            {
                                Variables.Config.Where(x => (x.deviceIp == clientIP && x.ioId == ray.ioId)).First().ioState = dm.action;
                                if (Variables.Buffer.Count > 0)
                                {
                                    if (Variables.Buffer.Count > 10240)
                                    {
                                        Variables.Buffer.Clear();
                                    }
                                    foreach (IntellectOutput recentEvent in Variables.Buffer)
                                    {
                                        if (Database.InsertIntoProtocol(sql, recentEvent))
                                        {
                                            Variables.Buffer.Remove(recentEvent);
                                        }
                                    }
                                }
                                if (!Database.InsertIntoProtocol(sql, dm))
                                {
                                    Variables.Buffer.Add(dm);
                                }
                            } // if (ray.ioState != dm.action)
                        } // foreach

                    } // if (config.Any(x => x.deviceIp == (clientIP)))*/
                } // while WORKFLAG
                //}// using SQLConnection
            } // if UDB
            #endregion
        } // public void StartCaptureEvents()
    } // Class

    /*class EventsTCP
    {
        public EventsTCP(TcpClient client)
        {
            EventLog log = new EventLog();
            log.Source = "SATM Rays";
            log.Log = "Application";
            string clientIP;
            Byte[] bytes = new Byte[256];
            IntellectOutput dm = new IntellectOutput();
            dm.owner = Properties.Settings.Default.Owner;
            using (SqlConnection sql = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                try
                {
                    sql.Open();
                }
                catch (Exception ex)
                {
                    log.WriteEntry(ex.Message, EventLogEntryType.Error);
                    log.WriteEntry(Properties.Settings.Default.ConnectionString, EventLogEntryType.Information);
                    return;
                }

                NetworkStream stream = client.GetStream();
                clientIP = (((IPEndPoint)client.Client.RemoteEndPoint).Address).ToString();
                if (Variables.Config.Any(x => x.deviceIp == (clientIP)))
                {
                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        foreach (IOConfiguration ray in Variables.Config.Where(x => (x.deviceIp == clientIP)))
                        {
                            dm.date = DateTime.Now;
                            dm.objId = ray.rayId.ToString();
                            dm.action = (Encoding.UTF8.GetString(bytes, ray.ioId - 1, 1) == "1") ? true : false;
                            if (Properties.Settings.Default.Debug)
                            {
                                Log.Write(String.Format("Ray #{0} is {1} was {2} {3}", dm.objId, dm.action ? "ON" : "OFF", ray.ioState ? "ON" : "OFF", (dm.action != ray.ioState)));
                            }
                            if (ray.ioState != dm.action)
                            {
                                Variables.Config.Where(x => (x.deviceIp == clientIP && x.ioId == ray.ioId)).First().ioState = dm.action;
                                if (Variables.Buffer.Count > 0)
                                {
                                    if (Variables.Buffer.Count > 10240)
                                    {
                                        Variables.Buffer.Clear();
                                    }
                                    foreach (IntellectOutput recentEvent in Variables.Buffer)
                                    {
                                        if (Database.InsertIntoProtocol(sql, recentEvent))
                                        {
                                            Variables.Buffer.Remove(recentEvent);
                                        }
                                    }
                                }
                                if (!Database.InsertIntoProtocol(sql, dm))
                                {
                                    Variables.Buffer.Add(dm);
                                }
                            } // if (ray.ioState != dm.action)
                        } // foreach
                    } // while
                } // if (config.Any(x => x.deviceIp == (clientIP)))
                client.GetStream().Close();
                client.Close();
            }// using SQLConnection
            Core.clientConnected.Set();
        }
    }*/

    class Log
    {
        private static object sync = new object();
        public static void Write(string exMessage)
        {
            try
            {
                // Путь .\\Log
                string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToLog, string.Format("{0}_{1:dd.MM.yyy}.log",
                AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1}\r\n",
                DateTime.Now, exMessage);
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

}
