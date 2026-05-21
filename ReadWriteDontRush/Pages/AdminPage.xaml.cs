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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadComplaints();
        }

        private void LoadComplaints()
        {
            var complaints = Core.Context.Complaints
                .Where(c => c.Status == "На рассмотрении")
                .Select(c => new
                {
                    c.ComplaintID,
                    c.Reason,
                    c.CreatedAt,
                    BookTitle = c.Books != null ? c.Books.Title : "Неизвестная книга",
                    UserLogin = c.Users.Login,
                    TargetUser = c.TargetUserNavigation != null ? c.TargetUserNavigation.Login : null
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            AdminContentFrame.Navigate(new ComplaintsPage(complaints));
        }

        private void LoadUnfreezeRequests()
        {
            var requests = Core.Context.UnfreezeRequests
                .Where(r => r.Status == "На рассмотрении")
                .Select(r => new
                {
                    r.RequestID,
                    r.Reason,
                    r.CreatedAt,
                    UserLogin = r.Users.Login,
                    BookTitle = r.Books != null ? r.Books.Title : null
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            AdminContentFrame.Navigate(new UnfreezeRequestsPage(requests));
        }

        private void LoadAuthorRequests()
        {
            var requests = Core.Context.AuthorRequests
                .Where(r => r.Status == "На рассмотрении")
                .Select(r => new
                {
                    r.RequestID,
                    r.Message,
                    r.CreatedAt,
                    UserLogin = r.Users.Login,
                    UserDisplayName = r.Users.DisplayName
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            AdminContentFrame.Navigate(new AuthorRequestsPage(requests));
        }

        private void LoadFrozenItems()
        {
            var frozenBooks = Core.Context.Books.Where(b => b.IsFrozen == true)
                .Select(b => new { Type = "Книга", Name = b.Title, Reason = b.FreezeReason, Id = b.BookID }).ToList();
            var frozenUsers = Core.Context.Users.Where(u => u.IsFrozen == true)
                .Select(u => new { Type = "Пользователь", Name = u.DisplayName, Reason = u.FreezeReason, Id = u.UserID }).ToList();
            var frozenReviews = Core.Context.Reviews.Where(r => r.IsFrozen == true)
                .Select(r => new { Type = "Отзыв", Name = $"Отзыв к книге {r.Books.Title}", Reason = r.FreezeReason, Id = r.ReviewID }).ToList();

            var allFrozen = frozenBooks.Cast<dynamic>().Concat(frozenUsers).Concat(frozenReviews).ToList();
            AdminContentFrame.Navigate(new FrozenItemsPage(allFrozen));
        }

        private void LoadUsers()
        {
            var users = Core.Context.Users.ToList();
            AdminContentFrame.Navigate(new UsersManagementPage(users));
        }

        private void BtnComplaints_Click(object sender, RoutedEventArgs e) => LoadComplaints();
        private void BtnUnfreezeRequests_Click(object sender, RoutedEventArgs e) => LoadUnfreezeRequests();
        private void BtnAuthorRequests_Click(object sender, RoutedEventArgs e) => LoadAuthorRequests();
        private void BtnFrozenItems_Click(object sender, RoutedEventArgs e) => LoadFrozenItems();
        private void BtnUsers_Click(object sender, RoutedEventArgs e) => LoadUsers();
    }
}
