using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                EpsonArmController service1 = new EpsonArmController();
                service1.ListenAndSynch();
            }

            if (Environment.UserInteractive)
            {
                var parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
            }
            else
            {
                ServiceBase[] servicesToRun = {
           new EpsonArmController()
          };
                ServiceBase.Run(servicesToRun);
            }
        }
        //static void Main()
        //{


        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[]
        //    {
        //        new EpsonArmController()
        //    };
        //    ServiceBase.Run(ServicesToRun);
        //}
    }
}
