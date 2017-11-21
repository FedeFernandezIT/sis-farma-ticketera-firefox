using Lector.Sharp.ClickOnce;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lector.Sharp.Wpf
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {        
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {                
                var clickOnce = new ClickOnceHelper(Globals.PublisherName, Globals.ProductName);                
                clickOnce.UpdateUninstallParameters();
                RegisterStartup(clickOnce.ProductName);
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }            
        }

        public void RegisterStartup(string productName)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
                return;
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);            
            reg.SetValue(productName, Assembly.GetExecutingAssembly().Location);
        }
                        
    }
}
