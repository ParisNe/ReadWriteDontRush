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

            LoadBooks();
        }

        private void LoadBooks()
        {
            BooksGrid.ItemsSource =
                Core.Context.Books.ToList();
        }

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Books selectedBook =
                BooksGrid.SelectedItem as Books;

            if (selectedBook != null)
            {
                NavigationService.Navigate(
                    new BookPage(selectedBook));
            }
        }
    }
}
