using Npgsql;

namespace PostgresDbUp
{
    public class PostgresDbUper
    {
        private readonly string _connectionString;
        private readonly string _scriptsPath;

        public PostgresDbUper(string connectionString, string scriptsPath)
        {
            _connectionString = connectionString;
            _scriptsPath = scriptsPath;
        }

        public void RunMigration()
        {
            EnsureSchemaTableExists();

            var executedScripts = GetExecutedScripts();
            var scriptsToRun = Directory.GetFiles(_scriptsPath, "*.sql")
                .Select(Path.GetFileName)
                .Where(name => !executedScripts.Contains(name!))
                .OrderBy(name => name)
                .ToList();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            foreach (var scriptName in scriptsToRun)
            {
                var scriptPath = Path.Combine(_scriptsPath, scriptName!);
                var scriptContent = File.ReadAllText(scriptPath);

                using var transaction = connection.BeginTransaction();
                try
                {
                    using var cmd = new NpgsqlCommand(scriptContent, connection, transaction);
                    cmd.ExecuteNonQuery();

                    using var logCmd = new NpgsqlCommand(
                        "INSERT INTO __schema_versions (script_name, applied_at) VALUES (@name, NOW())",
                        connection, transaction);

                    logCmd.Parameters.AddWithValue("name", scriptName!);
                    logCmd.ExecuteNonQuery();

                    transaction.Commit();
                    Console.WriteLine($"Applied: {scriptName}");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void EnsureSchemaTableExists()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var createTable = @"
                CREATE TABLE IF NOT EXISTS __schema_versions (
                    id BIGSERIAL PRIMARY KEY,
                    script_name VARCHAR(255) NOT NULL UNIQUE,
                    applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                )";

            using var cmd = new NpgsqlCommand(createTable, connection);
            cmd.ExecuteNonQuery();
        }

        private HashSet<string> GetExecutedScripts()
        {
            var executedScripts = new HashSet<string>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT script_name FROM __schema_versions", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                executedScripts.Add(reader.GetString(0));
            }

            return executedScripts;
        }
    }
}
