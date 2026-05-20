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
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();

            LoadBooks();
        }

        private void LoadBooks()
        {
            BooksGrid.ItemsSource =
                Core.Context.Books
                .Where(x => x.AuthorId ==
                    App.CurrentUser.Id)
                .ToList();
        }

        private void AddBookBtn_Click(object sender, RoutedEventArgs e)
        {
            Books book = new Books()
            {
                Title = "Новая книга",
                Description = "Описание книги",
                TextContent = "Текст книги",
                AuthorId = App.CurrentUser.Id,
                AverageRating = 0,
                IsFrozen = false
            };

            Core.Context.Books.Add(book);

            Core.Context.SaveChanges();

            MessageBox.Show("Книга добавлена");

            LoadBooks();
        }

        private void DeleteBookBtn_Click(object sender, RoutedEventArgs e)
        {
            Books selectedBook =
                BooksGrid.SelectedItem as Books;

            if (selectedBook != null)
            {
                Core.Context.Books.Remove(selectedBook);

                Core.Context.SaveChanges();

                MessageBox.Show("Книга удалена");

                LoadBooks();
            }
        }
    }
}
