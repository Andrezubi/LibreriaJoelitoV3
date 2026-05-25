using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicioProveedores.Infraestructura.Persistence
{
    public class RepositorioBD
    {
        private static string? _connectionString;
        private static string? _databaseName;
        private static readonly Lazy<RepositorioBD> _instancia = new(() => new RepositorioBD());

        private MongoClient? _client;
        private IMongoDatabase? _database;

        // Soporte para transacciones en MongoDB mediante Sesiones
        private readonly AsyncLocal<IClientSessionHandle?> _activeSession = new();

        public static RepositorioBD Instancia => _instancia.Value;

        public void Initiate(string connectionString, string databaseName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
            _databaseName = databaseName;

            _client = new MongoClient(_connectionString);
            _database = _client.GetDatabase(_databaseName);
        }

        public IMongoDatabase GetDatabase()
        {
            if (_database == null)
                throw new InvalidOperationException("La base de datos no ha sido inicializada.");
            return _database;
        }

        #region Manejo de Transacciones 

        public async Task BeginTransactionAsync()
        {
            if (_activeSession.Value != null || _client == null) return;

            _activeSession.Value = await _client.StartSessionAsync();
            _activeSession.Value.StartTransaction();
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_activeSession.Value != null)
                    await _activeSession.Value.CommitTransactionAsync();
            }
            finally
            {
                CloseAndClearSession();
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_activeSession.Value != null)
                    await _activeSession.Value.AbortTransactionAsync();
            }
            finally
            {
                CloseAndClearSession();
            }
        }

        private void CloseAndClearSession()
        {
            _activeSession.Value?.Dispose();
            _activeSession.Value = null;
        }

        public IClientSessionHandle? Session => _activeSession.Value;

        #endregion

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return GetDatabase().GetCollection<T>(collectionName);
        }
    }
}
