using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AP.SATM.gpio
{
    class Program
    {
        private static TcpClient tcpClient;
        static void Main(string[] args)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("217.77.220.8"), 18086);
            bool loop = true;
            while (loop)
            {
                Console.WriteLine("Type to send!");
                string data = Console.ReadLine();
                loop = !data.Contains("q");
                if (data.Length > 7)
                {
                    send(data);
                }
            }
            tcpClient.Close();

        }

        /// <summary>
        /// Sending data
        /// </summary>
        /// <param name="data">Data to send</param>
        private static void send(string data)
        {
            data = data.Remove(8, data.Length - 8);
            try
            {
                Console.WriteLine("Connected with {0}", tcpClient.Client.RemoteEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (tcpClient.Connected)
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamWriter streamWriter = new StreamWriter(networkStream);
                Console.WriteLine("Messege {0} to {1}", data, tcpClient.Client.RemoteEndPoint);
                //streamWriter.WriteLine(data);
                //streamWriter.WriteLine();
                networkStream.Write(Encoding.UTF8.GetBytes(data + "\r\n"), 0, 10);
                //streamWriter.
                streamWriter.Flush();
            }
        }
    }
}
