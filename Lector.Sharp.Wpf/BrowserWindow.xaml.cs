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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
}
