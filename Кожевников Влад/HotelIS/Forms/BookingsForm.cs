using System.Data;
using Microsoft.Data.Sqlite;
using HotelIS.Data;

namespace HotelIS.Forms;

internal sealed class BookingsForm : BaseEntityForm
{
    private ComboBox _client = null!;
    private ComboBox _room = null!;
    private DateTimePicker _checkIn = null!;
    private DateTimePicker _checkOut = null!;
    private NumericUpDown _total = null!;

    protected override string TableName => "Бронирование";
    protected override string SelectSql =>
        """
        SELECT b.ID_Брони,
               b.ID_Клиента,
               c.Фамилия || ' ' || c.Имя AS Клиент,
               b.ID_Номера,
               n.Номер AS НомерКомнаты,
               n.Категория,
               b.ДатаЗаезда,
               b.ДатаВыезда,
               b.Итог
        FROM Бронирование AS b
        INNER JOIN Клиенты AS c ON b.ID_Клиента = c.ID_Клиента
        INNER JOIN Номера AS n ON b.ID_Номера = n.ID_Номера
        ORDER BY b.ID_Брони
        """;

    public BookingsForm() : this(false, false) { }

    public BookingsForm(bool userMode, bool viewOnly)
        : base(
            readOnly: viewOnly,
            allowAdd: !viewOnly,
            allowEdit: !userMode && !viewOnly,
            allowDelete: !userMode && !viewOnly)
    {
        var title = viewOnly ? "Мои бронирования (просмотр)"
            : userMode ? "Бронирование номера"
            : "Бронирование";
        InitializeForm(title, 980, 600);
        BuildInputs();
        LoadLookups();
        ReloadData();
    }

    private void BuildInputs()
    {
        var panel = CreateInputPanel();
        var top = 20;
        AddLabel(panel, "Клиент:", top);
        _client = AddComboBox(panel, top, 400);
        top += 40;
        AddLabel(panel, "Номер:", top);
        _room = AddComboBox(panel, top, 400);
        top += 40;
        AddLabel(panel, "Дата заезда:", top);
        _checkIn = AddDatePicker(panel, top);
        _checkIn.ValueChanged += (_, _) => RecalculateTotal();
        top += 40;
        AddLabel(panel, "Дата выезда:", top);
        _checkOut = AddDatePicker(panel, top);
        _checkOut.ValueChanged += (_, _) => RecalculateTotal();
        top += 40;
        AddLabel(panel, "Итог (руб.):", top);
        _total = AddNumeric(panel, top, 10000000, 2);
        _room.SelectedIndexChanged += (_, _) => RecalculateTotal();
    }

    private void LoadLookups()
    {
        var clients = DatabaseHelper.ExecuteQuery(
            "SELECT ID_Клиента, Фамилия, Имя FROM Клиенты ORDER BY Фамилия, Имя");
        _client.DisplayMember = "Display";
        _client.ValueMember = "ID_Клиента";
        clients.Columns.Add("Display", typeof(string));
        foreach (DataRow row in clients.Rows)
            row["Display"] = $"{row["Фамилия"]} {row["Имя"]}";
        _client.DataSource = clients;

        var rooms = DatabaseHelper.ExecuteQuery(
            "SELECT ID_Номера, Номер, Категория, Цена FROM Номера ORDER BY Номер");
        _room.DisplayMember = "Display";
        _room.ValueMember = "ID_Номера";
        rooms.Columns.Add("Display", typeof(string));
        foreach (DataRow row in rooms.Rows)
            row["Display"] = $"№{row["Номер"]} — {row["Категория"]} ({Convert.ToDecimal(row["Цена"]):N0} ₽)";
        _room.DataSource = rooms;
    }

    private void RecalculateTotal()
    {
        if (_room.SelectedValue is not int roomId || _checkOut.Value.Date <= _checkIn.Value.Date)
            return;

        var priceObj = DatabaseHelper.ExecuteScalar(
            "SELECT Цена FROM Номера WHERE ID_Номера=@p1",
            new SqliteParameter("@p1", roomId));

        if (priceObj is null or DBNull)
            return;

        var nights = (_checkOut.Value.Date - _checkIn.Value.Date).Days;
        var price = Convert.ToDecimal(priceObj);
        _total.Value = Math.Max(0, nights * price);
    }

    protected override void ConfigureGrid()
    {
        Grid.DataBindingComplete += (_, _) =>
        {
            HideColumn("ID_Клиента");
            HideColumn("ID_Номера");
            SetHeader("ID_Брони", "ID");
            SetHeader("Клиент", "Клиент");
            SetHeader("НомерКомнаты", "Номер");
            SetHeader("Категория", "Категория");
            SetHeader("ДатаЗаезда", "Заезд");
            SetHeader("ДатаВыезда", "Выезд");
            SetHeader("Итог", "Итог");
            if (Grid.Columns.Contains("Итог"))
                Grid.Columns["Итог"].DefaultCellStyle.Format = "N2";
            if (Grid.Columns.Contains("ДатаЗаезда"))
                Grid.Columns["ДатаЗаезда"].DefaultCellStyle.Format = "d";
            if (Grid.Columns.Contains("ДатаВыезда"))
                Grid.Columns["ДатаВыезда"].DefaultCellStyle.Format = "d";
        };
    }

    private void HideColumn(string name)
    {
        if (Grid.Columns.Contains(name))
            Grid.Columns[name].Visible = false;
    }

    private void SetHeader(string name, string header)
    {
        if (Grid.Columns.Contains(name))
            Grid.Columns[name].HeaderText = header;
    }

    protected override void LoadSelectedRow()
    {
        if (Grid.CurrentRow == null)
            return;

        _client.SelectedValue = Grid.CurrentRow.Cells["ID_Клиента"].Value;
        _room.SelectedValue = Grid.CurrentRow.Cells["ID_Номера"].Value;
        _checkIn.Value = ParseDate(Grid.CurrentRow.Cells["ДатаЗаезда"].Value);
        _checkOut.Value = ParseDate(Grid.CurrentRow.Cells["ДатаВыезда"].Value);
        _total.Value = Convert.ToDecimal(Grid.CurrentRow.Cells["Итог"].Value);
    }

    protected override void ClearInputs()
    {
        if (_client.Items.Count > 0)
            _client.SelectedIndex = 0;
        if (_room.Items.Count > 0)
            _room.SelectedIndex = 0;
        _checkIn.Value = DateTime.Today;
        _checkOut.Value = DateTime.Today.AddDays(1);
        RecalculateTotal();
    }

    protected override bool ValidateInput()
    {
        if (_client.SelectedValue is null)
        {
            MessageBox.Show("Выберите клиента.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (_room.SelectedValue is null)
        {
            MessageBox.Show("Выберите номер.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (_checkOut.Value.Date <= _checkIn.Value.Date)
        {
            MessageBox.Show("Дата выезда должна быть позже даты заезда.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private int GetSelectedClientId() => Convert.ToInt32(_client.SelectedValue!);

    private int GetSelectedRoomId() => Convert.ToInt32(_room.SelectedValue!);

    protected override void InsertRecord()
    {
        DatabaseHelper.ExecuteNonQuery(
            """
            INSERT INTO Бронирование (ID_Клиента, ID_Номера, ДатаЗаезда, ДатаВыезда, Итог)
            VALUES (@p1, @p2, @p3, @p4, @p5)
            """,
            new SqliteParameter("@p1", GetSelectedClientId()),
            new SqliteParameter("@p2", GetSelectedRoomId()),
            new SqliteParameter("@p3", _checkIn.Value.ToString("yyyy-MM-dd")),
            new SqliteParameter("@p4", _checkOut.Value.ToString("yyyy-MM-dd")),
            new SqliteParameter("@p5", _total.Value));
        UpdateRoomStatus(GetSelectedRoomId());
    }

    protected override void UpdateRecord()
    {
        var id = GetSelectedId("ID_Брони") ?? throw new InvalidOperationException("Не выбрана бронь.");
        DatabaseHelper.ExecuteNonQuery(
            """
            UPDATE Бронирование
            SET ID_Клиента=@p1, ID_Номера=@p2, ДатаЗаезда=@p3, ДатаВыезда=@p4, Итог=@p5
            WHERE ID_Брони=@p6
            """,
            new SqliteParameter("@p1", GetSelectedClientId()),
            new SqliteParameter("@p2", GetSelectedRoomId()),
            new SqliteParameter("@p3", _checkIn.Value.ToString("yyyy-MM-dd")),
            new SqliteParameter("@p4", _checkOut.Value.ToString("yyyy-MM-dd")),
            new SqliteParameter("@p5", _total.Value),
            new SqliteParameter("@p6", id));
        UpdateRoomStatus(GetSelectedRoomId());
    }

    protected override void DeleteRecord()
    {
        var id = GetSelectedId("ID_Брони") ?? throw new InvalidOperationException("Не выбрана бронь.");
        var roomId = Convert.ToInt32(Grid.CurrentRow!.Cells["ID_Номера"].Value);
        DatabaseHelper.ExecuteNonQuery(
            "DELETE FROM Бронирование WHERE ID_Брони=@p1",
            new SqliteParameter("@p1", id));
        RefreshRoomStatus(roomId);
    }

    private static void UpdateRoomStatus(int roomId)
    {
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE Номера SET Статус='Занят' WHERE ID_Номера=@p1",
            new SqliteParameter("@p1", roomId));
    }

    private static void RefreshRoomStatus(int roomId)
    {
        var active = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
            """
            SELECT COUNT(*) FROM Бронирование
            WHERE ID_Номера=@p1 AND date(ДатаВыезда) >= date(@p2)
            """,
            new SqliteParameter("@p1", roomId),
            new SqliteParameter("@p2", DateTime.Today.ToString("yyyy-MM-dd"))) ?? 0);

        var status = active > 0 ? "Занят" : "Свободный";
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE Номера SET Статус=@p1 WHERE ID_Номера=@p2",
            new SqliteParameter("@p1", status),
            new SqliteParameter("@p2", roomId));
    }

    private static DateTime ParseDate(object? value)
    {
        if (value is DateTime dt)
            return dt;
        return DateTime.Parse(value?.ToString() ?? DateTime.Today.ToString());
    }
}
