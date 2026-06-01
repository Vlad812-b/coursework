namespace HotelIS.UI;

internal static class HotelTheme
{
    public static readonly Color Background = Color.FromArgb(245, 247, 250);
    public static readonly Color Panel = Color.White;
    public static readonly Color Accent = Color.FromArgb(25, 55, 95);
    public static readonly Color AccentLight = Color.FromArgb(45, 85, 140);
    public static readonly Color TextMuted = Color.FromArgb(70, 90, 120);
    public static readonly Color Border = Color.FromArgb(180, 195, 215);

    public static void ApplyForm(Form form)
    {
        form.BackColor = Background;
        form.Font = new Font("Segoe UI", 10F);
    }

    public static void StyleMenuButton(Button button, bool isExit = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Border;
        button.BackColor = isExit ? Color.FromArgb(252, 240, 240) : Panel;
        button.ForeColor = isExit ? Color.FromArgb(140, 50, 50) : Accent;
        button.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    public static void StylePrimaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Accent;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Height = 36;
    }

    public static void StyleTabButton(Button button, bool active)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = active ? Accent : Panel;
        button.ForeColor = active ? Color.White : Accent;
        button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    public static void StyleInput(Control control)
    {
        control.Font = new Font("Segoe UI", 10F);
        if (control is TextBox or ComboBox)
        {
            control.BackColor = Panel;
            control.ForeColor = Color.Black;
        }
    }
}
