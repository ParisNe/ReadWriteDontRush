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
    /// Логика взаимодействия для AuthorBooksPage.xaml
    /// </summary>
    public partial class AuthorBooksPage : Page
    {
        private System.Collections.Generic.List<Books> books;
        private bool showAppealButtons;

        public class AuthorBookViewModel
        {
            public int BookID { get; set; }
            public string Title { get; set; }
            public string CoverImagePath { get; set; }
            public double AverageRating { get; set; }
            public string GenresString { get; set; }
            public bool IsFrozen { get; set; }
            public Visibility ShowAppealButton { get; set; }
        }

        public AuthorBooksPage(System.Collections.Generic.List<Books> booksList, bool showAppeal)
        {
            InitializeComponent();
            books = booksList;
            showAppealButtons = showAppeal;
            TxtTitle.Text = showAppeal ? "❄️ Замороженные книги" : "📚 Мои книги";
            LoadBooks();
        }

        private void LoadBooks()
        {
            string searchText = TxtSearch?.Text.Trim().ToLower() ?? "";

            var query = books.AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                // Исправлено: убираем оператор ?. и используем обычное сравнение
                query = query.Where(b => b.Title != null && b.Title.ToLower().Contains(searchText));
            }

            var booksWithDetails = query.Select(b => new AuthorBookViewModel
            {
                BookID = b.BookID,
                Title = b.Title ?? "Без названия",
                CoverImagePath = b.CoverImagePath,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genres.GenreName)),
                IsFrozen = b.IsFrozen ?? false,
                ShowAppealButton = showAppealButtons ? Visibility.Visible : Visibility.Collapsed
            }).ToList();

            BooksItemsControl.ItemsSource = booksWithDetails;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadBooks();

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bookVM = button?.Tag as AuthorBookViewModel;
            if (bookVM != null)
            {
                var book = books.FirstOrDefault(b => b.BookID == bookVM.BookID);
                NavigationService?.Navigate(new AddEditBookPage(book));
            }
        }

        private void BtnAppeal_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bookVM = button?.Tag as AuthorBookViewModel;
            if (bookVM != null)
            {
                var book = books.FirstOrDefault(b => b.BookID == bookVM.BookID);

                var window = new Window
                {
                    Title = "Оспаривание заморозки книги",
                    Width = 450,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Книга: {book.Title}\n\nПричина заморозки: {book.FreezeReason ?? "Не указана"}\n\nУкажите причину для разморозки:",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap
                });

                var textBox = new TextBox
                {
                    Height = 100,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                stackPanel.Children.Add(textBox);

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 20, 0, 0)
                };

                var okBtn = new Button { Content = "Отправить", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
                okBtn.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        MessageBox.Show("Укажите причину", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    UnfreezeRequests request = new UnfreezeRequests
                    {
                        UserID = Core.CurrentUser.UserID,
                        BookID = book.BookID,
                        Reason = textBox.Text,
                        Status = "На рассмотрении",
                        CreatedAt = DateTime.Now
                    };

                    Core.Context.UnfreezeRequests.Add(request);
                    Core.Context.SaveChanges();

                    MessageBox.Show("Заявка на разморозку отправлена администратору", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    window.Close();
                };

                var cancelBtn = new Button { Content = "Отмена", Width = 80, Height = 30 };
                cancelBtn.Click += (s, args) => window.Close();

                buttonPanel.Children.Add(okBtn);
                buttonPanel.Children.Add(cancelBtn);
                stackPanel.Children.Add(buttonPanel);

                window.Content = stackPanel;
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
    }
}
