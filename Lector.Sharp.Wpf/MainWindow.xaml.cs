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
        /// <summary>
        /// Listener que escucha cada vez que se presiona una tecla.       
        /// </summary>
        private LowLevelKeyboardListener _listener;

        /// <summary>
        /// Gestiona todos los servicios de SisFarma, como acceso a la
        /// base de datos, lectura de archivos de configuración.
        /// </summary>
        private FarmaService _service;

        /// <summary>
        /// Almacena el valor de las teclas presionadas, específcamente números.
        /// </summary>
        private string _keyData = string.Empty;

        /// <summary>
        /// Window para mostrar información de la base de datos.
        /// Está ventana emerge después de presionar ENTER y las teclas presionadas
        /// previamente forman un código existente en la base de datos.
        /// </summary>
        private BrowserWindow _infoBrowser;

        /// <summary>
        /// Window para mostrar una dirección web. Esta ventana emerge cuando se
        /// presiona SHIFT+F1 y se cierrra al presionar SHIFT+F2
        /// </summary>
        private BrowserWindow _customBrowser;

        /// <summary>
        /// Icono de barra de tareas, gestiona la salida del programa
        /// </summary>
        private System.Windows.Forms.NotifyIcon _iconNotification;

        /// <summary>
        /// Devuelve una ventana para mostrar info de la base de datos, si esta ya ce cerró devuelve una nueva
        /// </summary>
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

        /// <summary>
        /// Devuelve una ventana para mostrar una web específica, si esta ya ce cerró devuelve una nueva
        /// </summary>
        public BrowserWindow CustomBrowser
        {
            get
            {
                if (_customBrowser.IsClosed)
                {
                    _customBrowser = new BrowserWindow();
                }
                return _customBrowser;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _service = new FarmaService();
            _listener = new LowLevelKeyboardListener();
            _infoBrowser = new BrowserWindow();
            _customBrowser = new BrowserWindow();

            // Leemos los archivos de configuración
            _service.LeerFicherosConfiguracion();

            // Setamos el comportamiento de la aplicación al presionar una tecla
            _listener.OnKeyPressed += _listener_OnKeyPressed;

            // Activamos el listener de teclado
            _listener.HookKeyboard();

            _iconNotification = new System.Windows.Forms.NotifyIcon();
            _iconNotification.BalloonTipText = "La Aplicación SisFarma se encuentra ejecutando";
            _iconNotification.BalloonTipTitle = "SisFarma Notificación";
            _iconNotification.Text = "Presione Click para Mostrar";
            _iconNotification.Icon = new System.Drawing.Icon(@"./Icons/sisfarma-logo.ico");
            _iconNotification.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;

            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();            
            System.Windows.Forms.MenuItem notificactionInfoMenu = new System.Windows.Forms.MenuItem("Info");
            notificactionInfoMenu.Click += notificactionInfoMenu_Click;
            System.Windows.Forms.MenuItem notificationQuitMenu = new System.Windows.Forms.MenuItem("Salir");
            notificationQuitMenu.Click += notificationQuitMenu_Click;

            menu.MenuItems.Add(notificactionInfoMenu);
            menu.MenuItems.Add(notificationQuitMenu);            
            _iconNotification.ContextMenu = menu;
            _iconNotification.Visible = true;
        }

        /// <summary>
        /// Acción para salir del programa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notificationQuitMenu_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void notificactionInfoMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("SisFarma Application \nsisfarma.es");
        }    
       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Desactivamos el listener del teclado
            _listener.UnHookKeyboard();
        }

        /// <summary>
        /// Precesa el comporatiento de la aplicación al presionarse una tecla.
        /// </summary>
        /// <param name="sender">Listener del teclado</param>
        /// <param name="e">Información de la tecla presionada</param>
        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            try
            {
                if (e.KeyPressed != Key.Enter)
                {
                    // Si presionamos SHIFT + F1
                    if (_listener.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_SHIFT) && e.KeyPressed == Key.F1)
                        // Abrimos una ventana con la web personalizada.
                        OpenWindowBrowser(CustomBrowser);                    
                    // Si presionamos SHIFT + F2
                    else if (_listener.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_SHIFT) && e.KeyPressed == Key.F2)
                    {
                        // Cerramos la ventana con la web personalizada
                        CloseWindowBrowser(CustomBrowser);
                    }
                    // Si la tecla presioanada es numérica
                    else if (!_listener.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_SHIFT) &&
                        (e.KeyPressed >= Key.D0 && e.KeyPressed <= Key.D9 || e.KeyPressed >= Key.NumPad0 && e.KeyPressed <= Key.NumPad9))
                    {
                        // Almacenamos el valor de la tecla.
                        StoreKey(e.KeyPressed);
                    }
                }
                else
                {                    
                    ProccessEnterKey();
                }
            }
            catch (Exception)
            {
                // Sólo seguimos capturando las entadas por teclado.
                // No lanzamos la exception
            }
            
        }

        /// <summary>
        /// Procesa los _keyData para buscar datos en la base de datos
        /// </summary>
        private void ProccessEnterKey()
        {
            long number = 0;
            // Si el valor almacenado en _keyData en numérico y con longitud superior a 4
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

            // Limpieamos los datos de las teclas almacenados en _keyData
            _keyData = "";
        }

        /// <summary>
        /// Almacena el valor de la tacla en _keyData
        /// </summary>
        /// <param name="key">Tecla presionada</param>
        private void StoreKey(Key key)
        {
            if (_keyData.Length > 50)
                _keyData = _keyData.Substring(_keyData.Length - 20 - 1);
            var kc = new KeyConverter();
            // Key.NumPad# se convierte en 'NumPad#' por lo cual lo eliminamos
            _keyData += kc.ConvertToString(key)?.Replace("NumPad", string.Empty);
        }

        /// <summary>
        /// Cierra una ventana que contiene un browser
        /// </summary>
        /// <param name="browser"></param>
        private void CloseWindowBrowser(BrowserWindow browser)
        {            
            SystemCommands.CloseWindow(browser);
        }

        /// <summary>
        /// Abre una ventana que contiene un browser
        /// </summary>
        /// <param name="browser">Ventana con un browser</param>
        private void OpenWindowBrowser(BrowserWindow browser)
        {            
            browser.browser.Navigate(_service.UrlNavegarCustom);
            browser.Show();
        }

        /// <summary>
        /// Simula presionar una tecla
        /// </summary>
        /// <param name="key"></param>
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
