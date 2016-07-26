using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace satmIGPU
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
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
        internal string server { get; set; }
        internal string entry { get; set; }
        internal int enterCamera { get; set; }
        internal int exitCamera { get; set; }
        internal int upCamera { get; set; }
        internal int enterDelay { get; set; }
        internal int extiDelay { get; set; }
    }
}
