using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace AP.SATM.RaySpider
{
    [RunInstaller(true)]
    public partial class raysServiceInstaller : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;
        public raysServiceInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();
            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "SATM Rays";
            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
            //InitializeComponent();
        }
    }
}
