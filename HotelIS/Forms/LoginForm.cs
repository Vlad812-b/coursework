using HotelIS.Auth;
using HotelIS.Data;
using HotelIS.UI;

namespace HotelIS.Forms;

internal sealed class LoginForm : Form
{
    private readonly Panel _loginPanel;
    private readonly Panel _registerPanel;
    private readonly TextBox _loginUser;
    private readonly TextBox _loginPassword;
    private readonly TextBox _regLogin;
    private readonly TextBox _regPassword;
    private readonly TextBox _regConfirm;
    private readonly TextBox _regName;
    private readonly ComboBox _regRole;
    private readonly Button _btnLoginTab;
    private readonly Button _btnRegTab;

    public LoginForm()
    {
        Text = "Сопровождение ИС — гостиничный бизнес";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 460);
        HotelTheme.ApplyForm(this);

        var header = new Label
        {
            Text = "Гостиничный комплекс",
            Dock = DockStyle.Top,
            Height = 56,
            TextAlign = ContentAlignment.BottomCenter,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = HotelTheme.Accent
        };

        var subtitle = new Label
        {
            Text = "Сопровождение информационной системы",
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = HotelTheme.TextMuted
        };

        var tabs = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(40, 8, 40, 0)
        };

        _btnLoginTab = MakeTabButton("Вход", true);
        _btnRegTab = MakeTabButton("Регистрация", false);
        _btnLoginTab.Left = 40;
        _btnRegTab.Left = 240;
        tabs.Controls.Add(_btnLoginTab);
        tabs.Controls.Add(_btnRegTab);

        _loginPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40, 12, 40, 12) };
        _registerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40, 8, 40, 8), Visible = false };

        _loginUser = AddField(_loginPanel, "Логин:", 16);
        _loginPassword = AddField(_loginPanel, "Пароль:", 64, password: true);

        var loginBtn = MakePrimaryButton("Войти");
        loginBtn.Top = 120;
        loginBtn.Click += (_, _) => DoLogin();
        _loginPanel.Controls.Add(loginBtn);

        var hint = new Label
        {
            Text = "Администратор: admin / admin\r\nПользователь: user / user",
            Left = 40,
            Top = 172,
            Width = 400,
            Height = 40,
            ForeColor = HotelTheme.TextMuted,
            Font = new Font("Segoe UI", 9F)
        };
        _loginPanel.Controls.Add(hint);

        _regLogin = AddField(_registerPanel, "Логин:", 8);
        _regName = AddField(_registerPanel, "Имя:", 48);
        _regPassword = AddField(_registerPanel, "Пароль:", 88, password: true);
        _regConfirm = AddField(_registerPanel, "Повтор пароля:", 128, password: true);

        AddLabel(_registerPanel, "Роль:", 168);
        _regRole = new ComboBox
        {
            Left = 150,
            Top = 165,
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _regRole.Items.AddRange(["Пользователь", "Администратор"]);
        _regRole.SelectedIndex = 0;
        HotelTheme.StyleInput(_regRole);
        _registerPanel.Controls.Add(_regRole);

        var regBtn = MakePrimaryButton("Зарегистрироваться");
        regBtn.Top = 210;
        regBtn.Click += (_, _) => DoRegister();
        _registerPanel.Controls.Add(regBtn);

        AcceptButton = loginBtn;

        _btnLoginTab.Click += (_, _) => ShowLoginMode();
        _btnRegTab.Click += (_, _) => ShowRegisterMode();

        var host = new Panel { Dock = DockStyle.Fill };
        host.Controls.Add(_registerPanel);
        host.Controls.Add(_loginPanel);

        Controls.Add(host);
        Controls.Add(tabs);
        Controls.Add(subtitle);
        Controls.Add(header);

        _loginPassword.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
                DoLogin();
        };
    }

    private void ShowLoginMode()
    {
        _loginPanel.Visible = true;
        _registerPanel.Visible = false;
        HotelTheme.StyleTabButton(_btnLoginTab, true);
        HotelTheme.StyleTabButton(_btnRegTab, false);
    }

    private void ShowRegisterMode()
    {
        _loginPanel.Visible = false;
        _registerPanel.Visible = true;
        HotelTheme.StyleTabButton(_btnLoginTab, false);
        HotelTheme.StyleTabButton(_btnRegTab, true);
    }

    private void DoLogin()
    {
        if (!UserAuthService.TryLogin(_loginUser.Text, _loginPassword.Text, out var error))
        {
            MessageBox.Show(error, "Вход", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void DoRegister()
    {
        var role = _regRole.SelectedIndex == 1 ? UserRole.Admin : UserRole.User;
        if (!UserAuthService.TryRegister(
                _regLogin.Text,
                _regPassword.Text,
                _regConfirm.Text,
                _regName.Text,
                role,
                out var error))
        {
            MessageBox.Show(error, "Регистрация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(
            "Регистрация выполнена. Войдите под созданным логином.",
            "Готово",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        _loginUser.Text = _regLogin.Text.Trim();
        _loginPassword.Clear();
        ShowLoginMode();
    }

    private TextBox AddField(Panel panel, string label, int top, bool password = false)
    {
        AddLabel(panel, label, top);
        var box = new TextBox
        {
            Left = 150,
            Top = top - 3,
            Width = 250,
            UseSystemPasswordChar = password
        };
        HotelTheme.StyleInput(box);
        panel.Controls.Add(box);
        return box;
    }

    private static void AddLabel(Panel panel, string text, int top)
    {
        panel.Controls.Add(new Label
        {
            Text = text,
            Left = 0,
            Top = top,
            Width = 140,
            ForeColor = HotelTheme.TextMuted
        });
    }

    private static Button MakeTabButton(string text, bool active)
    {
        var btn = new Button
        {
            Text = text,
            Width = 180,
            Height = 32,
            FlatStyle = FlatStyle.Flat
        };
        HotelTheme.StyleTabButton(btn, active);
        return btn;
    }

    private Button MakePrimaryButton(string text)
    {
        var btn = new Button
        {
            Text = text,
            Left = 150,
            Width = 250
        };
        HotelTheme.StylePrimaryButton(btn);
        return btn;
    }
}
