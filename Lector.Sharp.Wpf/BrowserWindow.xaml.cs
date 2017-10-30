using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Lector.Sharp.Wpf.Extensions;

namespace Lector.Sharp.Wpf
{
    /// <summary>
    /// Lógica de interacción para BrowserWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window
    {
        private LowLevelWindowsListener _windows;
        private bool _closed;

        public bool IsClosed
        {
            get { return _closed; }
        }

        public BrowserWindow()
        {
            _windows = new LowLevelWindowsListener();
            _closed = false;
            InitializeComponent();            
        }

        private void winBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            ColocarEnTop(true);
            Browser.SuppressScriptErrors(true);
        }

        private void winBrowser_Unloaded(object sender, RoutedEventArgs e)
        {
            ColocarEnTop(false);
        }

        private void ColocarEnTop(bool top)
        {
            _windows.SetWindowPos(this, top ? LowLevelWindowsListener.HWND.TopMost : LowLevelWindowsListener.HWND.NoTopMost, 0, 0, 0, 0, LowLevelWindowsListener.SetWindowPosFlags.SWP_NOMOVE | LowLevelWindowsListener.SetWindowPosFlags.SWP_NOSIZE);
        }

        private void winBrowser_Closed(object sender, EventArgs e)
        {
            _closed = true;
        }
        

        private void WinBrowser_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        const int SC_RESTORE = 0xF120;

        /// <summary>
        /// Previene ciertos commandos sobre la ventana
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        // Previene que el usuario mueva la ventana
                        handled = true;
                    }
                    else if (command == SC_RESTORE)
                    {
                        // Previene restaurar la ventana a su tamaño original
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
