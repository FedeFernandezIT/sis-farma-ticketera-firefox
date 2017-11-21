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
                SupportHtml5();
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

        private void SupportHtml5()
        {
            var fileExcecutable = Path.GetFileName(Assembly.GetEntryAssembly().Location);
            RegistryKey reg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            reg.SetValue(fileExcecutable, 11001, RegistryValueKind.DWord);
        }

        private void RemoveSupportHtml5()
        {
            var fileExcecutable = Path.GetFileName(Assembly.GetEntryAssembly().Location);
            RegistryKey reg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            reg.DeleteValue(fileExcecutable);
        }

        protected override void OnExit(ExitEventArgs e)
        {                     
            RemoveSupportHtml5();         
            base.OnExit(e);
        }        
    }
}
