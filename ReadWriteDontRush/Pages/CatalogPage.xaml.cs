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

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            BooksGrid.ItemsSource =
                Core.Context.Books
                .Where(x => x.Title.Contains(TbSearch.Text))
                .ToList();
        }
    }
}
