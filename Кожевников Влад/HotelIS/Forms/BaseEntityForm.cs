using System.Data;
using HotelIS.Data;

namespace HotelIS.Forms;

internal abstract class BaseEntityForm : Form
{
    private readonly bool _readOnly;
    private readonly bool _allowAdd;
    private readonly bool _allowEdit;
    private readonly bool _allowDelete;

    protected DataGridView Grid = null!;
    protected BindingSource Binding = new();

    protected BaseEntityForm(bool readOnly = false, bool allowAdd = true, bool allowEdit = true, bool allowDelete = true)
    {
        _readOnly = readOnly;
        _allowAdd = allowAdd && !readOnly;
        _allowEdit = allowEdit && !readOnly;
        _allowDelete = allowDelete && !readOnly;
    }

    protected abstract string TableName { get; }
    protected abstract string SelectSql { get; }
    protected abstract void ConfigureGrid();
    protected abstract void LoadSelectedRow();
    protected abstract void ClearInputs();
    protected abstract bool ValidateInput();
    protected abstract void InsertRecord();
    protected abstract void UpdateRecord();
    protected abstract void DeleteRecord();

    protected void InitializeForm(string title, int width = 900, int height = 560)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(width, height);
        ClientSize = new Size(width, height);
        Font = new Font("Segoe UI", 10F);

        Grid = new DataGridView
        {
            Dock = DockStyle.Top,
            Height = 260,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };
        Grid.SelectionChanged += (_, _) => LoadSelectedRow();

        Controls.Add(Grid);
        ConfigureGrid();
        CreateButtons();
    }

    protected Panel CreateInputPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            AutoScroll = true
        };
        Controls.Add(panel);
        panel.BringToFront();
        return panel;
    }

    private void CreateButtons()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(8),
            FlowDirection = FlowDirection.LeftToRight
        };

        var actions = new List<(string, EventHandler)>
        {
            ("Обновить", (_, _) => ReloadData()),
            ("Закрыть", (_, _) => Close())
        };
        if (_allowAdd)
            actions.Insert(1, ("Добавить", (_, _) => SaveNew()));
        if (_allowEdit)
            actions.Insert(_allowAdd ? 2 : 1, ("Изменить", (_, _) => SaveEdit()));
        if (_allowDelete)
            actions.Insert(actions.Count - 1, ("Удалить", (_, _) => RemoveSelected()));

        foreach (var (text, handler) in actions)
            panel.Controls.Add(CreateButton(text, handler));

        Controls.Add(panel);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_readOnly || (!_allowAdd && !_allowEdit))
        {
            foreach (Control control in Controls)
            {
                if (control is Panel { Dock: DockStyle.Fill, AutoScroll: true } inputPanel)
                    SetInputsReadOnly(inputPanel);
            }
        }
    }

    private static void SetInputsReadOnly(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.ReadOnly = true;
                    break;
                case ComboBox comboBox:
                    comboBox.Enabled = false;
                    break;
                case NumericUpDown numeric:
                    numeric.Enabled = false;
                    break;
                case DateTimePicker picker:
                    picker.Enabled = false;
                    break;
                default:
                    if (control.HasChildren)
                        SetInputsReadOnly(control);
                    break;
            }
        }
    }

    private static Button CreateButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(6),
            Padding = new Padding(12, 6, 12, 6)
        };
        button.Click += onClick;
        return button;
    }

    protected void ReloadData()
    {
        var table = DatabaseHelper.ExecuteQuery(SelectSql);
        Binding.DataSource = table;
        Grid.DataSource = Binding;
        if (Grid.Rows.Count > 0)
            Grid.Rows[0].Selected = true;
        else
            ClearInputs();
    }

    private void SaveNew()
    {
        if (!ValidateInput())
            return;

        try
        {
            InsertRecord();
            ReloadData();
            MessageBox.Show("Запись добавлена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void SaveEdit()
    {
        if (Grid.CurrentRow == null)
        {
            MessageBox.Show("Выберите запись в таблице.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!ValidateInput())
            return;

        try
        {
            UpdateRecord();
            ReloadData();
            MessageBox.Show("Запись обновлена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void RemoveSelected()
    {
        if (Grid.CurrentRow == null)
        {
            MessageBox.Show("Выберите запись для удаления.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show(
            "Удалить выбранную запись?",
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        try
        {
            DeleteRecord();
            ReloadData();
        }
        catch (Exception ex)
        {
            ShowError(ex, "Не удалось удалить запись. Возможно, она используется в бронировании.");
        }
    }

    protected static void ShowError(Exception ex, string? prefix = null)
    {
        var message = string.IsNullOrWhiteSpace(prefix) ? ex.Message : $"{prefix}\n\n{ex.Message}";
        MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected int? GetSelectedId(string columnName)
    {
        if (Grid.CurrentRow?.Cells[columnName].Value is null or DBNull)
            return null;
        return Convert.ToInt32(Grid.CurrentRow.Cells[columnName].Value);
    }

    protected Label AddLabel(Panel panel, string text, int top)
    {
        var label = new Label
        {
            Text = text,
            Left = 12,
            Top = top,
            Width = 180,
            AutoSize = false
        };
        panel.Controls.Add(label);
        return label;
    }

    protected TextBox AddTextBox(Panel panel, int top, int width = 320)
    {
        var textBox = new TextBox
        {
            Left = 200,
            Top = top - 3,
            Width = width
        };
        panel.Controls.Add(textBox);
        return textBox;
    }

    protected ComboBox AddComboBox(Panel panel, int top, int width = 320)
    {
        var combo = new ComboBox
        {
            Left = 200,
            Top = top - 3,
            Width = width,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        panel.Controls.Add(combo);
        return combo;
    }

    protected DateTimePicker AddDatePicker(Panel panel, int top)
    {
        var picker = new DateTimePicker
        {
            Left = 200,
            Top = top - 3,
            Width = 200,
            Format = DateTimePickerFormat.Short
        };
        panel.Controls.Add(picker);
        return picker;
    }

    protected NumericUpDown AddNumeric(Panel panel, int top, decimal max = 1000000, int decimals = 2)
    {
        var numeric = new NumericUpDown
        {
            Left = 200,
            Top = top - 3,
            Width = 200,
            Maximum = max,
            DecimalPlaces = decimals,
            ThousandsSeparator = true
        };
        panel.Controls.Add(numeric);
        return numeric;
    }
}
