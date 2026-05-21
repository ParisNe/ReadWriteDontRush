using ReadWriteDontRush.Pages;
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

namespace ReadWriteDontRush
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Users CurrentUser;

        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(new LoginPage());

            AdminBtn.Visibility = Visibility.Collapsed;
            AuthorBtn.Visibility = Visibility.Collapsed;
            FreezeBtn.Visibility = Visibility.Collapsed;
        }

        public void UpdateSidebar()
        {
            if (CurrentUser == null)
                return;

            var role = Core.Context.Roles
                .First(x => x.Id == CurrentUser.RoleId);

            AdminBtn.Visibility = Visibility.Collapsed;
            AuthorBtn.Visibility = Visibility.Collapsed;
            FreezeBtn.Visibility = Visibility.Collapsed;

            if (role.Name == "ADMIN")
            {
                AdminBtn.Visibility = Visibility.Visible;
            }

            if (role.Name == "AUTHOR")
            {
                AuthorBtn.Visibility = Visibility.Visible;
            }

            if (CurrentUser.IsFrozen == true)
            {
                FreezeBtn.Visibility = Visibility.Visible;
            }
        }
    }
}
