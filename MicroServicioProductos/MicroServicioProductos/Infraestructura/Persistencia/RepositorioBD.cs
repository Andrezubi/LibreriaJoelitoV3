using System.Data;
using System.Data.Common;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace MicroServicioProductos.Infraestructura.Persistencia
{
    public class RepositorioBD
    {
        private static string? _connectionString;
        private static readonly Lazy<RepositorioBD> _instancia = new Lazy<RepositorioBD>(() => new RepositorioBD());
        
        // Soporte para transacciones concurrentes por hilo/tarea
        private readonly AsyncLocal<SqlTransaction?> _activeTransaction = new();
        private readonly AsyncLocal<SqlConnection?> _activeConnection = new();

        public static RepositorioBD Instancia
        {
            get
            {
                return _instancia.Value;
            }
        }

        private string CatchStringConnection()
        {
            return _connectionString
                ?? throw new InvalidOperationException("La cadena de conexión no ha sido configurada. Por favor, configure la cadena de conexión antes de usar el repositorio.");
        }

        public void Initiate(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString", "La cadena de conexión no puede ser nula o vacía.");
            _connectionString = connectionString;
        }

        #region Manejo de Transacciones
        public void BeginTransaction()
        {
            if (_activeTransaction.Value != null) return;

            var connection = new SqlConnection(CatchStringConnection());
            connection.Open();
            var transaction = connection.BeginTransaction();

            _activeConnection.Value = connection;
            _activeTransaction.Value = transaction;
        }

        public void Commit()
        {
            try
            {
                _activeTransaction.Value?.Commit();
            }
            finally
            {
                CloseAndClearTransaction();
            }
        }

        public void Rollback()
        {
            try
            {
                _activeTransaction.Value?.Rollback();
            }
            finally
            {
                CloseAndClearTransaction();
            }
        }

        private void CloseAndClearTransaction()
        {
            if (_activeConnection.Value != null)
            {
                if (_activeConnection.Value.State == ConnectionState.Open)
                    _activeConnection.Value.Close();
                _activeConnection.Value.Dispose();
            }
            _activeConnection.Value = null;
            _activeTransaction.Value = null;
        }
        #endregion

        public int ExecuteNonQuery(SqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return comando.ExecuteNonQuery();
            }

            using (SqlConnection con = new SqlConnection(CatchStringConnection()))
            {
                con.Open();
                comando.Connection = con;
                return comando.ExecuteNonQuery();
            }
        }

        public SqlDataReader ExecuteReader(SqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return comando.ExecuteReader();
            }

            SqlConnection con = new SqlConnection(CatchStringConnection());
            con.Open();
            comando.Connection = con;
            return comando.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public SqlDataAdapter ExecuteDataAdapter(SqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return new SqlDataAdapter(comando);
            }

            SqlConnection con = new SqlConnection(CatchStringConnection());
            comando.Connection = con;
            return new SqlDataAdapter(comando);
        }

        public DataTable ExecuteReturningDataTable(SqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(comando))
                {
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    return dataTable;
                }
            }

            using (SqlConnection con = new SqlConnection(CatchStringConnection()))
            {
                con.Open();
                comando.Connection = con;

                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(comando))
                {
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        public DataRow? ExecuteReturningDataRow(SqlCommand comando)
        {
            DataTable dt = ExecuteReturningDataTable(comando);
            if (dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }

        public object? ExecuteScalar(SqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return comando.ExecuteScalar();
            }

            using (SqlConnection con = new SqlConnection(CatchStringConnection()))
            {
                con.Open();
                comando.Connection = con;
                return comando.ExecuteScalar();
            }
        }
    }
}
