namespace HotelIS.Data;

internal static class DatabaseScriptRunner
{
    public static void ExecuteCreateScript() =>
        ExecuteScriptFile("CreateDatabase.sql", DatabaseInitializer.CreateTablesInCode);

    public static void ExecuteSeedScript() =>
        ExecuteScriptFile("SeedData.sql", DatabaseInitializer.SeedDemoDataInCode);

    private static void ExecuteScriptFile(string fileName, Action fallback)
    {
        var scriptPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Database",
            fileName);

        if (!File.Exists(scriptPath))
        {
            fallback();
            return;
        }

        var sql = File.ReadAllText(scriptPath);
        foreach (var statement in SplitStatements(sql))
        {
            if (string.IsNullOrWhiteSpace(statement))
                continue;
            DatabaseHelper.ExecuteNonQuery(statement);
        }
    }

    private static IEnumerable<string> SplitStatements(string sql)
    {
        var lines = sql.Split('\n');
        var buffer = new System.Text.StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("--", StringComparison.Ordinal))
                continue;

            buffer.AppendLine(line);
            if (!line.EndsWith(';'))
                continue;

            var statement = buffer.ToString().Trim().TrimEnd(';');
            buffer.Clear();
            if (statement.Length > 0)
                yield return statement;
        }

        var tail = buffer.ToString().Trim().TrimEnd(';');
        if (tail.Length > 0)
            yield return tail;
    }
}
