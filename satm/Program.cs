using System;
using System.ServiceProcess;


namespace AP.SATM.Heart
{
    class Program
    {
        static void Main(string[] args)
        {
            System.ServiceProcess.ServiceBase.Run(new Daemon());
        }
    }
}
