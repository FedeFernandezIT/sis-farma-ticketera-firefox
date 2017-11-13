using Lector.Sharp.Wpf.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lector.Sharp.Wpf.Services
{    

    public class TicketService
    {        
        public string TicketTerminal { get; set; }
        public string TicketServer { get; set; }
        public string TicketDatabase { get; set; }

        public void InitializeConfiguration()
        {
            try
            {
                var dir = ConfigurationManager.AppSettings["Directory.Setup"];

                var pathTicketTerminal = Path.Combine(dir, ConfigurationManager.AppSettings["Ticket.Terminal"]);
                TicketTerminal = new StreamReader(pathTicketTerminal).ReadLine();

                var pathTicketServer = Path.Combine(dir, ConfigurationManager.AppSettings["Ticket.Server"]);
                TicketServer = new StreamReader(pathTicketServer).ReadLine();

                var pathTicketDatabase = Path.Combine(dir, ConfigurationManager.AppSettings["Ticket.Database"]);
                TicketDatabase = new StreamReader(pathTicketDatabase).ReadLine();
            }
            catch (IOException ex)
            {
                throw new IOException("Error al leer archivos de configuración");
            }
        }

        public void SetTicketsPrinted()
        {
            try
            {
                using (var db = new TicketContext(TicketServer, TicketDatabase))
                {
                    var sql = @"update turnos set ticketImpreso ='1' WHERE 1";
                    db.Database.ExecuteSqlCommand(sql);
                }
            }
            catch (Exception)
            {
                Task.Delay(500).Wait();
            }            
        }

        public void Print()
        {
            try
            {
                using (var db = new TicketContext(TicketServer, TicketDatabase))
                {
                    var turno = GetNonPrintedTurn(db);
                    if (turno != null)
                    {
                        UpdateTurnToPrinted(db, turno.IdTurno);
                        turno.Textos = GetTextsVisbleFromTickets(db);
                        var printer = new PrintDocument();
                        printer.PrintPage += (sender, e) => PrintPage(sender, e, turno);
                        printer.Print();

                    }
                }
                //var turno = Turno.GetMock();
                //var printer = new PrintDocument();
                //printer.PrintPage += (sender, e) => PrintPage(sender, e, turno);
                //printer.Print();
            }
            catch (Exception)
            {
                Task.Delay(500).Wait();
            }            
        }

        private void PrintPage(object sender, PrintPageEventArgs e, Turno turno)
        {                        
            var currentY = Convert.ToSingle(e.MarginBounds.Top);

            var text = "*****************************************************";
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold), StringAlignment.Center, ref currentY);            

            text = turno.IdTurno;
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 20f, FontStyle.Bold), StringAlignment.Center, ref currentY);            

            text = "SU NÚMERO ES";
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 15f, FontStyle.Bold), StringAlignment.Center, ref currentY);            

            text = turno.Letra + turno.Numero;
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 50f, FontStyle.Bold), StringAlignment.Center, ref currentY);

            text = "*****************************************************";
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold), StringAlignment.Center, ref currentY);

            text = "Por favor espere su turno";
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold), StringAlignment.Center, ref currentY);
            
            text = "Fecha: " + turno.Fecha;
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold), StringAlignment.Center, ref currentY);
            
            text = "_____________________________________________________";
            PrintLine(text, e, new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold), StringAlignment.Center, ref currentY);            

            foreach (var item in turno.Textos)
            {
                PrintLine(item, e, new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold), StringAlignment.Center, ref currentY);                
            }
        }

        private void PrintLine(string text, PrintPageEventArgs e, Font font, StringAlignment alignment, ref float currentY)
        {            
            using (var sf = new StringFormat())
            {
                sf.Alignment = alignment;
                //var family = font.FontFamily;
                //var linespacing = family.GetLineSpacing(font.Style);
                //var ascent = family.GetCellAscent(font.Style);
                //var baseline = font.GetHeight(e.Graphics) * ascent / linespacing;
                var size = e.Graphics.MeasureString(text, font, e.MarginBounds.Size);
                e.Graphics.DrawString(text, font, Brushes.Black, new PointF(e.MarginBounds.Left + e.MarginBounds.Width / 2, currentY), sf);
                //currentY += size.Height - baseline;
                currentY += font.GetHeight(e.Graphics);
            }
        }

        private void PrintBlock(string text, PrintPageEventArgs e, Font font, ref float currentY)
        {                        
            var size = e.Graphics.MeasureString(text, font, e.MarginBounds.Size);
            e.Graphics.DrawString(text, font, Brushes.Black, new RectangleF(e.MarginBounds.Left, currentY, size.Width, size.Height));
            currentY += size.Height;            
        }

        private void UpdateTurnToPrinted(TicketContext db, string turno)
        {
            var sql = @"update turnos set ticketImpreso ='1' WHERE idturno = @turno";
            db.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("turno", turno));
        }

        private Turno GetNonPrintedTurn(TicketContext db)
        {
            var sql = @"SELECT t.idturno AS IdTurno, t.numero AS Numero, t.fechaTurno AS Fecha, c.letra AS Letra, c.nombre AS Tipo From turnos t INNER JOIN colas c ON c.idcola = t.idcola WHERE  t.idterminal = @terminal AND t.ticketImpreso = '0' Order by t.idturno DESC";
            var result = db.Database.SqlQuery<Turno>(sql,
                new MySqlParameter("terminal", TicketTerminal))
                .ToList();
            return result.Count > 0 ? result[0] : null;
        }

        private List<string> GetTextsVisbleFromTickets(TicketContext db)
        {
            var sql = @"SELECT texto From ticket WHERE visible = '1'  Order by orden ASC";
            return db.Database.SqlQuery<string>(sql).ToList();
        }
    }
}
