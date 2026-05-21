using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReadWriteDontRush.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();

            LoadProfile();
        }

        private void LoadProfile()
        {
            var user = MainWindow.CurrentUser;

            UsernameText.Text =
                "Логин: " + user.Username;

            EmailText.Text =
                "Email: " + user.Email;

            var role = Core.Context.Roles
                .First(x => x.Id == user.RoleId);

            RoleText.Text =
                "Роль: " + role.Name;

            ReviewsList.ItemsSource =
                Core.Context.Reviews
                .Where(x => x.UserId == user.Id)
                .ToList();

            if (user.IsFrozen == true)
            {
                FreezeReasonText.Text =
                    "Причина заморозки: " +
                    user.FreezeReason;
            }
        }
    }
}
