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
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();

            GenreBox.ItemsSource = Core.Context.Genres.ToList();

            UpdateBooks();
        }

        private void UpdateBooks(object sender = null, EventArgs e = null)
        {
            BooksPanel.Children.Clear();

            var books = Core.Context.Books
                .Where(x => !x.IsFrozen)
                .ToList();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                books = books.Where(x =>
                    x.Title.ToLower().Contains(SearchBox.Text.ToLower()) ||
                    x.Users.DisplayName.ToLower().Contains(SearchBox.Text.ToLower()))
                    .ToList();
            }

            if (SortBox.SelectedIndex == 0)
                books = books.OrderBy(x => x.Title).ToList();

            if (SortBox.SelectedIndex == 1)
                books = books.OrderByDescending(x => x.AverageRating).ToList();

            if (GenreBox.SelectedItem != null)
            {
                Genres genre = GenreBox.SelectedItem as Genres;

                books = books.Where(x =>
                    x.BookGenres.Any(g => g.GenreId == genre.Id))
                    .ToList();
            }

            foreach (var book in books)
            {
                BooksPanel.Children.Add(CreateBookCard(book));
            }
        }

        private Border CreateBookCard(Books book)
        {
            StackPanel panel = new StackPanel();

            Image image = new Image()
            {
                Width = 120,
                Height = 160,
                Margin = new Thickness(5)
            };

            try
            {
                image.Source = new BitmapImage(new Uri(book.CoverUrl));
            }
            catch { }

            TextBlock title = new TextBlock()
            {
                Text = book.Title,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };

            Button openBtn = new Button()
            {
                Content = "Открыть",
                Tag = book,
                Margin = new Thickness(5)
            };

            openBtn.Click += OpenBook_Click;

            panel.Children.Add(image);
            panel.Children.Add(title);
            panel.Children.Add(openBtn);

            return new Border()
            {
                BorderThickness = new Thickness(1),
                Margin = new Thickness(10),
                Padding = new Thickness(5),
                Child = panel
            };
        }

        private void OpenBook_Click(object sender, RoutedEventArgs e)
        {
            Books book = (sender as Button).Tag as Books;

            NavigationService.Navigate(new BookPage(book.Id));
        }
    }
}
