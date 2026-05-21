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
    /// Логика взаимодействия для AuthorRequestsPage.xaml
    /// </summary>
    public partial class AuthorRequestsPage : Page
    {
        private dynamic requestsData;

        public AuthorRequestsPage(dynamic requests)
        {
            InitializeComponent();
            requestsData = requests;
            LoadRequests();
        }

        private void LoadRequests()
        {
            if (requestsData == null || requestsData.Count == 0)
            {
                TxtNoItems.Visibility = Visibility.Visible;
                RequestsItemsControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoItems.Visibility = Visibility.Collapsed;
                RequestsItemsControl.Visibility = Visibility.Visible;
                RequestsItemsControl.ItemsSource = requestsData;
            }
        }

        private void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var request = Core.Context.AuthorRequests.Find(requestId);
            if (request != null)
            {
                request.Status = "Принята";

                // Меняем роль пользователя на автора (RoleID = 3)
                var user = Core.Context.Users.Find(request.UserID);
                if (user != null)
                {
                    user.RoleID = 3;
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Заявка принята, пользователь получил роль автора", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Refresh();
            }
        }

        private void RejectRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var request = Core.Context.AuthorRequests.Find(requestId);
            if (request != null)
            {
                request.Status = "Отклонена";
                Core.Context.SaveChanges();
                MessageBox.Show("Заявка отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Refresh();
            }
        }
    }
}
