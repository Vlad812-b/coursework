using Microsoft.Data.Sqlite;

namespace HotelIS.Data;

internal static class DatabaseInitializer
{
    public static void Initialize()
    {
        DatabaseHelper.EnsureDatabaseReady();
        DatabaseScriptRunner.ExecuteCreateScript();
        SeedDataIfEmpty();
        ReplaceOutdatedDemoDataIfNeeded();
        FixAutoIncrementSequences();
    }

    /// <summary>Резервное создание таблиц, если SQL-файл не скопирован в папку сборки.</summary>
    internal static void CreateTablesInCode()
    {
        if (!DatabaseHelper.TableExists("Пользователи"))
        {
            DatabaseHelper.ExecuteNonQuery("""
                CREATE TABLE Пользователи (
                    Код INTEGER PRIMARY KEY AUTOINCREMENT,
                    Логин TEXT NOT NULL UNIQUE,
                    Пароль TEXT NOT NULL,
                    Роль TEXT NOT NULL,
                    Имя TEXT
                )
                """);
        }

        if (!DatabaseHelper.TableExists("Клиенты"))
        {
            DatabaseHelper.ExecuteNonQuery("""
                CREATE TABLE Клиенты (
                    ID_Клиента INTEGER PRIMARY KEY AUTOINCREMENT,
                    Фамилия TEXT NOT NULL,
                    Имя TEXT NOT NULL,
                    Телефон TEXT,
                    Паспорт TEXT
                )
                """);
        }

        if (!DatabaseHelper.TableExists("Номера"))
        {
            DatabaseHelper.ExecuteNonQuery("""
                CREATE TABLE Номера (
                    ID_Номера INTEGER PRIMARY KEY AUTOINCREMENT,
                    Номер INTEGER NOT NULL,
                    Категория TEXT NOT NULL,
                    Цена REAL NOT NULL,
                    Статус TEXT NOT NULL
                )
                """);
        }

        if (!DatabaseHelper.TableExists("Бронирование"))
        {
            DatabaseHelper.ExecuteNonQuery("""
                CREATE TABLE Бронирование (
                    ID_Брони INTEGER PRIMARY KEY AUTOINCREMENT,
                    ID_Клиента INTEGER NOT NULL,
                    ID_Номера INTEGER NOT NULL,
                    ДатаЗаезда TEXT NOT NULL,
                    ДатаВыезда TEXT NOT NULL,
                    Итог REAL NOT NULL,
                    FOREIGN KEY (ID_Клиента) REFERENCES Клиенты(ID_Клиента),
                    FOREIGN KEY (ID_Номера) REFERENCES Номера(ID_Номера)
                )
                """);
        }
    }

    private static void SeedDataIfEmpty()
    {
        SeedUsersIfEmpty();

        if (TableIsEmpty("Клиенты") && TableIsEmpty("Номера") && TableIsEmpty("Бронирование"))
        {
            try
            {
                DatabaseScriptRunner.ExecuteSeedScript();
            }
            catch
            {
                SeedDemoDataInCode();
            }
            return;
        }

        if (TableIsEmpty("Клиенты"))
            SeedClients();
        if (TableIsEmpty("Номера"))
            SeedRooms();
        if (TableIsEmpty("Бронирование"))
            SeedBookings();
    }

    internal static void SeedDemoDataInCode()
    {
        SeedClients();
        SeedRooms();
        SeedBookings();
    }

    private static void SeedClients()
    {
        InsertClient(1, "Иванов", "Иван", "+7(912)345-67-89", "4501 123456");
        InsertClient(2, "Петрова", "Мария", "+7(922)234-56-78", "4502 234567");
        InsertClient(3, "Сидоров", "Алексей", "+7(932)123-45-67", "4503 345678");
        InsertClient(4, "Козлова", "Елена", "+7(942)456-78-90", "4504 456789");
        InsertClient(5, "Смирнов", "Дмитрий", "+7(952)567-89-01", "4505 567890");
    }

    private static void SeedRooms()
    {
        InsertRoom(1, 1, "Семейный", 20000m, "Свободный");
        InsertRoom(2, 2, "Призеденский", 28000m, "Свободный");
        InsertRoom(3, 3, "Люкс", 25000m, "Занят");
        InsertRoom(4, 4, "Стандарт", 10000m, "Свободный");
        InsertRoom(5, 5, "Эконом", 5000m, "Занят");
    }

    private static void SeedBookings()
    {
        InsertBooking(1, 1, 1, new DateTime(2026, 5, 1), new DateTime(2026, 5, 8), 25000m);
        InsertBooking(2, 3, 3, new DateTime(2026, 6, 15), new DateTime(2026, 6, 22), 30000m);
        InsertBooking(3, 2, 5, new DateTime(2026, 5, 4), new DateTime(2026, 5, 24), 28000m);
        InsertBooking(4, 4, 2, new DateTime(2026, 6, 19), new DateTime(2026, 6, 27), 50000m);
        InsertBooking(5, 5, 4, new DateTime(2026, 5, 8), new DateTime(2026, 5, 12), 20000m);
        InsertBooking(6, 2, 2, new DateTime(2026, 5, 4), new DateTime(2026, 5, 14), 55000m);
    }

    private static void ReplaceOutdatedDemoDataIfNeeded()
    {
        if (TableIsEmpty("Клиенты"))
            return;

        var fifthClient = DatabaseHelper.ExecuteScalar(
            "SELECT Фамилия FROM Клиенты WHERE ID_Клиента = 5")?.ToString();

        if (fifthClient != "Новиков")
            return;

        DatabaseHelper.ExecuteNonQuery("DELETE FROM Бронирование");
        DatabaseHelper.ExecuteNonQuery("DELETE FROM Клиенты");
        DatabaseHelper.ExecuteNonQuery("DELETE FROM Номера");

        try
        {
            DatabaseScriptRunner.ExecuteSeedScript();
        }
        catch
        {
            SeedDemoDataInCode();
        }
    }

    private static void FixAutoIncrementSequences()
    {
        SetSequence("Клиенты", 5);
        SetSequence("Номера", 5);
        SetSequence("Бронирование", 6);
    }

    private static void SetSequence(string table, int lastId)
    {
        DatabaseHelper.ExecuteNonQuery(
            "INSERT OR REPLACE INTO sqlite_sequence (name, seq) VALUES (@name, @seq)",
            new SqliteParameter("@name", table),
            new SqliteParameter("@seq", lastId));
    }

    private static bool TableIsEmpty(string table) =>
        Convert.ToInt32(DatabaseHelper.ExecuteScalar($"SELECT COUNT(*) FROM {table}") ?? 0) == 0;

    private static void SeedUsersIfEmpty()
    {
        if (!TableIsEmpty("Пользователи"))
            return;

        InsertUser("admin", "admin", "Admin", "Администратор");
        InsertUser("user", "user", "User", "Пользователь");
    }

    private static void InsertUser(string login, string password, string role, string name)
    {
        DatabaseHelper.ExecuteNonQuery(
            "INSERT INTO Пользователи (Логин, Пароль, Роль, Имя) VALUES (@p1, @p2, @p3, @p4)",
            new SqliteParameter("@p1", login),
            new SqliteParameter("@p2", UserAuthService.HashPassword(password)),
            new SqliteParameter("@p3", role),
            new SqliteParameter("@p4", name));
    }

    private static void InsertClient(int id, string lastName, string firstName, string phone, string passport)
    {
        DatabaseHelper.ExecuteNonQuery(
            """
            INSERT INTO Клиенты (ID_Клиента, Фамилия, Имя, Телефон, Паспорт)
            VALUES (@id, @p1, @p2, @p3, @p4)
            """,
            new SqliteParameter("@id", id),
            new SqliteParameter("@p1", lastName),
            new SqliteParameter("@p2", firstName),
            new SqliteParameter("@p3", phone),
            new SqliteParameter("@p4", passport));
    }

    private static void InsertRoom(int id, int number, string category, decimal price, string status)
    {
        DatabaseHelper.ExecuteNonQuery(
            """
            INSERT INTO Номера (ID_Номера, Номер, Категория, Цена, Статус)
            VALUES (@id, @p1, @p2, @p3, @p4)
            """,
            new SqliteParameter("@id", id),
            new SqliteParameter("@p1", number),
            new SqliteParameter("@p2", category),
            new SqliteParameter("@p3", price),
            new SqliteParameter("@p4", status));
    }

    private static void InsertBooking(int id, int clientId, int roomId, DateTime checkIn, DateTime checkOut, decimal total)
    {
        DatabaseHelper.ExecuteNonQuery(
            """
            INSERT INTO Бронирование (ID_Брони, ID_Клиента, ID_Номера, ДатаЗаезда, ДатаВыезда, Итог)
            VALUES (@id, @p1, @p2, @p3, @p4, @p5)
            """,
            new SqliteParameter("@id", id),
            new SqliteParameter("@p1", clientId),
            new SqliteParameter("@p2", roomId),
            new SqliteParameter("@p3", checkIn.ToString("yyyy-MM-dd")),
            new SqliteParameter("@p4", checkOut.ToString("yyyy-MM-dd")),
            new SqliteParameter("@p5", total));
    }
}
