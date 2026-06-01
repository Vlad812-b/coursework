using HotelIS.Auth;
using HotelIS.Data;
using HotelIS.UI;

namespace HotelIS.Forms;

internal sealed class AdminMainForm : Form
{
    public bool LogoutRequested { get; private set; }

    public AdminMainForm()
    {
        Text = "Сопровождение ИС — панель администратора";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 420);
        ClientSize = new Size(760, 440);
        HotelTheme.ApplyForm(this);

        var title = new Label
        {
            Text = "Гостиничный комплекс",
            Dock = DockStyle.Top,
            Height = 56,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = HotelTheme.Accent
        };

        var subtitle = new Label
        {
            Text = $"Администратор · {SessionUser.DisplayName} ({SessionUser.Login})",
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = HotelTheme.TextMuted
        };

        var info = new Label
        {
            Text = "Полный доступ: клиенты, номера, бронирование",
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = HotelTheme.TextMuted,
            Font = new Font("Segoe UI", 9.5F)
        };

        var buttonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(80, 16, 80, 16)
        };
        for (var i = 0; i < 4; i++)
            buttonsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        buttonsPanel.Controls.Add(MakeBtn("Клиенты", (_, _) => Open<ClientsForm>()), 0, 0);
        buttonsPanel.Controls.Add(MakeBtn("Номера", (_, _) => Open<RoomsForm>()), 0, 1);
        buttonsPanel.Controls.Add(MakeBtn("Бронирование", (_, _) => Open<BookingsForm>()), 0, 2);
        buttonsPanel.Controls.Add(MakeBtn("Сменить пользователя", (_, _) => Logout(), exit: true), 0, 3);

        var status = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 16, 0),
            ForeColor = HotelTheme.TextMuted,
            Text = $"База данных: {DatabaseHelper.DatabasePath}"
        };

        Controls.Add(buttonsPanel);
        Controls.Add(info);
        Controls.Add(subtitle);
        Controls.Add(title);
        Controls.Add(status);
    }

    private static Button MakeBtn(string text, EventHandler click, bool exit = false)
    {
        var btn = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(8)
        };
        HotelTheme.StyleMenuButton(btn, exit);
        btn.Click += click;
        return btn;
    }

    private void Open<T>() where T : Form, new()
    {
        try
        {
            using var form = new T();
            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при открытии формы:\n{ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Logout()
    {
        LogoutRequested = true;
        SessionUser.Clear();
        Close();
    }
}
