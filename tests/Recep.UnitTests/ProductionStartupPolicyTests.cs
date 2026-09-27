namespace Recep.UnitTests;

public sealed class ProductionStartupPolicyTests
{
    [Fact]
    public void Program_keeps_database_migration_inside_explicit_development_gate()
    {
        var program = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "API", "Program.cs"));

        program.Should().Contain("app.Environment.IsDevelopment()");
        program.Should().Contain("Database:AutoMigrate");
        program.Should().Contain("await db.Database.MigrateAsync();");
        program.Should().NotContain("EnsureCreated");
        program.Should().NotContain("EnsureDeleted");
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "API")) &&
                File.Exists(Path.Combine(directory.FullName, "Recep.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root could not be located.");
    }
}
