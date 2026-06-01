using Microsoft.Data.Sqlite;
using HotelIS.Data;

namespace HotelIS.Forms;

internal sealed class RoomsForm : BaseEntityForm
{
    private NumericUpDown _roomNumber = null!;
    private TextBox _category = null!;
    private NumericUpDown _price = null!;
    private ComboBox _status = null!;

    protected override string TableName => "Номера";
    protected override string SelectSql =>
        "SELECT ID_Номера, Номер, Категория, Цена, Статус FROM Номера ORDER BY Номер";

    public RoomsForm() : this(false) { }

    public RoomsForm(bool readOnly)
        : base(readOnly: readOnly, allowAdd: !readOnly, allowEdit: !readOnly, allowDelete: !readOnly)
    {
        InitializeForm(readOnly ? "Каталог номеров (просмотр)" : "Номера");
        BuildInputs();
        ReloadData();
    }

    private void BuildInputs()
    {
        var panel = CreateInputPanel();
        var top = 20;
        AddLabel(panel, "Номер:", top);
        _roomNumber = AddNumeric(panel, top, 9999, 0);
        top += 40;
        AddLabel(panel, "Категория:", top);
        _category = AddTextBox(panel, top);
        top += 40;
        AddLabel(panel, "Цена за сутки:", top);
        _price = AddNumeric(panel, top);
        top += 40;
        AddLabel(panel, "Статус:", top);
        _status = AddComboBox(panel, top);
        _status.Items.AddRange(["Свободный", "Занят", "На ремонте"]);
        _status.SelectedIndex = 0;
    }

    protected override void ConfigureGrid()
    {
        Grid.DataBindingComplete += (_, _) =>
        {
            if (Grid.Columns.Contains("ID_Номера"))
                Grid.Columns["ID_Номера"].HeaderText = "ID";
            if (Grid.Columns.Contains("Номер"))
                Grid.Columns["Номер"].HeaderText = "Номер";
            if (Grid.Columns.Contains("Категория"))
                Grid.Columns["Категория"].HeaderText = "Категория";
            if (Grid.Columns.Contains("Цена"))
            {
                Grid.Columns["Цена"].HeaderText = "Цена";
                Grid.Columns["Цена"].DefaultCellStyle.Format = "N2";
            }
            if (Grid.Columns.Contains("Статус"))
                Grid.Columns["Статус"].HeaderText = "Статус";
        };
    }

    protected override void LoadSelectedRow()
    {
        if (Grid.CurrentRow == null)
            return;

        _roomNumber.Value = Convert.ToDecimal(Grid.CurrentRow.Cells["Номер"].Value);
        _category.Text = Grid.CurrentRow.Cells["Категория"].Value?.ToString() ?? "";
        _price.Value = Convert.ToDecimal(Grid.CurrentRow.Cells["Цена"].Value);
        var status = Grid.CurrentRow.Cells["Статус"].Value?.ToString() ?? "Свободный";
        _status.SelectedItem = status;
        if (_status.SelectedIndex < 0)
            _status.Text = status;
    }

    protected override void ClearInputs()
    {
        _roomNumber.Value = 1;
        _category.Clear();
        _price.Value = 0;
        _status.SelectedIndex = 0;
    }

    protected override bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_category.Text))
        {
            MessageBox.Show("Укажите категорию номера.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (_status.SelectedItem is null)
        {
            MessageBox.Show("Выберите статус номера.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    protected override void InsertRecord()
    {
        DatabaseHelper.ExecuteNonQuery(
            "INSERT INTO Номера (Номер, Категория, Цена, Статус) VALUES (@p1, @p2, @p3, @p4)",
            new SqliteParameter("@p1", (int)_roomNumber.Value),
            new SqliteParameter("@p2", _category.Text.Trim()),
            new SqliteParameter("@p3", _price.Value),
            new SqliteParameter("@p4", _status.SelectedItem!.ToString()));
    }

    protected override void UpdateRecord()
    {
        var id = GetSelectedId("ID_Номера") ?? throw new InvalidOperationException("Не выбран номер.");
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE Номера SET Номер=@p1, Категория=@p2, Цена=@p3, Статус=@p4 WHERE ID_Номера=@p5",
            new SqliteParameter("@p1", (int)_roomNumber.Value),
            new SqliteParameter("@p2", _category.Text.Trim()),
            new SqliteParameter("@p3", _price.Value),
            new SqliteParameter("@p4", _status.SelectedItem!.ToString()),
            new SqliteParameter("@p5", id));
    }

    protected override void DeleteRecord()
    {
        var id = GetSelectedId("ID_Номера") ?? throw new InvalidOperationException("Не выбран номер.");
        DatabaseHelper.ExecuteNonQuery(
            "DELETE FROM Номера WHERE ID_Номера=@p1",
            new SqliteParameter("@p1", id));
    }
}
