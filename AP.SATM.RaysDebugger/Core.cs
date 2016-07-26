using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AP.SATM.RaysDebugger
{
    class Core
    {
        public static void CaptureEvents()
        {
            List<IOConfiguration> config = new List<IOConfiguration>();
            List<IntellectOutput> buffer = new List<IntellectOutput>();
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
                    Console.WriteLine(ex);
                }

                foreach (string rayConfig in Properties.Settings.Default.Config)
                {
                    string[] cfg = rayConfig.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    config.Add(new IOConfiguration { deviceIp = cfg[0], ioId = Int32.Parse(cfg[1]), rayId = Int32.Parse(cfg[2]), ioState = Database.SelectLastState(sql, dm.owner, cfg[2]) });
                }

                //TcpListener captureServer = new TcpListener(IPAddress.Any, Properties.Settings.Default.Port);
                //captureServer.Start();

                byte[] data = new byte[1024];

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 18086);
                UdpClient newsock = new UdpClient(ipep);
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);


                Console.WriteLine("Server start!");
                string clientIP;
                Byte[] bytes = new Byte[256];
                
                while (true)
                {
                    if (newsock.Available <= 0)
                    {
                        //Console.Write(".");
                        continue;
                    }
                    //Console.Write(".");
                    data = newsock.Receive(ref sender);
                    
                    clientIP = sender.Address.ToString();
                    if (config.Any(x => x.deviceIp == (clientIP)))
                    {
                        foreach (IOConfiguration ray in config.Where(x => (x.deviceIp == clientIP)))
                        {
                            dm.date = DateTime.Now;
                            dm.objId = ray.rayId.ToString();
                            dm.action = (Encoding.UTF8.GetString(bytes, ray.ioId - 1, 1) == "1") ? true : false;
                            Console.WriteLine("Ray #{0} is {1} was {2} {3}", dm.objId, dm.action ? "ON" : "OFF", ray.ioState ? "ON" : "OFF", (dm.action != ray.ioState));
                            /*if (Properties.Settings.Default.Debug)
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
                            } // if (ray.ioState != dm.action)*/
                        } // foreach

                    } // if (config.Any(x => x.deviceIp == (clientIP)))


                    Console.WriteLine(clientIP);
                    Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));
                    //newsock.Send(data, data.Length, sender);


                    //TcpClient client = captureServer.AcceptTcpClient();
                    //NetworkStream stream = client.GetStream();
                    /*clientIP = (((IPEndPoint)client.Client.RemoteEndPoint).Address).ToString();
                    if (config.Any(x => x.deviceIp == (clientIP)))
                    {
                        int i;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            string data = Encoding.UTF8.GetString(bytes, 0, i);
                            Console.WriteLine("Received: {0}", data);
                            foreach (IOConfiguration ray in config.Where(x => (x.deviceIp == clientIP)))
                            {
                                dm.date = DateTime.Now;
                                dm.objId = ray.rayId.ToString();
                                dm.action = (Encoding.UTF8.GetString(bytes, ray.ioId - 1, 1) == "1") ? true : false;
                                Console.WriteLine("Ray #{0} is {1} (last state is {2})", dm.objId, dm.action ? "ON" : "OFF", ray.ioState ? "ON" : "OFF");
                                if (ray.ioState != dm.action)
                                {
                                    Console.WriteLine("Updating DB and state");
                                    config.Where(x => (x.deviceIp == clientIP && x.ioId == ray.ioId)).First().ioState = dm.action;
                                    if (buffer.Count > 0)
                                    {
                                        foreach (IntellectOutput recentEvent in buffer)
                                        {
                                            if (Database.InsertIntoProtocol(sql, recentEvent))
                                            {
                                                buffer.Remove(recentEvent);
                                            }
                                        }
                                    }
                                    if (!Database.InsertIntoProtocol(sql, dm))
                                    {
                                        buffer.Add(dm);
                                    }
                                }
                            }
                        }
                    }
                    client.Close();*/
                }
            }
        }
    }
}
