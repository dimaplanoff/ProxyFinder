using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyService
{
    public partial class _Service : ServiceBase
    {
        public static bool exit = false;
        public static object addingSync = new object();
        private MainWorker finder = new MainWorker();
        private OnRemove remover = new OnRemove();

       
        public _Service()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            ThreadPool.SetMaxThreads(80,80);

            finder.StartAsync();
            remover.FindUnactiveAsync();

        }

        protected override void OnStop()
        {
            exit = true;            
            finder = null;
            remover = null;
            this.Dispose();
        }

        public void onDebug()
        {
            OnStart(null);
        }
    }
}
