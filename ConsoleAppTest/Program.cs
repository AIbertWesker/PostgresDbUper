
using PostgresDbUp;

var migrator = new PostgresDbUper(
    connectionString: "Host=localhost;Port=7004;Database=appdb;Username=appuser;Password=appsecret;Pooling=true;",
    scriptsPath: "scripts"
);

try
{
    migrator.RunMigration();
    Console.WriteLine("Migrations completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Migration failed: {ex.Message}");
    Environment.Exit(1);
}