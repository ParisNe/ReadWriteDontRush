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
            LoadMyBooks();
        }

        private void LoadMyBooks()
        {
            var books = Core.Context.Books
                .Where(b => b.AuthorID == Core.CurrentUser.UserID && b.IsFrozen == false)
                .ToList();

            var booksPage = new AuthorBooksPage(books, false);
            AuthorContentFrame.Navigate(booksPage);
        }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            AuthorContentFrame.Navigate(new AddEditBookPage(null));
        }

        private void BtnMyBooks_Click(object sender, RoutedEventArgs e)
        {
            LoadMyBooks();
        }

        private void BtnFrozenBooks_Click(object sender, RoutedEventArgs e)
        {
            var frozenBooks = Core.Context.Books
                .Where(b => b.AuthorID == Core.CurrentUser.UserID && b.IsFrozen == true)
                .ToList();

            var booksPage = new AuthorBooksPage(frozenBooks, true);
            AuthorContentFrame.Navigate(booksPage);
        }
    }
}
