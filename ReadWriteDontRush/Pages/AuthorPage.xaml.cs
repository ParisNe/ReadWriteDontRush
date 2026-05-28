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
    /// Логика взаимодействия для AuthorPage.xaml
    /// </summary>
    public class AuthorBookViewModel
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string CoverImagePath { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public string GenresString { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFrozen { get; set; }
        public string ShowEditButton => IsFrozen ? "Collapsed" : "Visible";
        public string ShowAppealButton => IsFrozen ? "Visible" : "Collapsed";
        public string ShowAppealSentText => (IsFrozen && HasAppealSent) ? "Visible" : "Collapsed";
        public bool HasAppealSent { get; set; }
    }

    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();

            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

            if (Core.CurrentUser.RoleID != 2 && Core.CurrentUser.RoleID != 3)
            {
                MessageBox.Show("Доступ только для авторов", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

            LoadMyBooks();
        }

        private void LoadMyBooks()
        {
            try
            {
                var books = Core.Context.Books
                    .Where(b => b.AuthorID == Core.CurrentUser.UserID && b.IsFrozen == false)
                    .ToList();

                if (!books.Any())
                {
                    BooksItemsControl.Visibility = Visibility.Collapsed;
                    TxtEmptyMessage.Visibility = Visibility.Visible;
                    TxtEmptyMessage.Text = "У вас пока нет книг. Нажмите 'Добавить книгу' чтобы создать первую книгу.";
                    return;
                }

                BooksItemsControl.Visibility = Visibility.Visible;
                TxtEmptyMessage.Visibility = Visibility.Collapsed;

                var booksWithDetails = books.Select(b => new AuthorBookViewModel
                {
                    BookID = b.BookID,
                    Title = b.Title ?? "Без названия",
                    CoverImagePath = b.CoverImagePath,
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = b.Reviews.Count,
                    GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genres.GenreName)),
                    CreatedAt = b.CreatedAt,
                    IsFrozen = false,
                    HasAppealSent = false
                }).OrderByDescending(b => b.CreatedAt).ToList();

                BooksItemsControl.ItemsSource = booksWithDetails;

                BtnMyBooks.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                BtnFrozenBooks.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFrozenBooks()
        {
            try
            {
                var books = Core.Context.Books
                    .Where(b => b.AuthorID == Core.CurrentUser.UserID && b.IsFrozen == true)
                    .ToList();

                if (!books.Any())
                {
                    BooksItemsControl.Visibility = Visibility.Collapsed;
                    TxtEmptyMessage.Visibility = Visibility.Visible;
                    TxtEmptyMessage.Text = "У вас нет замороженных книг";
                    return;
                }

                BooksItemsControl.Visibility = Visibility.Visible;
                TxtEmptyMessage.Visibility = Visibility.Collapsed;

                var booksWithDetails = new List<AuthorBookViewModel>();
                foreach (var b in books)
                {
                    // Проверяем, подана ли уже заявка на разморозку
                    var existingRequest = Core.Context.UnfreezeRequests
                        .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID &&
                                            r.BookID == b.BookID &&
                                            r.IsProcessed == false);

                    booksWithDetails.Add(new AuthorBookViewModel
                    {
                        BookID = b.BookID,
                        Title = b.Title ?? "Без названия",
                        CoverImagePath = b.CoverImagePath,
                        AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                        ReviewsCount = b.Reviews.Count,
                        GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genres.GenreName)),
                        CreatedAt = b.CreatedAt,
                        IsFrozen = true,
                        HasAppealSent = existingRequest != null
                    });
                }

                BooksItemsControl.ItemsSource = booksWithDetails.OrderByDescending(b => b.CreatedAt).ToList();

                BtnMyBooks.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                BtnFrozenBooks.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0392B"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки замороженных книг: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMyBooks_Click(object sender, RoutedEventArgs e)
        {
            LoadMyBooks();
        }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddEditBookPage());
        }

        private void BtnFrozenBooks_Click(object sender, RoutedEventArgs e)
        {
            LoadFrozenBooks();
        }

        private void BtnEditBook_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var book = button?.Tag as AuthorBookViewModel;
            if (book != null)
            {
                NavigationService?.Navigate(new AddEditBookPage(book.BookID));
            }
        }

        private void BtnAppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int bookId = (int)button.Tag;

            var book = Core.Context.Books.Find(bookId);
            if (book == null) return;

            // Проверяем, не подавал ли автор уже заявку
            var existingRequest = Core.Context.UnfreezeRequests
                .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID &&
                                    r.BookID == bookId &&
                                    r.IsProcessed == false);

            if (existingRequest != null)
            {
                MessageBox.Show("Вы уже подавали заявку на разморозку этой книги. Ожидайте рассмотрения.",
                               "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var reasonWindow = new ReasonInputWindow("разморозку книги");
            reasonWindow.Title = $"Оспаривание заморозки книги \"{book.Title}\"";
            reasonWindow.Owner = Application.Current.MainWindow;

            if (reasonWindow.ShowDialog() == true)
            {
                try
                {
                    UnfreezeRequests request = new UnfreezeRequests
                    {
                        UserID = Core.CurrentUser.UserID,
                        BookID = bookId,
                        Reason = reasonWindow.Reason,
                        RequestDate = DateTime.Now,
                        IsProcessed = false,
                        IsApproved = null
                    };

                    Core.Context.UnfreezeRequests.Add(request);
                    Core.Context.SaveChanges();

                    MessageBox.Show("Заявка на разморозку книги отправлена администратору.",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем список замороженных книг
                    LoadFrozenBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отправки заявки: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
