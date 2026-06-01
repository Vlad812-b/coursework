using HotelIS.Auth;
using HotelIS.Data;
using HotelIS.Forms;

namespace HotelIS;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            DatabaseInitializer.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось создать или открыть базу данных.\n\n{ex.Message}",
                "Ошибка базы данных",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        RunWithAuthLoop();
    }

    private static void RunWithAuthLoop()
    {
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK || !SessionUser.IsAuthenticated)
                return;

            Form mainForm = SessionUser.IsAdmin
                ? new AdminMainForm()
                : new UserMainForm();

            Application.Run(mainForm);

            var logout = mainForm is AdminMainForm admin && admin.LogoutRequested
                || mainForm is UserMainForm user && user.LogoutRequested;

            if (!logout)
                return;
        }
    }
}
