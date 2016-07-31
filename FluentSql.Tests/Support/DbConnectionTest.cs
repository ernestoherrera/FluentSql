using System.Data;
using System.Data.SqlClient;

namespace FluentSql.Tests.Support
{
    public class DbConnectionTest : IDbConnection
    {
        private IDbConnection _dbConnection;
        public string ConnectionString
        {
            get { return _dbConnection.ConnectionString; }

            set { _dbConnection.ConnectionString = value; }
        }

        public int ConnectionTimeout
        {
            get { return _dbConnection.ConnectionTimeout; }
        }

        public string Database
        {
            get { return _dbConnection.Database; }
        }

        public ConnectionState State
        {
            get { return _dbConnection.State; }
        }

        public object ConfigurationManager { get; private set; }

        public IDbTransaction BeginTransaction()
        {
            return _dbConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _dbConnection.BeginTransaction(il);
        }

        public void Close()
        {
            _dbConnection.Close();
        }

        public void ChangeDatabase(string databaseName)
        {
            _dbConnection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            return _dbConnection.CreateCommand();
        }

        public void Open()
        {
            _dbConnection.Open();
        }

        public void Dispose()
        {
            if (_dbConnection.State != ConnectionState.Closed)
                _dbConnection.Close();

            _dbConnection.Dispose();
        }

        public DbConnectionTest(string connectionString)
        {
            _dbConnection = new SqlConnection(connectionString);

            try
            {
                _dbConnection.Open();
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
