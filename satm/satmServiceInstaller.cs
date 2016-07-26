using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;


namespace AP.SATM.Heart
{

    [RunInstaller(true)] 

    public class DaemonInstaller:Installer
    {
        private ServiceProcessInstaller processInstaller; 
        private ServiceInstaller serviceInstaller;
        public DaemonInstaller() 
        { 
            processInstaller = new ServiceProcessInstaller(); 
            serviceInstaller = new ServiceInstaller(); 
            processInstaller.Account = ServiceAccount.LocalSystem; 
            serviceInstaller.StartType = ServiceStartMode.Manual; 
            serviceInstaller.ServiceName = "SATM Core"; //must match CronService.ServiceName 
            Installers.Add(serviceInstaller); 
            Installers.Add(processInstaller); 
        } 
    }
}
