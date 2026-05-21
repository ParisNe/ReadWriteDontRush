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

namespace ReadWriteDontRush.Pages
{
    /// <summary>
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
        private int _bookId;

        public BookPage(int bookId)
        {
            InitializeComponent();

            _bookId = bookId;

            LoadBook();
        }

        private void LoadBook()
        {
            var book = Core.Context.Books
                .First(x => x.Id == _bookId);

            TitleText.Text = book.Title;

            var author = Core.Context.Users
                .First(x => x.Id == book.AuthorId);

            AuthorText.Text =
                "Автор: " + author.DisplayName;

            var genres = Core.Context.BookGenres
                .Where(x => x.BookId == book.Id)
                .Select(x => x.Genre.Name)
                .ToList();

            GenresText.Text =
                string.Join(", ", genres);

            DescriptionText.Text = book.Description;

            ContentText.Text = book.TextContent;

            try
            {
                CoverImage.Source =
                    new BitmapImage(new Uri(book.CoverUrl));
            }
            catch { }

            ReviewsList.ItemsSource =
                Core.Context.Reviews
                .Where(x => x.BookId == _bookId)
                .ToList();
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser == null)
            {
                MessageBox.Show("Авторизуйтесь");
                return;
            }

            Reviews review = new Reviews()
            {
                BookId = _bookId,
                UserId = MainWindow.CurrentUser.Id,

                Rating = int.Parse(
                    ((ComboBoxItem)RatingBox.SelectedItem)
                    .Content.ToString()),

                Comment = ReviewText.Text,

                CreatedAt = DateTime.Now
            };

            Core.Context.Reviews.Add(review);

            Core.Context.SaveChanges();

            UpdateRating();

            MessageBox.Show("Отзыв добавлен");

            ReviewText.Text = "";

            LoadBook();
        }

        private void UpdateRating()
        {
            var book = Core.Context.Books
                .First(x => x.Id == _bookId);

            var reviews = Core.Context.Reviews
                .Where(x => x.BookId == _bookId)
                .ToList();

            if (reviews.Count > 0)
            {
                book.AverageRating =
                    Convert.ToDecimal(
                    reviews.Average(x => x.Rating));
            }

            Core.Context.SaveChanges();
        }

        private void ComplainBook_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser == null)
            {
                MessageBox.Show("Авторизуйтесь");
                return;
            }

            Complaints complaint = new Complaints()
            {
                ComplaintTypeId = 1,
                SenderId = MainWindow.CurrentUser.Id,
                TargetBookId = _bookId,
                Reason = "Жалоба на книгу",
                Status = "PENDING",
                CreatedAt = DateTime.Now
            };

            Core.Context.Complaints.Add(complaint);

            Core.Context.SaveChanges();

            MessageBox.Show("Жалоба отправлена");
        }

        private void ComplainAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser == null)
            {
                MessageBox.Show("Авторизуйтесь");
                return;
            }

            var book = Core.Context.Books
                .First(x => x.Id == _bookId);

            Complaints complaint = new Complaints()
            {
                ComplaintTypeId = 2,
                SenderId = MainWindow.CurrentUser.Id,
                TargetUserId = book.AuthorId,
                Reason = "Жалоба на автора",
                Status = "PENDING",
                CreatedAt = DateTime.Now
            };

            Core.Context.Complaints.Add(complaint);

            Core.Context.SaveChanges();

            MessageBox.Show("Жалоба отправлена");
        }
    }
}
