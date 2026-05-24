using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Dominio.Modelos
{
    public class Proveedor
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Nombre { get; set; }
        public int Nit { get; set; }
        public int TelefonoContacto { get; set; }
        public string? Descripcion { get; set; }
        public string Direccion { get; set; }
        public int IdUsuario { get; set; }
        public int Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }


        public Proveedor() { }

        public Proveedor(string nombre, int nit, int telefonoContacto, string? descripcion, string direccion, int idUsuario)
        {
            Nombre = nombre;
            Nit = nit;
            TelefonoContacto = telefonoContacto;
            Descripcion = descripcion;
            Direccion = direccion;
            IdUsuario = idUsuario;
        }
    }
}
