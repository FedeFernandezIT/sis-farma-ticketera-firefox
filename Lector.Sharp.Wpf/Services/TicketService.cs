using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lector.Sharp.Wpf.Services
{    

    public class TicketService
    {
        public string TicketWindowUrl { get; set; }
        public string TicketTerminal { get; set; }
        public string TicketServer { get; set; }
        public string TicketDatabase { get; set; }

        public void InitializeConfiguration()
        {
            try
            {
                var dir = ConfigurationManager.AppSettings["Sisfarma.Path.Base"];
                var pathTicketWindowUrl = Path.Combine(dir, ConfigurationManager.AppSettings["Ticket.Window.Url"]);
                var pathTicketTerminal = Path.Combine(dir, ConfigurationManager.AppSettings["Ticket.Terminal"]);
                var pathTicketServer = Path.Combine(dir, ConfigurationManager.AppSettings["Ticket.Server"]);
                var pathTicketDatabase = Path.Combine(dir, ConfigurationManager.AppSettings["Ticket.Database"]);

                TicketWindowUrl = new StreamReader(pathTicketWindowUrl).ReadLine();
                TicketTerminal = new StreamReader(pathTicketTerminal).ReadLine();
                TicketServer = new StreamReader(pathTicketServer).ReadLine();
                TicketDatabase = new StreamReader(pathTicketDatabase).ReadLine();                                
            }
            catch (IOException ex)
            {
                throw new IOException("Error al leer archivos de configuración");
            }
        }         

        public void Print()
        {

        }
    }
}
