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

namespace ReadWriteDontRush
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());

            AuthorBtn.Visibility = Visibility.Collapsed;
            AdminBtn.Visibility = Visibility.Collapsed;
        }

        public void OpenCatalog()
        {
            MainFrame.Navigate(new CatalogPage());

            if (App.CurrentUser.Roles.Name == "AUTHOR")
            {
                AuthorBtn.Visibility = Visibility.Visible;
            }

            if (App.CurrentUser.Roles.Name == "ADMIN")
            {
                AdminBtn.Visibility = Visibility.Visible;
            }
        }

        private void CatalogBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }

        private void AuthorBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AuthorPage());
        }

        private void AdminBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminPage());
        }
    }
}
