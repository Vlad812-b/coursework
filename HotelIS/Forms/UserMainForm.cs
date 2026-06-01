using HotelIS.Auth;
using HotelIS.UI;

namespace HotelIS.Forms;

internal sealed class UserMainForm : Form
{
    public bool LogoutRequested { get; private set; }

    public UserMainForm()
    {
        Text = "Сопровождение ИС — личный кабинет гостя";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(640, 400);
        ClientSize = new Size(700, 420);
        HotelTheme.ApplyForm(this);

        var title = new Label
        {
            Text = "Личный кабинет",
            Dock = DockStyle.Top,
            Height = 52,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = HotelTheme.AccentLight
        };

        var subtitle = new Label
        {
            Text = $"Добро пожаловать, {SessionUser.DisplayName}",
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = HotelTheme.TextMuted
        };

        var info = new Label
        {
            Text = "Просмотр номеров и оформление бронирования",
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = HotelTheme.TextMuted,
            Font = new Font("Segoe UI", 9.5F)
        };

        var buttonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(100, 20, 100, 20)
        };
        for (var i = 0; i < 4; i++)
            buttonsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        buttonsPanel.Controls.Add(MakeBtn("Каталог номеров", (_, _) => OpenRooms()), 0, 0);
        buttonsPanel.Controls.Add(MakeBtn("Забронировать номер", (_, _) => OpenBooking()), 0, 1);
        buttonsPanel.Controls.Add(MakeBtn("Мои бронирования", (_, _) => OpenBookingsView()), 0, 2);
        buttonsPanel.Controls.Add(MakeBtn("Сменить пользователя", (_, _) => Logout(), exit: true), 0, 3);

        Controls.Add(buttonsPanel);
        Controls.Add(info);
        Controls.Add(subtitle);
        Controls.Add(title);
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

    private static void OpenRooms()
    {
        try
        {
            using var form = new RoomsForm(readOnly: true);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void OpenBooking()
    {
        try
        {
            using var form = new BookingsForm(userMode: true, viewOnly: false);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void OpenBookingsView()
    {
        try
        {
            using var form = new BookingsForm(userMode: true, viewOnly: true);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Logout()
    {
        LogoutRequested = true;
        SessionUser.Clear();
        Close();
    }
}
