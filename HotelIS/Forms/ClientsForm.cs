using Microsoft.Data.Sqlite;
using HotelIS.Data;

namespace HotelIS.Forms;

internal sealed class ClientsForm : BaseEntityForm
{
    private TextBox _lastName = null!;
    private TextBox _firstName = null!;
    private TextBox _phone = null!;
    private TextBox _passport = null!;

    protected override string TableName => "Клиенты";
    protected override string SelectSql =>
        "SELECT ID_Клиента, Фамилия, Имя, Телефон, Паспорт FROM Клиенты ORDER BY ID_Клиента";

    public ClientsForm()
    {
        InitializeForm("Клиенты");
        BuildInputs();
        ReloadData();
    }

    private void BuildInputs()
    {
        var panel = CreateInputPanel();
        var top = 20;
        AddLabel(panel, "Фамилия:", top);
        _lastName = AddTextBox(panel, top);
        top += 40;
        AddLabel(panel, "Имя:", top);
        _firstName = AddTextBox(panel, top);
        top += 40;
        AddLabel(panel, "Телефон:", top);
        _phone = AddTextBox(panel, top);
        top += 40;
        AddLabel(panel, "Паспорт:", top);
        _passport = AddTextBox(panel, top);
    }

    protected override void ConfigureGrid()
    {
        Grid.DataBindingComplete += (_, _) =>
        {
            if (Grid.Columns.Contains("ID_Клиента"))
                Grid.Columns["ID_Клиента"].HeaderText = "ID";
            if (Grid.Columns.Contains("Фамилия"))
                Grid.Columns["Фамилия"].HeaderText = "Фамилия";
            if (Grid.Columns.Contains("Имя"))
                Grid.Columns["Имя"].HeaderText = "Имя";
            if (Grid.Columns.Contains("Телефон"))
                Grid.Columns["Телефон"].HeaderText = "Телефон";
            if (Grid.Columns.Contains("Паспорт"))
                Grid.Columns["Паспорт"].HeaderText = "Паспорт";
        };
    }

    protected override void LoadSelectedRow()
    {
        if (Grid.CurrentRow == null)
            return;

        _lastName.Text = Grid.CurrentRow.Cells["Фамилия"].Value?.ToString() ?? "";
        _firstName.Text = Grid.CurrentRow.Cells["Имя"].Value?.ToString() ?? "";
        _phone.Text = Grid.CurrentRow.Cells["Телефон"].Value?.ToString() ?? "";
        _passport.Text = Grid.CurrentRow.Cells["Паспорт"].Value?.ToString() ?? "";
    }

    protected override void ClearInputs()
    {
        _lastName.Clear();
        _firstName.Clear();
        _phone.Clear();
        _passport.Clear();
    }

    protected override bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_lastName.Text) || string.IsNullOrWhiteSpace(_firstName.Text))
        {
            MessageBox.Show("Укажите фамилию и имя клиента.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    protected override void InsertRecord()
    {
        DatabaseHelper.ExecuteNonQuery(
            "INSERT INTO Клиенты (Фамилия, Имя, Телефон, Паспорт) VALUES (@p1, @p2, @p3, @p4)",
            new SqliteParameter("@p1", _lastName.Text.Trim()),
            new SqliteParameter("@p2", _firstName.Text.Trim()),
            new SqliteParameter("@p3", _phone.Text.Trim()),
            new SqliteParameter("@p4", _passport.Text.Trim()));
    }

    protected override void UpdateRecord()
    {
        var id = GetSelectedId("ID_Клиента") ?? throw new InvalidOperationException("Не выбран клиент.");
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE Клиенты SET Фамилия=@p1, Имя=@p2, Телефон=@p3, Паспорт=@p4 WHERE ID_Клиента=@p5",
            new SqliteParameter("@p1", _lastName.Text.Trim()),
            new SqliteParameter("@p2", _firstName.Text.Trim()),
            new SqliteParameter("@p3", _phone.Text.Trim()),
            new SqliteParameter("@p4", _passport.Text.Trim()),
            new SqliteParameter("@p5", id));
    }

    protected override void DeleteRecord()
    {
        var id = GetSelectedId("ID_Клиента") ?? throw new InvalidOperationException("Не выбран клиент.");
        DatabaseHelper.ExecuteNonQuery(
            "DELETE FROM Клиенты WHERE ID_Клиента=@p1",
            new SqliteParameter("@p1", id));
    }
}
