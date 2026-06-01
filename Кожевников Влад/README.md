# Сопровождение информационной системы гостиничного бизнеса

Приложение **C# Windows Forms** для Visual Studio.

## База данных (без MySQL)

Используется **SQLite** — один файл `Hotel.db` на диске.  
**Не нужно** устанавливать MySQL, SQL Server, Access или другие серверы.

| Что | Где |
|-----|-----|
| СУБД | SQLite (пакет `Microsoft.Data.Sqlite` в проекте) |
| Скрипт создания | `HotelIS\Database\CreateDatabase.sql` |
| Файл базы после запуска | `HotelIS\bin\Debug\net8.0-windows\Database\Hotel.db` |
| Описание таблиц | `HotelIS\Database\Схема_базы.txt` |

База **создаётся сама** при первом запуске программы.

## Запуск

1. Откройте **`HotelIS.sln`** в Visual Studio 2022.
2. Нажмите **F5**.

```powershell
cd "HotelIS"
dotnet run
```

## Вход в систему

| Логин | Пароль | Роль |
|--------|--------|------|
| `admin` | `admin` | Администратор |
| `user` | `user` | Пользователь |

## Требования

- Windows 10/11
- Visual Studio 2022 (.NET desktop development)
- .NET 8 SDK
