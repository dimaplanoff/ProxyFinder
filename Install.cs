using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ProxyService
{
    [RunInstaller(true)]
    public partial class Install : System.Configuration.Install.Installer
    {        

        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;

        public Install()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalService;

            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "ProxySearch";
            serviceInstaller.Description = "Search proxy on google and yandex";

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }


    }
}
