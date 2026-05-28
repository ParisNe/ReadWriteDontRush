using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private List<FrozenBookDisplay> allFrozenBooks;

        public AdminPage()
        {
            InitializeComponent();

            if (Core.CurrentUser == null || Core.CurrentUser.RoleID != 3)
            {
                MessageBox.Show("Доступ запрещен. Требуются права администратора.", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

            allFrozenBooks = new List<FrozenBookDisplay>(); // Инициализация списка

            LoadComplaints();
            LoadAuthorRequests();
            LoadUnfreezeRequests();
            LoadFrozenBooks();
            LoadUsers();
        }

        private void LoadComplaints()
        {
            try
            {
                var complaints = Core.Context.Complaints.ToList();

                if (!complaints.Any())
                {
                    ComplaintsItemsControl.Visibility = Visibility.Collapsed;
                    TxtNoComplaints.Visibility = Visibility.Visible;
                    return;
                }

                ComplaintsItemsControl.Visibility = Visibility.Visible;
                TxtNoComplaints.Visibility = Visibility.Collapsed;

                var complaintList = new List<ComplaintDisplay>();
                foreach (var c in complaints)
                {
                    complaintList.Add(new ComplaintDisplay
                    {
                        ComplaintID = c.ComplaintID,
                        UserID = c.UserID,
                        BookID = c.BookID,
                        ReviewID = c.ReviewID,
                        Reason = c.Reason,
                        CreatedAt = c.CreatedAt,
                        UserDisplayName = c.Users?.DisplayName ?? "Неизвестный",
                        ComplaintType = c.BookID != null ? "Жалоба на книгу" : "Жалоба на отзыв"
                    });
                }

                ComplaintsItemsControl.ItemsSource = complaintList.OrderByDescending(c => c.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жалоб: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAuthorRequests()
        {
            try
            {
                var requests = Core.Context.AuthorRoleRequests
                    .Where(r => r.IsProcessed == false)
                    .ToList();

                if (!requests.Any())
                {
                    AuthorRequestsItemsControl.Visibility = Visibility.Collapsed;
                    TxtNoAuthorRequests.Visibility = Visibility.Visible;
                    return;
                }

                AuthorRequestsItemsControl.Visibility = Visibility.Visible;
                TxtNoAuthorRequests.Visibility = Visibility.Collapsed;

                var requestList = new List<AuthorRequestDisplay>();
                foreach (var r in requests)
                {
                    string currentRoleName = "";
                    if (r.Users.RoleID == 1) currentRoleName = "Пользователь";
                    else if (r.Users.RoleID == 2) currentRoleName = "Автор";
                    else if (r.Users.RoleID == 3) currentRoleName = "Администратор";

                    requestList.Add(new AuthorRequestDisplay
                    {
                        RequestID = r.RequestID,
                        UserID = r.UserID,
                        RequestDate = r.RequestDate,
                        UserDisplayName = r.Users?.DisplayName ?? "Неизвестный",
                        CurrentRoleName = currentRoleName
                    });
                }

                AuthorRequestsItemsControl.ItemsSource = requestList.OrderByDescending(r => r.RequestDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUnfreezeRequests()
        {
            try
            {
                var requests = Core.Context.UnfreezeRequests
                    .Where(r => r.IsProcessed == false)
                    .ToList();

                if (!requests.Any())
                {
                    UnfreezeRequestsItemsControl.Visibility = Visibility.Collapsed;
                    TxtNoUnfreezeRequests.Visibility = Visibility.Visible;
                    return;
                }

                UnfreezeRequestsItemsControl.Visibility = Visibility.Visible;
                TxtNoUnfreezeRequests.Visibility = Visibility.Collapsed;

                var requestList = new List<UnfreezeRequestDisplay>();
                foreach (var r in requests)
                {
                    requestList.Add(new UnfreezeRequestDisplay
                    {
                        UnfreezeRequestID = r.UnfreezeRequestID,
                        UserID = r.UserID,
                        BookID = r.BookID,
                        Reason = r.Reason,
                        RequestDate = r.RequestDate,
                        UserDisplayName = r.Users?.DisplayName ?? "Неизвестный",
                        RequestType = r.BookID != null ? "Разморозка книги" : "Разморозка аккаунта"
                    });
                }

                UnfreezeRequestsItemsControl.ItemsSource = requestList.OrderByDescending(r => r.RequestDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFrozenBooks()
        {
            try
            {
                var frozenBooks = Core.Context.Books
                    .Where(b => b.IsFrozen == true)
                    .ToList();

                if (!frozenBooks.Any())
                {
                    FrozenBooksItemsControl.Visibility = Visibility.Collapsed;
                    TxtNoFrozenBooks.Visibility = Visibility.Visible;
                    allFrozenBooks = new List<FrozenBookDisplay>();
                    return;
                }

                FrozenBooksItemsControl.Visibility = Visibility.Visible;
                TxtNoFrozenBooks.Visibility = Visibility.Collapsed;

                allFrozenBooks = new List<FrozenBookDisplay>();
                foreach (var b in frozenBooks)
                {
                    allFrozenBooks.Add(new FrozenBookDisplay
                    {
                        BookID = b.BookID,
                        Title = b.Title ?? "Без названия",
                        AuthorName = b.Users?.DisplayName ?? "Неизвестный автор",
                        GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genres.GenreName)),
                        FrozenDate = b.CreatedAt
                    });
                }

                FilterFrozenBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки замороженных книг: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                allFrozenBooks = new List<FrozenBookDisplay>();
            }
        }

        private void FilterFrozenBooks()
        {
            try
            {
                if (allFrozenBooks == null)
                {
                    allFrozenBooks = new List<FrozenBookDisplay>();
                }

                string searchText = TxtFrozenBookSearch?.Text.Trim().ToLower() ?? "";

                if (string.IsNullOrEmpty(searchText))
                {
                    FrozenBooksItemsControl.ItemsSource = allFrozenBooks.OrderBy(b => b.Title).ToList();
                }
                else
                {
                    var filtered = allFrozenBooks
                        .Where(b => (b.Title?.ToLower().Contains(searchText) ?? false) ||
                                   (b.AuthorName?.ToLower().Contains(searchText) ?? false) ||
                                   (b.GenresString?.ToLower().Contains(searchText) ?? false))
                        .OrderBy(b => b.Title)
                        .ToList();

                    FrozenBooksItemsControl.ItemsSource = filtered;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                FrozenBooksItemsControl.ItemsSource = new List<FrozenBookDisplay>();
            }
        }

        private void LoadUsers()
        {
            try
            {
                var users = Core.Context.Users.ToList();
                string searchText = TxtUserSearch?.Text.Trim().ToLower() ?? "";

                if (!string.IsNullOrEmpty(searchText))
                {
                    users = users.Where(u => u.Login.ToLower().Contains(searchText) ||
                                            u.DisplayName.ToLower().Contains(searchText) ||
                                            u.Email.ToLower().Contains(searchText)).ToList();
                }

                var userList = new List<UserDisplayInfo>();
                foreach (var u in users)
                {
                    string roleName = "";
                    if (u.RoleID == 1) roleName = "Пользователь";
                    else if (u.RoleID == 2) roleName = "Автор";
                    else if (u.RoleID == 3) roleName = "Администратор";
                    else roleName = "Неизвестно";

                    userList.Add(new UserDisplayInfo
                    {
                        UserID = u.UserID,
                        Login = u.Login,
                        DisplayName = u.DisplayName,
                        Email = u.Email,
                        IsFrozen = u.IsFrozen,
                        RoleName = roleName,
                        RoleID = u.RoleID,
                        Status = u.IsFrozen ? "Заморожен" : "Активен",
                        StatusColor = u.IsFrozen ? Brushes.Red : Brushes.Green
                    });
                }

                UsersItemsControl.ItemsSource = userList.OrderBy(u => u.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int complaintId = (int)button.Tag;

            var result = MessageBox.Show("Принять жалобу? Это действие заморозит книгу/отзыв.",
                                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var complaint = Core.Context.Complaints.Find(complaintId);
                    if (complaint != null)
                    {
                        if (complaint.BookID != null)
                        {
                            var book = Core.Context.Books.Find(complaint.BookID);
                            if (book != null)
                                book.IsFrozen = true;
                        }
                        else if (complaint.ReviewID != null)
                        {
                            var review = Core.Context.Reviews.Find(complaint.ReviewID);
                            if (review != null)
                                Core.Context.Reviews.Remove(review);
                        }

                        Core.Context.Complaints.Remove(complaint);
                        Core.Context.SaveChanges();

                        MessageBox.Show("Жалоба принята", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadComplaints();
                        LoadFrozenBooks();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int complaintId = (int)button.Tag;

            var result = MessageBox.Show("Отклонить жалобу?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var complaint = Core.Context.Complaints.Find(complaintId);
                    if (complaint != null)
                    {
                        Core.Context.Complaints.Remove(complaint);
                        Core.Context.SaveChanges();

                        MessageBox.Show("Жалоба отклонена", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadComplaints();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AcceptAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var result = MessageBox.Show("Одобрить заявку на смену роли?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var request = Core.Context.AuthorRoleRequests.Find(requestId);
                    if (request != null)
                    {
                        var user = Core.Context.Users.Find(request.UserID);
                        if (user != null)
                        {
                            if (user.RoleID == 1)
                                user.RoleID = 2;
                            else if (user.RoleID == 2)
                                user.RoleID = 3;
                        }

                        request.IsProcessed = true;
                        request.IsApproved = true;
                        Core.Context.SaveChanges();

                        MessageBox.Show("Заявка одобрена", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadAuthorRequests();
                        LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RejectAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var result = MessageBox.Show("Отклонить заявку на смену роли?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var request = Core.Context.AuthorRoleRequests.Find(requestId);
                    if (request != null)
                    {
                        request.IsProcessed = true;
                        request.IsApproved = false;
                        Core.Context.SaveChanges();

                        MessageBox.Show("Заявка отклонена", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadAuthorRequests();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AcceptUnfreezeRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var result = MessageBox.Show("Разморозить?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var request = Core.Context.UnfreezeRequests.Find(requestId);
                    if (request != null)
                    {
                        if (request.BookID != null)
                        {
                            var book = Core.Context.Books.Find(request.BookID);
                            if (book != null)
                                book.IsFrozen = false;
                        }
                        else
                        {
                            var user = Core.Context.Users.Find(request.UserID);
                            if (user != null)
                                user.IsFrozen = false;
                        }

                        request.IsProcessed = true;
                        request.IsApproved = true;
                        Core.Context.SaveChanges();

                        MessageBox.Show("Заявка одобрена", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUnfreezeRequests();
                        LoadFrozenBooks();
                        LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RejectUnfreezeRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var result = MessageBox.Show("Отклонить заявку?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var request = Core.Context.UnfreezeRequests.Find(requestId);
                    if (request != null)
                    {
                        request.IsProcessed = true;
                        request.IsApproved = false;
                        Core.Context.SaveChanges();

                        MessageBox.Show("Заявка отклонена", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUnfreezeRequests();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int bookId = (int)button.Tag;

            var result = MessageBox.Show("Разморозить эту книгу?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var book = Core.Context.Books.Find(bookId);
                    if (book != null)
                    {
                        book.IsFrozen = false;
                        Core.Context.SaveChanges();

                        MessageBox.Show("Книга разморожена", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadFrozenBooks();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TxtFrozenBookSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allFrozenBooks != null)
            {
                FilterFrozenBooks();
            }
        }

        private void TxtUserSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadUsers();
        }

        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;

            var userItem = UsersItemsControl.ItemsSource as IEnumerable<UserDisplayInfo>;
            var user = userItem?.FirstOrDefault(u => u.UserID == userId);

            if (user != null)
            {
                if (user.RoleID == 3)
                {
                    MessageBox.Show("Нельзя изменить роль администратора", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int newRoleId = (user.RoleID == 1) ? 2 : 1;
                string newRoleName = (newRoleId == 2) ? "Автора" : "Пользователя";

                var result = MessageBox.Show($"Сменить роль пользователя {user.DisplayName} на {newRoleName}?",
                                            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var dbUser = Core.Context.Users.Find(userId);
                        if (dbUser != null && dbUser.RoleID != 3)
                        {
                            dbUser.RoleID = newRoleId;
                            Core.Context.SaveChanges();

                            MessageBox.Show("Роль изменена", "Успех",
                                           MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadUsers();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void FreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;

            var result = MessageBox.Show("Заморозить аккаунт пользователя?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var user = Core.Context.Users.Find(userId);
                    if (user != null && user.RoleID != 3)
                    {
                        user.IsFrozen = true;
                        Core.Context.SaveChanges();

                        MessageBox.Show("Пользователь заморожен", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("Нельзя заморозить администратора", "Ошибка",
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;

            var result = MessageBox.Show("Разморозить аккаунт пользователя?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var user = Core.Context.Users.Find(userId);
                    if (user != null)
                    {
                        user.IsFrozen = false;
                        Core.Context.SaveChanges();

                        MessageBox.Show("Пользователь разморожен", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;

            var result = MessageBox.Show("Сбросить пароль пользователя?\nНовый пароль будет '123456'",
                                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var user = Core.Context.Users.Find(userId);
                    if (user != null)
                    {
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("123456"));
                            user.PasswordHash = Convert.ToBase64String(hashedBytes);
                        }
                        Core.Context.SaveChanges();

                        MessageBox.Show("Пароль сброшен на '123456'", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class ComplaintDisplay
    {
        public int ComplaintID { get; set; }
        public int UserID { get; set; }
        public int? BookID { get; set; }
        public int? ReviewID { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserDisplayName { get; set; }
        public string ComplaintType { get; set; }
    }

    public class AuthorRequestDisplay
    {
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public DateTime RequestDate { get; set; }
        public string UserDisplayName { get; set; }
        public string CurrentRoleName { get; set; }
    }

    public class UnfreezeRequestDisplay
    {
        public int UnfreezeRequestID { get; set; }
        public int UserID { get; set; }
        public int? BookID { get; set; }
        public string Reason { get; set; }
        public DateTime RequestDate { get; set; }
        public string UserDisplayName { get; set; }
        public string RequestType { get; set; }
    }

    public class FrozenBookDisplay
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string GenresString { get; set; }
        public DateTime FrozenDate { get; set; }
    }

    public class UserDisplayInfo
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsFrozen { get; set; }
        public string RoleName { get; set; }
        public int RoleID { get; set; }
        public string Status { get; set; }
        public Brush StatusColor { get; set; }
    }
}
