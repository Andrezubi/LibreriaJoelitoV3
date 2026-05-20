using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroservicioProveedores.Infraestructura.Persistence;
using MicroServicioProveedores.Dominio.Interfaces;
using MicroServicioProveedores.Dominio.Modelos;
using MongoDB.Driver;

namespace MicroservicioProveedores.Infraestructura.ProductosConcretos
{
    public class ProveedorRepositorio : IRepositorio<Proveedor>
    {
        private readonly IMongoCollection<Proveedor> _proveedores = RepositorioBD.Instancia.GetCollection<Proveedor>("Proveedores");

        public async Task Insertar(Proveedor t)
        {
            await _proveedores.InsertOneAsync(t);
        }

        public async Task<bool> Actualizar(Proveedor t)
        {
            var resultado = await _proveedores.ReplaceOneAsync(p => p.Id == t.Id, t);
            return resultado.IsAcknowledged && resultado.ModifiedCount > 0;
        }

        public async Task<bool> Eliminar(Proveedor t)
        {
            var resultado = await _proveedores.DeleteOneAsync(p => p.Id == t.Id);
            return resultado.IsAcknowledged && resultado.DeletedCount > 0;
        }

        public async Task<List<Proveedor>> ObtenerTodo()
        {
            return await _proveedores.Find(_ => true).ToListAsync();
        }
    }
}
