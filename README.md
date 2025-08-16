# PostgresDbUper

**PostgresDbUper** is a simple PostgreSQL migration tool for .NET. It automatically executes SQL scripts in a specified folder and logs applied migrations in a `__schema_versions` table.

---

# Usage
```csharp
var migrator = new PostgresDbUper(
    connectionString: "Host=localhost;Port=7004;Database=appdb;Username=appuser;Password=appsecret;Pooling=true;",
    scriptsPath: "scripts"
);

migrator.RunMigration();
```
