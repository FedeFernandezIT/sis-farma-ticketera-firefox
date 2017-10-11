using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Lector.Sharp.Wpf.Models;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.IO;

namespace Lector.Sharp.Wpf.Services
{
    public class FarmaService
    {
        public string Url { get; set; }
        public string Mostrador { get; set; }
        public string UrlNavegarCustom { get; set; }
        public string UrlMensajes { get; set; }
        public string UrlNavegar { get; set; }
        public string DatabaseServer { get; set; }
        public string DatabaseCatalog { get; set; }

        /// <summary>
        /// Lee los archivos de configuración y setea las propiedades correspondientes
        /// </summary>
        public void LeerFicherosConfiguracion()
        {
            try
            {
                var pathUrlInformacionRemoto = ConfigurationManager.AppSettings["Url.Informacion.Remoto"];
                var pathUrlMensajesRemoto = ConfigurationManager.AppSettings["Url.Mensajes.Remoto"];
                var pathUrlCustom = ConfigurationManager.AppSettings["Url.Custom"];
                var pathMostradorVc = ConfigurationManager.AppSettings["Mostrador.Vc"];
                var pathDatabaseServer = ConfigurationManager.AppSettings["Database.Server"];
                var pathDatabseCatalog = ConfigurationManager.AppSettings["Database.Catalog"];

                Url = File.Exists(pathUrlInformacionRemoto)
                    ? new StreamReader(pathUrlInformacionRemoto).ReadLine()
                    : string.Empty;

                Mostrador = File.Exists(pathMostradorVc)
                    ? new StreamReader(pathMostradorVc).ReadLine()
                    : "1";

                UrlMensajes = File.Exists(pathUrlMensajesRemoto)
                    ? new StreamReader(pathUrlMensajesRemoto).ReadLine()
                    : string.Empty;

                UrlNavegarCustom = File.Exists(pathUrlCustom)
                    ? new StreamReader(pathUrlCustom).ReadLine()
                    : string.Empty;

                DatabaseServer = File.Exists(pathDatabaseServer)
                    ? new StreamReader(pathDatabaseServer).ReadLine()
                    : string.Empty;

                DatabaseCatalog = File.Exists(pathDatabseCatalog)
                    ? new StreamReader(pathDatabseCatalog).ReadLine()
                    : string.Empty;
            }
            catch (Exception)
            {
                LeerFicherosConfiguracion();
            }            
        }

        /// <summary>
        /// Obtiene los códigos de barras de los medicametos y formatea la salida de los mismos.
        /// </summary>
        /// <returns>códigos de barras formateados</returns>
        public string[] GetCodigoBarraMedicamentos()
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                return db.medicamentos.Where(med => med.cod_barras != null).Distinct()
                    .Select(med => med.cod_barras.Substring(0, 3)).ToArray();
            }
        }

        /// <summary>
        /// Obtiene los códigos de barras de los sinónimos y formatea la salida de los mismos.
        /// </summary>
        /// <returns>Códigos de barras formateados</returns>
        public string[] GetCodigoBarraSinonimos()
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                return db.sinonimos.Where(sin => sin.cod_barras != null).Distinct()
                    .Select(sin => sin.cod_barras.Substring(0, 3)).ToArray();
            }
        }

        /// <summary>
        /// Obtiene un cliente con una determinada tarjeta
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public clientes GetCliente(string tarjeta)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                if (db.clientes.Any(cli => cli.tarjeta == tarjeta))
                {
                    return db.clientes.First(cli => cli.tarjeta == tarjeta);
                }
                return null;
            }
        }

        /// <summary>
        /// Obtiene un trabajador con una determinada tarjeta
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public trabajador GetTrabajador(string tarjeta)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                if (db.trabajador.Any(tr => tr.tarjeta == tarjeta))
                {
                    return db.trabajador.First(tr => tr.tarjeta == tarjeta);
                }
                return null;
            }
        }

        /// <summary>
        /// Obtiene el código nacional de un sinónimo según un filtro aplicado a los códigos de barra
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>código nacioanal</returns>
        public long? GetCodigoNacionalSinonimo(string filter)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                if (db.sinonimos.Any(sin => sin.cod_barras.StartsWith(filter)))
                {
                    var strCodNacional = db.sinonimos.First(sin => sin.cod_barras.StartsWith(filter)).cod_nacional;
                    return Convert.ToInt64(strCodNacional);
                }
                return null;
            }            
        }

        /// <summary>
        /// Obtiene el código nacional de un medicamento según un filtro aplicado a los códigos de barra
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>código nacioanal</returns>
        public long? GetCodigoNacionalMedicamento(string filter)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                if (db.medicamentos.Any(med => med.cod_barras.StartsWith(filter)))
                {
                    return db.medicamentos.First(med => med.cod_barras.StartsWith(filter)).cod_nacional;
                }
                return null;
            }
        }

        /// <summary>
        /// Obtiene un asociado en la ventas por el código nacional.
        /// </summary>
        /// <param name="codNacional"></param>
        /// <returns>Asociado</returns>
        public asociados_cruzadas GetAsociado(long codNacional)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                try
                {
                    var query = (from v in db.ventas_cruzadas
                                 from a in db.asociados_cruzadas
                                 where v.id == (decimal)a.idVentaCruzada
                                 where v.eliminado == false && v.activo == true
                                 select a).ToList();

                    return query.Count != 0 ? query.First() : null;                   
                }
                catch (InvalidOperationException)
                {
                    return null;
                }                
            }
        }

        /// <summary>
        /// Obtiene un artículo de las ventas según el código nacional
        /// </summary>
        /// <param name="codNacional"></param>
        /// <returns></returns>
        public listas_articulos GetArticulo(long codNacional)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                try
                {
                    return db.ventas_cruzadas.Where(v => v.eliminado == false && v.activo == true && v.tipoAsociado == "Por Listas")
                    .Join(db.asociados_cruzadas,
                        v => v.id, a => (decimal)a.idVentaCruzada, (v, a) => new { Ventas = v, Asociado = a })
                    .Join(db.listas_articulos.Where(la => la.cod_articulo == codNacional), 
                        va => va.Asociado.asociado, la => la.cod_lista.ToString(), (va, la) => new { Articulo = la })
                    .Select(x => x.Articulo).Take(1).Single();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Obtiene la primera categorización
        /// </summary>
        /// <returns></returns>
        public categorizacion GetCategorizacion()
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                try
                {
                    return db.categorizacion.First();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Obtiene el código nacional de un asociado categorizado con ventas, según un código nacioanal
        /// </summary>
        /// <param name="codNacional"></param>
        /// <returns></returns>
        public long? GetAsociadoCategorizacion(long codNacional)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                var sql = "SELECT c.cod_nacional AS asociado " +
                            "FROM ventas_cruzadas v " +
                                "INNER JOIN asociados_cruzadas a ON a.idVentaCruzada = v.id " +
                                "INNER JOIN categorizacion c ON IF(INSTR(a.asociado,':') > 0, c.subfamilia = SUBSTRING_INDEX(a.asociado,':',-1), 1 = 1) AND c.familia = SUBSTRING_INDEX(a.asociado,':',1) " +
                                      "WHERE c.cod_nacional = @codNacional AND v.tipoAsociado = 'Por Familia/Subfamilia' " +
                                          "AND v.eliminado = 0 AND v.activo = 1 LIMIT 0,1";
                var query = db.Database.SqlQuery<long>(sql, new MySqlParameter("@codNacional", codNacional)).ToArray();
                return query.Length != 0 ? query[0] as long? : null;                                
            }
        }

        /// <summary>
        /// Obtiene el código nacional de un asociado con ventas de medicamentos, según un código nacional
        /// </summary>
        /// <param name="codNacional"></param>
        /// <returns>Código nacional</returns>
        public long? GetAnyAsociadoMedicamento(long codNacional)
        {
            using (var db = new SisFarmaEntities(DatabaseServer, DatabaseCatalog))
            {
                var sql = "SELECT m.cod_nacional AS asociado " +
                            "FROM ventas_cruzadas v " +
                                "INNER JOIN asociados_cruzadas a ON a.idVentaCruzada = v.id " +
                                "INNER JOIN medicamentos m ON m.familia = SUBSTRING_INDEX(a.asociado,':',-1) OR m.laboratorio = a.asociado " +
                                "WHERE m.cod_nacional = @codNacional AND (v.tipoAsociado = 'Por Familia/Subfamilia' OR v.tipoAsociado = 'Por Laboratorio') " +
                                        "AND v.eliminado = 0 AND v.activo = 1 LIMIT 0,1";
                var query = db.Database.SqlQuery<long>(sql, new MySqlParameter("@codNacional", codNacional)).ToArray();
                return query.Length != 0 ? query[0] as long? : null;
            }
        }
    
    }
}