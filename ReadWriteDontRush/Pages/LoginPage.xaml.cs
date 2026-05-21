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
using static System.Collections.Specialized.BitVector32;

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

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var user = Core.Context.Users.FirstOrDefault(x =>
                x.Username == UsernameBox.Text &&
                x.PasswordHash == PasswordBox.Password);

            if (user == null)
            {
                ErrorText.Text = "Неверный логин или пароль";
                return;
            }

            Session.CurrentUser = user;

            var window = (MainWindow)Application.Current.MainWindow;
            window.UpdateSidebar();

            window.MainFrame.Navigate(new CatalogPage());
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}
