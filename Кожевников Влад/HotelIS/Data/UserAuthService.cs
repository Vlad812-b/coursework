using System.Security.Cryptography;
using System.Text;
using HotelIS.Auth;
using Microsoft.Data.Sqlite;

namespace HotelIS.Data;

internal static class UserAuthService
{
    public static bool TryLogin(string login, string password, out string? error)
    {
        error = null;
        login = login.Trim();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            error = "Введите логин и пароль.";
            return false;
        }

        var table = DatabaseHelper.ExecuteQuery(
            "SELECT Код, Логин, Пароль, Роль, Имя FROM Пользователи WHERE Логин = @login COLLATE NOCASE",
            new SqliteParameter("@login", login));

        if (table.Rows.Count == 0)
        {
            error = "Неверный логин или пароль.";
            return false;
        }

        var row = table.Rows[0];
        var storedHash = row["Пароль"]?.ToString() ?? "";
        if (storedHash != HashPassword(password))
        {
            error = "Неверный логин или пароль.";
            return false;
        }

        var role = ParseRole(row["Роль"]?.ToString());
        var displayName = row["Имя"]?.ToString();
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = row["Логин"]?.ToString() ?? login;

        SessionUser.Set(Convert.ToInt32(row["Код"]), login, displayName, role);
        return true;
    }

    public static bool TryRegister(
        string login,
        string password,
        string confirmPassword,
        string displayName,
        UserRole role,
        out string? error)
    {
        error = null;
        login = login.Trim();
        displayName = displayName.Trim();

        if (string.IsNullOrWhiteSpace(login))
        {
            error = "Укажите логин.";
            return false;
        }

        if (login.Length < 3)
        {
            error = "Логин должен быть не короче 3 символов.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Укажите пароль.";
            return false;
        }

        if (password.Length < 4)
        {
            error = "Пароль должен быть не короче 4 символов.";
            return false;
        }

        if (password != confirmPassword)
        {
            error = "Пароли не совпадают.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = login;

        var exists = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
            "SELECT COUNT(*) FROM Пользователи WHERE Логин = @login COLLATE NOCASE",
            new SqliteParameter("@login", login)) ?? 0);

        if (exists > 0)
        {
            error = "Пользователь с таким логином уже существует.";
            return false;
        }

        try
        {
            DatabaseHelper.ExecuteNonQuery(
                """
                INSERT INTO Пользователи (Логин, Пароль, Роль, Имя)
                VALUES (@login, @password, @role, @name)
                """,
                new SqliteParameter("@login", login),
                new SqliteParameter("@password", HashPassword(password)),
                new SqliteParameter("@role", RoleToDb(role)),
                new SqliteParameter("@name", displayName));
        }
        catch (SqliteException)
        {
            error = "Не удалось зарегистрировать пользователя.";
            return false;
        }

        return true;
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private static UserRole ParseRole(string? value) =>
        string.Equals(value, "Admin", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "Администратор", StringComparison.OrdinalIgnoreCase)
            ? UserRole.Admin
            : UserRole.User;

    private static string RoleToDb(UserRole role) =>
        role == UserRole.Admin ? "Admin" : "User";
}
