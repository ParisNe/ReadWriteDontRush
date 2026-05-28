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


            // Настройка Sidebar в зависимости от роли пользователя
            SetupSidebar();


            MainFrame.Navigate(new LoginPage());
        }

        private void SetupSidebar()
        {
            if (Core.CurrentUser == null) SideBarMenu.Visibility = Visibility.Collapsed;

            else
            {
                SideBarMenu.Visibility = Visibility.Visible;

                // Администрирование
                if (Core.CurrentUser.RoleID == 3) // ID роли администратора
                {
                    BtnAdmin.Visibility = Visibility.Visible;
                }

                // Страница автора
                if (Core.CurrentUser.RoleID == 2) // ID роли автора
                {
                    BtnAuthor.Visibility = Visibility.Visible;
                }

                // Предупреждение о заморозке
                if (Core.CurrentUser.IsFrozen == true)
                {
                    BtnWarning.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }

        private void BtnBookLists_Click(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new ReadingListsPage());
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminPage());
        }

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AuthorPage());
        }

        private void BtnWarning_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new UnfreezeRequestPage());
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SetupSidebar();
        }
    }
}
