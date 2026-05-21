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

        public class BookViewModel
        {
            public int BookID { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string CoverImagePath { get; set; }
            public string Content { get; set; }
            public string AuthorName { get; set; }
            public int AuthorID { get; set; }
            public bool IsFrozen { get; set; }
            public DateTime CreatedAt { get; set; }
            public double AverageRating { get; set; }
            public int ReviewsCount { get; set; }
            public string GenresString { get; set; }
        }

        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
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
                LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жанров: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBooks()
        {
            try
            {
                var query = Core.Context.Books.Where(b => b.IsFrozen == false).ToList();

                string searchText = TxtSearch?.Text.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(b =>
                        (b.Title?.ToLower().Contains(searchText) ?? false) ||
                        (b.Users.DisplayName?.ToLower().Contains(searchText) ?? false)
                    ).ToList();
                }

                if (CmbGenre.SelectedItem != null && CmbGenre.SelectedItem is ComboBoxItem selectedGenre && selectedGenre.Tag != null)
                {
                    int genreId = (int)selectedGenre.Tag;
                    query = query.Where(b => b.BookGenres.Any(bg => bg.GenreID == genreId)).ToList();
                }

                var booksWithDetails = query.Select(b => new BookViewModel
                {
                    BookID = b.BookID,
                    Title = b.Title ?? "Без названия",
                    Description = b.Description ?? "",
                    CoverImagePath = b.CoverImagePath,
                    Content = b.Content ?? "",
                    AuthorName = b.Users?.DisplayName ?? "Неизвестный автор",
                    AuthorID = b.AuthorID,
                    IsFrozen = b.IsFrozen,
                    CreatedAt = b.CreatedAt,
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = b.Reviews.Count,
                    GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genre.GenreName))
                }).ToList();

                if (CmbSort.SelectedItem != null)
                {
                    switch (CmbSort.SelectedIndex)
                    {
                        case 0:
                            booksWithDetails = booksWithDetails.OrderBy(b => b.Title).ToList();
                            break;
                        case 1:
                            booksWithDetails = booksWithDetails.OrderByDescending(b => b.Title).ToList();
                            break;
                        case 2:
                            booksWithDetails = booksWithDetails.OrderByDescending(b => b.AverageRating).ToList();
                            break;
                        case 3:
                            booksWithDetails = booksWithDetails.OrderBy(b => b.AverageRating).ToList();
                            break;
                    }
                }

                BooksItemsControl.ItemsSource = booksWithDetails;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadBooks();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadBooks();
        private void CmbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadBooks();

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
            var book = button?.Tag as BookViewModel;
            if (book != null)
            {
                NavigationService?.Navigate(new BookPage(book.BookID));
            }
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bookVM = button?.Tag as BookViewModel;
            if (bookVM != null)
            {
                selectedBookForList = Core.Context.Books.Find(bookVM.BookID);

                var contextMenu = new ContextMenu();
                var listTypes = Core.Context.BookListTypes.ToList();

                foreach (var listType in listTypes)
                {
                    var menuItem = new MenuItem { Header = listType.ListName, Tag = listType.ListTypeID };
                    menuItem.Click += AddToList_Click;
                    contextMenu.Items.Add(menuItem);
                }

                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
            }
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            int listTypeId = (int)menuItem.Tag;
            string listName = menuItem.Header.ToString();

            if (selectedBookForList != null && Core.CurrentUser != null)
            {
                try
                {
                    var existing = Core.Context.UserBookLists
                        .FirstOrDefault(ubl => ubl.UserID == Core.CurrentUser.UserID &&
                                              ubl.BookID == selectedBookForList.BookID);

                    if (existing != null)
                    {
                        existing.ListTypeID = listTypeId;
                        MessageBox.Show($"Книга перемещена в список '{listName}'", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        UserBookLists newList = new UserBookLists
                        {
                            UserID = Core.CurrentUser.UserID,
                            BookID = selectedBookForList.BookID,
                            ListTypeID = listTypeId,
                            AddedAt = DateTime.Now
                        };
                        Core.Context.UserBookLists.Add(newList);
                        MessageBox.Show($"Книга добавлена в список '{listName}'", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Core.Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
