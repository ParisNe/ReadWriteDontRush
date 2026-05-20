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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var user = Core.Context.Users.FirstOrDefault(x =>
                x.Username == LoginTb.Text &&
                x.PasswordHash == PasswordTb.Password);

            if (user != null)
            {
                App.CurrentUser = user;

                MessageBox.Show("Успешный вход");

                MainWindow main =
                    (MainWindow)Application.Current.MainWindow;

                main.OpenCatalog();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}
