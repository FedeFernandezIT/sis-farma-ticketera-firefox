using Lector.Sharp.Wpf.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lector.Sharp.Wpf
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LowLevelKeyboardListener _listener;
        private FarmaService _service;
        private string _keyData = string.Empty;
        private BrowserWindow _infoBrowser;

        public BrowserWindow InfoBrowser
        {
            get
            {
                if (_infoBrowser.IsClosed)
                {
                    _infoBrowser = new BrowserWindow();
                }
                return _infoBrowser;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _service = new FarmaService();
            _listener = new LowLevelKeyboardListener();
            _infoBrowser = new BrowserWindow();

            _service.LeerFicherosConfiguracion();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }

        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed != Key.Enter)
            {
                if (_keyData.Length > 50)
                    _keyData = _keyData.Substring(_keyData.Length - 20 - 1);
                if (e.KeyPressed >= Key.D0 && e.KeyPressed <= Key.D9)
                {
                    var kc = new KeyConverter();
                    _keyData += (char)KeyInterop.VirtualKeyFromKey(e.KeyPressed);
                }
            }
            else

            {
                long number = 0;
                if (long.TryParse(_keyData, out number) && _keyData.Length >= 4)
                {
                    var lanzarBrowserWindow = false;
                    var noEntrar = string.Empty;
                    var continuar = false;
                    var continuarCodNacional = false;
                    var keyDataAux = _keyData;

                    if (_keyData.Length >= 13)
                    {
                        _keyData = _keyData.Substring(_keyData.Length - 13);
                        noEntrar = _keyData.Substring(0, 4);

                        if (Array.Exists(new[] { "1010", "1111", "0000", "9902", "9900", "9901", "9903", "9904", "9905", "9906", "9907", "9908", "9909", "9910", "9911", "9912", "9913", "9915", "9916", "9917", "9918", "9919", "9920", "1001", "2014", "2015", "2016", "2017", "2018" }, x => x.Equals(noEntrar)))
                            continuar = true;
                        else
                        {
                            noEntrar = _keyData.Substring(0, 7);
                            if (noEntrar.Equals("8470000"))
                                continuar = true;
                            else
                            {
                                noEntrar = _keyData.Substring(0, 3);
                                if (Array.Exists(_service.GetCodigoBarraMedicamentos(), x => x.Equals(noEntrar)))
                                    continuarCodNacional = true;
                                else if (Array.Exists(_service.GetCodigoBarraSinonimos(), x => x.Equals(noEntrar)))
                                    continuarCodNacional = true;
                            }
                        }
                    }

                    if (_keyData.Length >= 12 && !continuar && !continuarCodNacional)
                    {
                        _keyData = keyDataAux.Substring(_keyData.Length - 12);
                        noEntrar = _keyData.Substring(0, 4);
                        if ("1111".Equals(noEntrar) || "0000".Equals(noEntrar))
                            continuar = true;
                    }

                    if (_keyData.Length >= 10 && !continuar && !continuarCodNacional)
                    {
                        _keyData = keyDataAux.Substring(_keyData.Length - 10);
                        noEntrar = _keyData.Substring(0, 4);
                        if ("1111".Equals(noEntrar) || "1930".Equals(noEntrar))
                            continuar = true;
                    }

                    if (_keyData.Length >= 7 && !continuar && !continuarCodNacional)
                    {
                        _keyData = keyDataAux.Substring(_keyData.Length - 7);
                        noEntrar = _keyData.Substring(0, 4);
                        if (Array.Exists(new[] { "1000", "1001", "1002", "1003" }, x => x.Equals(noEntrar)))
                            continuar = true;
                    }

                    if (_keyData.Length >= 6 && !continuar && !continuarCodNacional)
                    {
                        _keyData = keyDataAux.Substring(_keyData.Length - 6);
                        noEntrar = _keyData.Substring(0, 3);
                        if (Array.Exists(new[] { "000", "001", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011", "100", "101", "102", "103" }, x => x.Equals(noEntrar)))
                            continuar = true;
                    }

                    if (_keyData.Length >= 5 && !continuar && !continuarCodNacional)
                    {
                        _keyData = keyDataAux.Substring(_keyData.Length - 5);
                        noEntrar = _keyData.Substring(0, 2);
                        if (Array.Exists(new[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" }, x => x.Equals(noEntrar)))
                            continuar = true;
                    }

                    if (_keyData.Length >= 4 && !continuar && !continuarCodNacional)
                    {
                        _keyData = keyDataAux.Substring(_keyData.Length - 4);
                        noEntrar = _keyData.Substring(0, 2);
                        if (Array.Exists(new[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" }, x => x.Equals(noEntrar)))
                            continuar = true;
                    }

                    if (continuar)
                    {
                        var cliente = _service.GetCliente(_keyData);
                        if (cliente != null)
                        {
                            SendKey(Key.Enter);
                            lanzarBrowserWindow = true;
                            _service.UrlNavegar = _service.Url.Replace("codigo", cliente.cod.ToString()) + "/" + _service.Mostrador;
                        }
                        else
                        {
                            var trabajador = _service.GetTrabajador(_keyData);
                            if (trabajador != null)
                            {
                                SendKey(Key.Enter);
                                lanzarBrowserWindow = true;
                                _service.UrlNavegar = _service.UrlMensajes.Replace("codigo", trabajador.id.ToString());
                            }
                        }
                    }
                    else
                    {
                        var codNacional = _service.GetCodigoNacionalSinonimo(_keyData.Substring(0, _keyData.Length > 12 ? 12 : _keyData.Length));
                        if (codNacional == null)
                        {
                            codNacional = _service.GetCodigoNacionalMedicamento(_keyData.Substring(0, _keyData.Length > 12 ? 12 : _keyData.Length));
                            if (codNacional == null)
                                codNacional = Convert.ToInt64(_keyData.Substring(3, _keyData.Length - 4));
                        }

                        var asociado = _service.GetAsociado(Convert.ToInt64(codNacional));
                        var mostrarVentana = false;
                        if (asociado != null)
                            mostrarVentana = true;
                        else
                        {
                            var articulo = _service.GetArticulo(Convert.ToInt64(codNacional));
                            if (articulo != null)
                                mostrarVentana = true;
                            else
                            {
                                var categ = _service.GetCategorizacion();
                                if (categ != null)
                                    mostrarVentana = true;
                            }
                        }

                        if (mostrarVentana)
                        {
                            var asociadoCodNacional = _service.GetAsociadoCategorizacion(Convert.ToInt64(codNacional)) ??
                                    _service.GetAnyAsociadoMedicamento(Convert.ToInt64(codNacional));
                            if (asociadoCodNacional != null)
                            {
                                SendKey(Key.Enter);
                                lanzarBrowserWindow = true;
                                _service.UrlNavegar = _service.Url.Replace("codigo", "cn" + asociadoCodNacional.ToString() + "/" + _service.Mostrador);
                            }
                        }
                    }

                    if (lanzarBrowserWindow)
                    {
                        var viewer = InfoBrowser;
                        viewer.browser.Navigate(_service.UrlNavegar);
                        viewer.Show();
                    }
                }

                _keyData = "";
            }
        }
        public static void SendKey(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(e);                    
                }
            }
        }
    }
}
