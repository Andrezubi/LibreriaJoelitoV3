
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Persistencia;
using System.Data;

namespace MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos
{
    public class PresentacionRepositorio: RepositorioBD, IRepositorio<Presentacion>
    {
       
        public int Insertar(Presentacion t)
        {
            throw new NotImplementedException();
        }

        public int Actualizar(Presentacion t)
        {
            throw new NotImplementedException();
        }

        public int Eliminar(Presentacion t)
        {
            throw new NotImplementedException();
        }

        public List<Presentacion> ObtenerTodo()
        {
            string query = "SELECT Id, Nombre FROM presentacion WHERE Estado = 1 ORDER BY Nombre";
            MySqlCommand cmd = new MySqlCommand(query);
            //return ExecuteReturningDataTable(cmd);

            List<Presentacion> result = new List<Presentacion>();
            using (var reader = ExecuteReader(cmd)) {
                while (reader.Read())
                {

                    result.Add(
                        new Presentacion
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader["Nombre"].ToString()
                        }


                        );

                }
            }
            return result;

        }
    }
}
