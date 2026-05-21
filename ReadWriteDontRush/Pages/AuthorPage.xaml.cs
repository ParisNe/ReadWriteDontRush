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
            BooksList.ItemsSource =
                Core.Context.Books
                .Where(x =>
                    x.AuthorId ==
                    MainWindow.CurrentUser.Id)
                .ToList();
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            Books book = new Books()
            {
                Title = TitleBox.Text,
                Description = DescriptionBox.Text,
                CoverUrl = CoverBox.Text,
                TextContent = ContentBox.Text,

                AuthorId = MainWindow.CurrentUser.Id,

                AverageRating = 0,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,

                IsFrozen = false
            };

            Core.Context.Books.Add(book);

            Core.Context.SaveChanges();

            MessageBox.Show("Книга добавлена");

            LoadBooks();
        }
    }
}
