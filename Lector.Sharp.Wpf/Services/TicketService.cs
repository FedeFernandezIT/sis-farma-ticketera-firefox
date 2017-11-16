using Lector.Sharp.Wpf.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.PowerPacks.Printing.Compatibility.VB6;
using Microsoft.PointOfService;

namespace Lector.Sharp.Wpf.Services
{    

    public class TicketService
    {
        //private PrintDocument _printer;

        private static TraceSource _logger = new TraceSource("TestLog");
        private static int _eventNumber = 0;

        public string TicketTerminal { get; set; }
        public string TicketServer { get; set; }
        public string TicketDatabase { get; set; }

        public TicketService()
        {
            //_printer = new PrintDocument();
        }

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
            var init = DateTime.Now;
            var number = _eventNumber++;
            _logger.TraceInformation($"#{number} Init event Print() - {init}");
            try
            {             
                _logger.TraceInformation($"#{number} Before GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                var turno = GetNonPrintedTurn();
                _logger.TraceInformation($"#{number} After GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                if (turno != null)
                {             
                    _logger.TraceInformation($"#{number} Init PrintDocument() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                    var printer = new PrintDocument();
                    printer.PrintPage += (sender, e) => PrintPage(sender, e, turno);
                    _logger.TraceInformation($"#{number} Before PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                    printer.Print();
                    _logger.TraceInformation($"#{number} After PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                }                
            }
            catch (Exception)
            {
                Task.Delay(500).Wait();
            }
            _logger.TraceInformation($"#{number} Finish event Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
        }

        public void PrintInside()
        {
            var init = DateTime.Now;
            var number = _eventNumber++;
            _logger.TraceInformation($"#{number} Init event Print() - {init}");
            try
            {                
                _logger.TraceInformation($"#{number} Before GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                using (var db = new TicketContext(TicketServer, TicketDatabase))
                {
                    using (var trans = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            var sql = @"SELECT t.idturno AS IdTurno, t.numero AS Numero, t.fechaTurno AS Fecha, c.letra AS Letra, c.nombre AS Tipo From turnos t INNER JOIN colas c ON c.idcola = t.idcola WHERE  t.idterminal = @terminal AND t.ticketImpreso = '0' Order by t.idturno DESC";
                            var result = db.Database.SqlQuery<Turno>(sql,
                                new MySqlParameter("terminal", TicketTerminal))
                                .ToList();
                            if (result.Count > 0)
                            {
                                var turno = result[0].IdTurno;
                                sql = @"update turnos set ticketImpreso ='1' WHERE idturno = @turno";
                                db.Database.ExecuteSqlCommand(sql,
                                    new MySqlParameter("turno", turno));
                                sql = @"SELECT texto From ticket WHERE visible = '1'  Order by orden ASC";
                                result[0].Textos = db.Database.SqlQuery<string>(sql).ToList();
                                trans.Commit();

                                _logger.TraceInformation($"#{number} Init PrintDocument() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{result[0].IdTurno}");
                                var printer = new PrintDocument();
                                printer.PrintPage += (sender, e) => PrintPage(sender, e, result[0]);
                                _logger.TraceInformation($"#{number} Before PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{result[0].IdTurno}");
                                printer.Print();
                                _logger.TraceInformation($"#{number} After PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{result[0].IdTurno}");
                            }
                            trans.Rollback();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                        }

                    }
                }
                _logger.TraceInformation($"#{number} After GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
            }
            catch (Exception)
            {
                Task.Delay(500).Wait();
            }
            _logger.TraceInformation($"#{number} Finish event Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
        }

        public void PrintWithVb6()
        {
            var init = DateTime.Now;
            var number = _eventNumber++;
            _logger.TraceInformation($"#{number} Init event Print() - {init}");
            try
            {
                _logger.TraceInformation($"#{number} Before GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                var turno = GetNonPrintedTurn();
                _logger.TraceInformation($"#{number} After GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                if (turno != null)
                {
                    _logger.TraceInformation($"#{number} Init PrintDocument() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                    var printer = new Printer();
                    _logger.TraceInformation($"#{number} Before PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                    printer.FontBold = true;
                    printer.FontSize = 15;
                    var text = "*****************************************************";
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    printer.FontSize = 20;
                    text = turno.IdTurno;
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    printer.FontSize = 15;
                    text = "SU NÚMERO ES";
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    printer.FontSize = 50;
                    text = turno.Letra + turno.Numero;
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    printer.FontSize = 15;
                    text = "*****************************************************";
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    printer.FontSize = 9;
                    text = "Por favor espere su turno";
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    text = "Fecha: " + turno.Fecha;
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    printer.FontSize = 10;
                    text = "_____________________________________________________";
                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                    printer.Print(text);

                    foreach (var item in turno.Textos)
                    {
                        printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(item) / 2;
                        printer.Print(item);
                    }

                    printer.EndDoc();                    
                    _logger.TraceInformation($"#{number} After PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                }
            }
            catch (Exception)
            {
                Task.Delay(500).Wait();
            }
            _logger.TraceInformation($"#{number} Finish event Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
        }

        public void PrintInsideWithVb6()
        {
            var init = DateTime.Now;
            var number = _eventNumber++;
            _logger.TraceInformation($"#{number} Init event Print() - {init}");
            try
            {
                _logger.TraceInformation($"#{number} Before GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                using (var db = new TicketContext(TicketServer, TicketDatabase))
                {
                    using (var trans = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            var sql = @"SELECT t.idturno AS IdTurno, t.numero AS Numero, t.fechaTurno AS Fecha, c.letra AS Letra, c.nombre AS Tipo From turnos t INNER JOIN colas c ON c.idcola = t.idcola WHERE  t.idterminal = @terminal AND t.ticketImpreso = '0' Order by t.idturno DESC";
                            var result = db.Database.SqlQuery<Turno>(sql,
                                new MySqlParameter("terminal", TicketTerminal))
                                .ToList();
                            if (result.Count > 0)
                            {
                                var turno = result[0];                                
                                sql = @"update turnos set ticketImpreso ='1' WHERE idturno = @turno";
                                db.Database.ExecuteSqlCommand(sql,
                                    new MySqlParameter("turno", turno.IdTurno));
                                sql = @"SELECT texto From ticket WHERE visible = '1'  Order by orden ASC";
                                result[0].Textos = db.Database.SqlQuery<string>(sql).ToList();
                                trans.Commit();
                                
                                _logger.TraceInformation($"#{number} Init PrintDocument() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                                var printer = new Printer();
                                _logger.TraceInformation($"#{number} Before PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                                printer.FontBold = true;
                                printer.FontSize = 15;
                                var text = "*****************************************************";
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                printer.FontSize = 20;
                                text = turno.IdTurno;
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                printer.FontSize = 15;
                                text = "SU NÚMERO ES";
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                printer.FontSize = 50;
                                text = turno.Letra + turno.Numero;
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                printer.FontSize = 15;
                                text = "*****************************************************";
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                printer.FontSize = 9;
                                text = "Por favor espere su turno";
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                text = "Fecha: " + turno.Fecha;
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                printer.FontSize = 10;
                                text = "_____________________________________________________";
                                printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(text) / 2;
                                printer.Print(text);

                                foreach (var item in turno.Textos)
                                {
                                    printer.CurrentX = printer.ScaleWidth / 2 - printer.TextWidth(item) / 2;
                                    printer.Print(item);
                                }

                                printer.EndDoc();
                                _logger.TraceInformation($"#{number} After PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                            }
                            trans.Rollback();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                        }

                    }
                }
                _logger.TraceInformation($"#{number} After GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
            }
            catch (Exception)
            {
                Task.Delay(500).Wait();
            }
            _logger.TraceInformation($"#{number} Finish event Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
        }

        public void PrintPos()
        {
            var init = DateTime.Now;
            var number = _eventNumber++;
            _logger.TraceInformation($"#{number} Init event Print() - {init}");
            
            try
            {
                _logger.TraceInformation($"#{number} Before GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                var turno = GetNonPrintedTurn();
                _logger.TraceInformation($"#{number} After GetNonPrintedTurn() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
                if (turno != null)
                {
                    PosExplorer posExplorer = new PosExplorer();
                    PosCommon posCommon = null;
                    DeviceInfo device = null;

                    DeviceCollection devices = posExplorer.GetDevices(DeviceType.PosPrinter);
                    if (devices.Count > 0)
                    {
                        _logger.TraceInformation($"#{number} Init PrintDocument() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                        device = devices[0];
                        posCommon = (PosCommon)posExplorer.CreateInstance(device);

                        posCommon.Open();
                        posCommon.Claim(0);
                        posCommon.DeviceEnabled = true;
                        _logger.TraceInformation($"#{number} Before PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                        PosPrinter posPrinter = posCommon as PosPrinter;
                        posPrinter.AsyncMode = true;
                        var text = "*****************************************************";
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);

                        text = turno.IdTurno;
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);
                        
                        text = "SU NÚMERO ES";
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);
                        
                        text = turno.Letra + turno.Numero;
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);
                        
                        text = "*****************************************************";
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);
                        
                        text = "Por favor espere su turno";
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);

                        text = "Fecha: " + turno.Fecha;
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);
                        
                        text = "_____________________________________________________";
                        posPrinter.PrintImmediate(PrinterStation.Receipt, text);

                        foreach (var item in turno.Textos)
                        {
                            posPrinter.PrintImmediate(PrinterStation.Receipt, item);
                        }
                        posPrinter.CutPaper(PosPrinter.PrinterCutPaperFullCut);
                        posCommon.Close();
                        _logger.TraceInformation($"#{number} After PrintDocument.Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms - Turno#{turno.IdTurno}");
                    }                                        
                }
            }
            catch (Exception e)
            {
                Task.Delay(500).Wait();
                _logger.TraceInformation($"#{number} Exception Print(): {e.Message} - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");
            }
            _logger.TraceInformation($"#{number} Finish event Print() - {init} - {(DateTime.Now - init).TotalMilliseconds} ms");            
        }

        private void PrintPage(object sender, PrintPageEventArgs e, Turno turno)
        {                        
            var currentY = 0f;

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

            e.HasMorePages = false;
        }

        private void PrintLine(string text, PrintPageEventArgs e, Font font, StringAlignment alignment, ref float currentY)
        {            
            using (var sf = new StringFormat())
            {
                sf.Alignment = alignment;                
                var size = e.Graphics.MeasureString(text, font, e.MarginBounds.Size);
                e.Graphics.DrawString(text, font, Brushes.Red, new PointF(e.MarginBounds.Left + e.MarginBounds.Width / 2, currentY), sf);                
                currentY += font.GetHeight(e.Graphics);
            }
        }

        private Turno GetNonPrintedTurn()
        {
            using (var db = new TicketContext(TicketServer, TicketDatabase))
            {
                using (var trans = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = @"SELECT t.idturno AS IdTurno, t.numero AS Numero, t.fechaTurno AS Fecha, c.letra AS Letra, c.nombre AS Tipo From turnos t INNER JOIN colas c ON c.idcola = t.idcola WHERE  t.idterminal = @terminal AND t.ticketImpreso = '0' Order by t.idturno DESC";
                        var result = db.Database.SqlQuery<Turno>(sql,
                            new MySqlParameter("terminal", TicketTerminal))
                            .ToList();
                        if (result.Count > 0)
                        {
                            var turno = result[0].IdTurno;
                            sql = @"update turnos set ticketImpreso ='1' WHERE idturno = @turno";
                            db.Database.ExecuteSqlCommand(sql,
                                new MySqlParameter("turno", turno));
                            sql = @"SELECT texto From ticket WHERE visible = '1'  Order by orden ASC";
                            result[0].Textos = db.Database.SqlQuery<string>(sql).ToList();
                            trans.Commit();
                            return result[0];
                        }
                        trans.Rollback();
                        return null;
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        return null;
                    }

                }
            }
            
        }
        
    }
}
