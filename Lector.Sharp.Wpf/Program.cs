using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lector.Sharp.Wpf
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
                App.Main();                
                instanceMutex.ReleaseMutex();                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        
    }
}
