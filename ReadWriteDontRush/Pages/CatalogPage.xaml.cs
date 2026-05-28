using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadWriteDontRush.Pages
{
    /// <summary>
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public class BookViewModel
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImagePath { get; set; }
        public string Content { get; set; }
        public Users Author { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime CreatedAt { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public string GenresString { get; set; }
    }

    public partial class CatalogPage : Page
    {
        private Books selectedBookForList;

        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
        }

        private void LoadGenres()
        {
            try
            {
                // Сначала загружаем жанры
                var genres = Core.Context.Genres.ToList();

                // Добавляем пункт "Все жанры"
                CmbGenre.Items.Add(new ComboBoxItem { Content = "Все жанры", Tag = null });

                // Добавляем остальные жанры
                foreach (var genre in genres)
                {
                    CmbGenre.Items.Add(new ComboBoxItem { Content = genre.GenreName, Tag = genre.GenreID });
                }

                CmbGenre.SelectedIndex = 0;

                // После загрузки жанров загружаем книги
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
                // Проверяем, что комбобоксы существуют
                if (CmbGenre == null || CmbSort == null)
                    return;

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

                // Фильтр по жанру - проверяем, что выбран элемент
                if (CmbGenre.SelectedItem != null && CmbGenre.SelectedItem is ComboBoxItem selectedGenre && selectedGenre.Tag != null)
                {
                    int genreId = (int)selectedGenre.Tag;
                    query = query.Where(b => b.BookGenres.Any(bg => bg.GenreID == genreId)).ToList();
                }

                // Добавляем вычисляемые поля
                var booksWithDetails = query.Select(b => new BookViewModel
                {
                    BookID = b.BookID,
                    Title = b.Title ?? "Без названия",
                    Description = b.Description ?? "",
                    CoverImagePath = b.CoverImagePath,
                    Content = b.Content ?? "",
                    Author = b.Users,
                    IsFrozen = b.IsFrozen,
                    CreatedAt = b.CreatedAt,
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = b.Reviews.Count,
                    GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genres.GenreName))
                }).ToList();

                // Сортировка
                if (CmbSort.SelectedItem != null)
                {
                    switch (CmbSort.SelectedIndex)
                    {
                        case 0: // По названию (А-Я)
                            booksWithDetails = booksWithDetails.OrderBy(b => b.Title).ToList();
                            break;
                        case 1: // По названию (Я-А)
                            booksWithDetails = booksWithDetails.OrderByDescending(b => b.Title).ToList();
                            break;
                        case 2: // По рейтингу (высокий)
                            booksWithDetails = booksWithDetails.OrderByDescending(b => b.AverageRating).ToList();
                            break;
                        case 3: // По рейтингу (низкий)
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

                // Создаем контекстное меню динамически
                var contextMenu = new ContextMenu();
                var sections = new[] { "В планах", "Читаю", "Прочитано", "Заброшено" };

                foreach (var section in sections)
                {
                    var menuItem = new MenuItem { Header = section, Tag = section };
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

            string section = menuItem.Tag.ToString();

            if (selectedBookForList != null && Core.CurrentUser != null)
            {
                try
                {
                    // Проверяем, нет ли уже книги в списках
                    var existing = Core.Context.ReadingLists
                        .FirstOrDefault(rl => rl.UserID == Core.CurrentUser.UserID &&
                                             rl.BookID == selectedBookForList.BookID);

                    if (existing != null)
                    {
                        // Обновляем существующую запись
                        existing.Section = section;
                        MessageBox.Show($"Книга перемещена в раздел '{section}'", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Добавляем новую
                        ReadingLists newList = new ReadingLists
                        {
                            UserID = Core.CurrentUser.UserID,
                            BookID = selectedBookForList.BookID,
                            Section = section,
                            AddedAt = DateTime.Now
                        };
                        Core.Context.ReadingLists.Add(newList);
                        MessageBox.Show($"Книга добавлена в раздел '{section}'", "Успех",
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
