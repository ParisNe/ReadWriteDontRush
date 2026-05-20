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
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
        Books currentBook;

        public BookPage(Books book)
        {
            InitializeComponent();

            currentBook = book;

            LoadBook();
            LoadReviews();
        }

        private void LoadBook()
        {
            TitleTb.Text = currentBook.Title;

            DescTb.Text = currentBook.Description;

            ContentTb.Text = currentBook.TextContent;
        }

        private void LoadReviews()
        {
            ReviewsGrid.ItemsSource =
                Core.Context.Reviews
                .Where(x => x.BookId == currentBook.Id)
                .ToList();
        }

        private void AddReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            Reviews review = new Reviews()
            {
                BookId = currentBook.Id,
                UserId = App.CurrentUser.Id,
                Rating = 5,
                Comment = ReviewTb.Text
            };

            Core.Context.Reviews.Add(review);

            Core.Context.SaveChanges();

            MessageBox.Show("Отзыв добавлен");

            LoadReviews();
        }
    }
}
