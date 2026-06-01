using System.Data;
using Microsoft.Data.Sqlite;

namespace HotelIS.Data;

internal static class DatabaseHelper
{
    private static readonly string DbFileName = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Database",
        "Hotel.db");

    public static string DatabasePath => DbFileName;

    public static string ConnectionString => $"Data Source={DbFileName}";

    public static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    public static void EnsureDatabaseReady()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbFileName)!);
        using var _ = CreateConnection();
    }

    public static DataTable ExecuteQuery(string sql, params SqliteParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = new SqliteCommand(sql, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public static int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = new SqliteCommand(sql, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);
        return command.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(string sql, params SqliteParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = new SqliteCommand(sql, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);
        return command.ExecuteScalar();
    }

    public static bool TableExists(string tableName)
    {
        var count = Convert.ToInt32(ExecuteScalar(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name",
            new SqliteParameter("@name", tableName)) ?? 0);
        return count > 0;
    }
}
