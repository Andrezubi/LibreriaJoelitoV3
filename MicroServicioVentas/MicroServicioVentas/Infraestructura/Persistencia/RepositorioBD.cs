using MySql.Data.MySqlClient;
using System.Data;
using System.Threading;

namespace MicroServicioVentas.Infraestructura.Persistencia
{
    public class RepositorioBD
    {
        private static string? _connectionString;
        private static readonly Lazy<RepositorioBD> _instancia = new(() => new RepositorioBD());

        // IMPORTANTE:
        // Deben ser static para que la transacción sea compartida entre todos los repositorios.
        private static readonly AsyncLocal<MySqlTransaction?> _activeTransaction = new();
        private static readonly AsyncLocal<MySqlConnection?> _activeConnection = new();

        public static RepositorioBD Instancia => _instancia.Value;

        private string CatchStringConnection()
        {
            return _connectionString
                ?? throw new InvalidOperationException("La cadena de conexión no ha sido configurada.");
        }

        public void Initiate(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "La cadena de conexión no puede ser nula o vacía.");

            _connectionString = connectionString;
        }

        public void BeginTransaction()
        {
            if (_activeTransaction.Value != null)
                return;

            var connection = new MySqlConnection(CatchStringConnection());
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

        public int ExecuteNonQuery(MySqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return comando.ExecuteNonQuery();
            }

            using var con = new MySqlConnection(CatchStringConnection());
            con.Open();

            comando.Connection = con;
            return comando.ExecuteNonQuery();
        }

        public object? ExecuteScalar(MySqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return comando.ExecuteScalar();
            }

            using var con = new MySqlConnection(CatchStringConnection());
            con.Open();

            comando.Connection = con;
            return comando.ExecuteScalar();
        }

        public MySqlDataReader ExecuteReader(MySqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return comando.ExecuteReader();
            }

            var con = new MySqlConnection(CatchStringConnection());
            con.Open();

            comando.Connection = con;
            return comando.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public MySqlDataAdapter ExecuteDataAdapter(MySqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;
                return new MySqlDataAdapter(comando);
            }

            var con = new MySqlConnection(CatchStringConnection());
            comando.Connection = con;

            return new MySqlDataAdapter(comando);
        }

        public DataTable ExecuteReturningDataTable(MySqlCommand comando)
        {
            if (_activeTransaction.Value != null)
            {
                comando.Connection = _activeConnection.Value;
                comando.Transaction = _activeTransaction.Value;

                using var dataAdapter = new MySqlDataAdapter(comando);
                var dataTable = new DataTable();

                dataAdapter.Fill(dataTable);
                return dataTable;
            }

            using var con = new MySqlConnection(CatchStringConnection());
            con.Open();

            comando.Connection = con;

            using var adapter = new MySqlDataAdapter(comando);
            var table = new DataTable();

            adapter.Fill(table);
            return table;
        }

        public DataRow? ExecuteReturningDataRow(MySqlCommand comando)
        {
            DataTable dt = ExecuteReturningDataTable(comando);

            if (dt.Rows.Count > 0)
                return dt.Rows[0];

            return null;
        }
    }
}