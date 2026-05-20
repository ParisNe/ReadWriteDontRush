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

            LoadUser();
        }

        private void LoadUser()
        {
            LoginTb.Text =
                App.CurrentUser.Username;

            EmailTb.Text =
                App.CurrentUser.Email;

            NameTb.Text =
                App.CurrentUser.DisplayName;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser.Username = LoginTb.Text;
            App.CurrentUser.Email = EmailTb.Text;
            App.CurrentUser.DisplayName = NameTb.Text;

            Core.Context.SaveChanges();

            MessageBox.Show("Профиль обновлен");
        }
    }
}
