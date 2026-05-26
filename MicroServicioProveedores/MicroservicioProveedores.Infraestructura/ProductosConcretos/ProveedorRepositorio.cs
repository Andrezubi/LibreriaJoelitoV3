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
        private readonly IMongoCollection<Proveedor> _proveedores = RepositorioBD.Instancia.GetCollection<Proveedor>("proveedores");

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
            var filtro = Builders<Proveedor>.Filter.Eq(p => p.Id, t.Id);
            var actualizacion = Builders<Proveedor>.Update.Set(p => p.Estado, 0)
                                                           .Set(p => p.IdUsuario, t.IdUsuario);
            var resultado = await _proveedores.UpdateOneAsync(filtro, actualizacion);
            return resultado.IsAcknowledged && resultado.MatchedCount > 0;
        }

        public async Task<List<Proveedor>> ObtenerTodo()
        {
            return await _proveedores.Find(p => p.Estado == 1).ToListAsync();
        }

        public async Task<Proveedor> ObtenerPorId(string id)
        {
            return await _proveedores.Find(p => p.Id == id && p.Estado == 1).FirstOrDefaultAsync();
        }
    }
}
