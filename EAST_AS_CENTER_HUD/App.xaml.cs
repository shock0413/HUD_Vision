
using PylonC.NET;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EAST_AS_CENTER_HUD
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
       

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process[] procs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            if (procs.Length > 1)
            {
                Application.Current.Shutdown();
            }

            try
            {
                Environment.SetEnvironmentVariable("PYLON_GIGE_HEARTBEAT", "10000" /*ms*/);

                Pylon.Initialize();
                try
                {

                }
                catch
                {
                    Pylon.Terminate();
                    throw;
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Pylon.Terminate();
            }
            catch
            {

            }
        }
    }
}
