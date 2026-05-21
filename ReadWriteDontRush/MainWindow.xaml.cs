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
        public MainWindow()
        {
            InitializeComponent();
            SetupSidebar();
            MainFrame.Navigate(new LoginPage());
        }

        private void SetupSidebar()
        {
            if (Core.CurrentUser == null)
            {
                SideBarMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                SideBarMenu.Visibility = Visibility.Visible;

                // Администрирование (RoleID = 1)
                BtnAdmin.Visibility = Core.CurrentUser.RoleID == 1 ? Visibility.Visible : Visibility.Collapsed;

                // Страница автора (RoleID = 3)
                BtnAuthor.Visibility = Core.CurrentUser.RoleID == 3 ? Visibility.Visible : Visibility.Collapsed;

                // Предупреждение о заморозке - исправлено с учетом nullable типа
                bool isFrozen = Core.CurrentUser.IsFrozen.HasValue && Core.CurrentUser.IsFrozen.Value;
                BtnWarning.Visibility = isFrozen ? Visibility.Visible : Visibility.Collapsed;
            }
        }



        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }

        private void BtnBookLists_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BookListsPage());
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser != null && Core.CurrentUser.RoleID == 1)
                MainFrame.Navigate(new AdminPage());
            else
                MessageBox.Show("У вас нет прав доступа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser != null && Core.CurrentUser.RoleID == 3)
                MainFrame.Navigate(new AuthorPage());
            else
                MessageBox.Show("У вас нет прав доступа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnWarning_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser != null)
                MainFrame.Navigate(new UserProfilePage());
            else
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser != null)
                MainFrame.Navigate(new UserProfilePage());
            else
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SetupSidebar();
        }

    }
}
