using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DockerTest.Api.Tests.Docker
{
    public sealed class MsSqlTestContainer : TestContainer
    {
        private readonly string _saPassword;

        private readonly bool _useContainerIpAddresses;
        private Lazy<string> _connectionString;

        public MsSqlTestContainer(string saPassword, string containerName, bool useContainerIpAddresses, Action<string> logger = null)
            : base("microsoft/mssql-server-linux", containerName,
                  environmentVariables: new[] { "ACCEPT_EULA=Y", $"SA_PASSWORD={saPassword}", "MSSQL_COLLATION=SQL_Latin1_General_CP1_CS_AS" },
                  logger: logger)
        {
            _saPassword = saPassword;
            _useContainerIpAddresses = useContainerIpAddresses;
        }

        protected override async Task Wait()
        {
            var attempts = AttemptsCount;
            var isAlive = false;
            do
            {
                try
                {
                    using (var connection = new SqlConnection(GetConnectionString()))
                    using (var command = new SqlCommand("SELECT @@VERSION", connection))
                    {
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }

                    isAlive = true;
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is NotSupportedException || ex is SqlException)
                {
                    Logger?.Invoke(ex.Message);
                }

                if (!isAlive)
                {
                    attempts--;
                    await Task.Delay(DelayTime);
                }
            }
            while (!isAlive && attempts != 0);

            if (attempts == 0)
            {
                throw new XunitException("MSSQL didn't start");
            }
        }

        private string GetConnectionString()
        {
            _connectionString = _connectionString ?? new Lazy<string>(() => $"Data Source={(_useContainerIpAddresses ? IpAddress : "localhost")}, {(_useContainerIpAddresses ? 1433 : Ports[1433])}; UID=sa; pwd={_saPassword};");
            return _connectionString.Value;
        }
    }
}
