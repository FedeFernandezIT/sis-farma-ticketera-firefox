using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lector.Sharp.Wpf.Models
{
    public class Turno
    {
        public string IdTurno { get; set; }
        public string Numero { get; set; }
        public string Fecha { get; set; }
        public string Letra { get; set; }
        public string Tipo { get; set; }
        public List<string> Textos { get; set; }

        public Turno()
        {
            Textos = new List<string>();
        }

        public static Turno GetMock()
        {
            return new Turno
            {
                IdTurno = "1",
                Fecha = "01/01/2017",
                Letra = "A",
                Numero = "001",
                Tipo = "Vacunas",
                Textos = new string[] { "text1", "text2", "text3" }.ToList()
            };
        }
    }
}
