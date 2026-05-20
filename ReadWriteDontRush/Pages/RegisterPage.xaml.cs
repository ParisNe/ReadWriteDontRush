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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            Users user = new Users()
            {
                UserName = TbName.Text,
                Login = TbLogin.Text,
                Email = TbEmail.Text,
                PasswordHash = PbPassword.Password,
                RoleID = 1,
                IsFrozen = false
            };

            Core.Context.Users.Add(user);

            Core.Context.SaveChanges();

            MessageBox.Show("Аккаунт создан");

            NavigationService.Navigate(new LoginPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}
