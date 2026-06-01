-- База данных гостиницы (SQLite)
-- Файл Hotel.db создаётся автоматически при запуске программы.
-- MySQL и отдельный сервер БД НЕ требуются.

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Пользователи (
    Код INTEGER PRIMARY KEY AUTOINCREMENT,
    Логин TEXT NOT NULL UNIQUE,
    Пароль TEXT NOT NULL,
    Роль TEXT NOT NULL,
    Имя TEXT
);

CREATE TABLE IF NOT EXISTS Клиенты (
    ID_Клиента INTEGER PRIMARY KEY AUTOINCREMENT,
    Фамилия TEXT NOT NULL,
    Имя TEXT NOT NULL,
    Телефон TEXT,
    Паспорт TEXT
);

CREATE TABLE IF NOT EXISTS Номера (
    ID_Номера INTEGER PRIMARY KEY AUTOINCREMENT,
    Номер INTEGER NOT NULL,
    Категория TEXT NOT NULL,
    Цена REAL NOT NULL,
    Статус TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Бронирование (
    ID_Брони INTEGER PRIMARY KEY AUTOINCREMENT,
    ID_Клиента INTEGER NOT NULL,
    ID_Номера INTEGER NOT NULL,
    ДатаЗаезда TEXT NOT NULL,
    ДатаВыезда TEXT NOT NULL,
    Итог REAL NOT NULL,
    FOREIGN KEY (ID_Клиента) REFERENCES Клиенты(ID_Клиента),
    FOREIGN KEY (ID_Номера) REFERENCES Номера(ID_Номера)
);
