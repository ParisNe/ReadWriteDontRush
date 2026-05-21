using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ReadWriteDontRush.Pages
{
    /// <summary>
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        private Books selectedBookForList;

        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadBooks();
        }

        private void LoadGenres()
        {
            try
            {
                var genres = Core.Context.Genres.ToList();
                CmbGenre.Items.Add(new ComboBoxItem { Content = "Все жанры", Tag = null });

                foreach (var genre in genres)
                {
                    CmbGenre.Items.Add(new ComboBoxItem { Content = genre.GenreName, Tag = genre.GenreID });
                }

                CmbGenre.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жанров: {ex.Message}");
            }
        }

        private void LoadBooks()
        {
            try
            {
                var query = Core.Context.Books.Where(b => b.IsFrozen == false).ToList();

                // Поиск
                string searchText = TxtSearch?.Text.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(b =>
                        (b.Title?.ToLower().Contains(searchText) ?? false) ||
                        (b.Users.DisplayName?.ToLower().Contains(searchText) ?? false)
                    ).ToList();
                }

                // Фильтр по жанру
                if (CmbGenre.SelectedItem is ComboBoxItem selectedGenre && selectedGenre.Tag != null)
                {
                    int genreId = (int)selectedGenre.Tag;
                    query = query.Where(b => b.BookGenres.Any(bg => bg.GenreID == genreId)).ToList();
                }

                // Сортировка
                switch (CmbSort?.SelectedIndex)
                {
                    case 1:
                        query = query.OrderByDescending(b => b.Title).ToList();
                        break;
                    case 2:
                        query = query.OrderByDescending(b => b.AverageRating).ToList();
                        break;
                    case 3:
                        query = query.OrderBy(b => b.AverageRating).ToList();
                        break;
                    default:
                        query = query.OrderBy(b => b.Title).ToList();
                        break;
                }

                BooksItemsControl.ItemsSource = query;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadBooks();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBooks();
        }

        private void CmbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBooks();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = "";
            CmbSort.SelectedIndex = 0;
            if (CmbGenre.Items.Count > 0)
                CmbGenre.SelectedIndex = 0;
            LoadBooks();
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var book = button?.Tag as Books;
            if (book != null)
            {
                NavigationService?.Navigate(new BookPage(book.BookID));
            }
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            selectedBookForList = button?.Tag as Books;

            if (selectedBookForList != null)
            {
                var contextMenu = new ContextMenu();
                var listTypes = Core.Context.BookListTypes.ToList();

                foreach (var listType in listTypes)
                {
                    var menuItem = new MenuItem { Header = listType.ListName, Tag = listType.ListTypeID };
                    menuItem.Click += AddToListMenuItem_Click;
                    contextMenu.Items.Add(menuItem);
                }

                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
            }
        }

        private void AddToListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            int listTypeId = (int)menuItem.Tag;
            string listName = menuItem.Header.ToString();

            try
            {
                var existing = Core.Context.UserBookLists
                    .FirstOrDefault(ubl => ubl.UserID == Core.CurrentUser.UserID &&
                                          ubl.BookID == selectedBookForList.BookID);

                if (existing != null)
                {
                    existing.ListTypeID = listTypeId;
                }
                else
                {
                    UserBookList newList = new UserBookList
                    {
                        UserID = Core.CurrentUser.UserID,
                        BookID = selectedBookForList.BookID,
                        ListTypeID = listTypeId,
                        AddedAt = DateTime.Now
                    };
                    Core.Context.UserBookLists.Add(newList);
                }

                Core.Context.SaveChanges();
                MessageBox.Show($"Книга добавлена в список '{listName}'", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
