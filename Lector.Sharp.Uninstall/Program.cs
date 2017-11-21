using Lector.Sharp.ClickOnce;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lector.Sharp.Uninstall
{    
    public class Program
    {
        private static Mutex instanceMutex;

        [STAThread]
        static void Main(string[] args)
        {            
            try
            {
                bool createdNew;
                instanceMutex = new Mutex(true, @"Local\" + Assembly.GetExecutingAssembly().GetType().GUID, out createdNew);
                if (!createdNew)
                {
                    instanceMutex = null;
                    return;
                }
                if (MessageBoxResult.Yes == MessageBox.Show($"¿Desea desinstalar {Globals.PublisherName} - {Globals.ProductName}", "Desinstalar", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    var clickOnce = new ClickOnceHelper(Globals.PublisherName, Globals.ProductName);
                    if (clickOnce.Uninstall())
                    {
                        clickOnce.RemoveStartup();
                        var publisherFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.PublisherName);
                        if (Directory.Exists(publisherFolder))
                            Directory.Delete(publisherFolder, true);
                    }                                        
                }
                ReleaseMutex();                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }                        
        }

        private static void ReleaseMutex()
        {
            if (instanceMutex == null)
                return;
            instanceMutex.ReleaseMutex();
            instanceMutex.Close();
            instanceMutex = null;
        }
    }
}
