using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    static class Program
    {
        [STAThread]
        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        static void Main()
        {


#if DEBUG
            _Service myService = new _Service();
            myService.onDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new _Service()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
