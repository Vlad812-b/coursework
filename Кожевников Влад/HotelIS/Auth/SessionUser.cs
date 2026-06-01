namespace HotelIS.Auth;

internal static class SessionUser
{
    public static int Id { get; private set; }
    public static string Login { get; private set; } = "";
    public static string DisplayName { get; private set; } = "";
    public static UserRole Role { get; private set; }

    public static bool IsAuthenticated => Id > 0;
    public static bool IsAdmin => Role == UserRole.Admin;

    public static void Set(int id, string login, string displayName, UserRole role)
    {
        Id = id;
        Login = login;
        DisplayName = displayName;
        Role = role;
    }

    public static void Clear()
    {
        Id = 0;
        Login = "";
        DisplayName = "";
        Role = UserRole.User;
    }
}
