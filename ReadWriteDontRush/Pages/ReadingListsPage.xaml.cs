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
    /// Логика взаимодействия для ReadingListsPage.xaml
    /// </summary>
    public class ReadingListBookViewModel
    {
        public int BookID { get; set; }
        public int ReadingListID { get; set; }
        public string Title { get; set; }
        public string CoverImagePath { get; set; }
        public string AuthorName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public string GenresString { get; set; }
        public string CurrentSection { get; set; }
    }

    public partial class ReadingListsPage : Page
    {
        private string currentSection = "В планах";
        private ReadingListBookViewModel selectedBookForMove;

        public ReadingListsPage()
        {
            InitializeComponent();

            // Проверяем авторизацию
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

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
                MessageBox.Show($"Ошибка загрузки жанров: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBooks()
        {
            try
            {
                if (Core.CurrentUser == null) return;

                // Проверяем, что контролы существуют
                if (BooksItemsControl == null || TxtEmptyMessage == null) return;

                // Получаем книги из текущего списка
                var readingList = Core.Context.ReadingLists
                    .Where(rl => rl.UserID == Core.CurrentUser.UserID && rl.Section == currentSection)
                    .ToList();

                if (!readingList.Any())
                {
                    BooksItemsControl.Visibility = Visibility.Collapsed;
                    TxtEmptyMessage.Visibility = Visibility.Visible;
                    return; // Важно: выходим из метода, если список пуст
                }

                BooksItemsControl.Visibility = Visibility.Visible;
                TxtEmptyMessage.Visibility = Visibility.Collapsed;

                // Получаем книги
                var bookIds = readingList.Select(rl => rl.BookID).ToList();
                var query = Core.Context.Books
                    .Where(b => bookIds.Contains(b.BookID) && b.IsFrozen == false)
                    .ToList();

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
                if (CmbGenre?.SelectedItem != null && CmbGenre.SelectedItem is ComboBoxItem selectedGenre && selectedGenre.Tag != null)
                {
                    int genreId = (int)selectedGenre.Tag;
                    query = query.Where(b => b.BookGenres.Any(bg => bg.GenreID == genreId)).ToList();
                }

                // Формируем ViewModel
                var booksWithDetails = query.Select(b => new ReadingListBookViewModel
                {
                    BookID = b.BookID,
                    ReadingListID = readingList.First(rl => rl.BookID == b.BookID).ReadingListID,
                    Title = b.Title ?? "Без названия",
                    CoverImagePath = b.CoverImagePath,
                    AuthorName = b.Users?.DisplayName ?? "Неизвестный автор",
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = b.Reviews.Count,
                    GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genres.GenreName)),
                    CurrentSection = currentSection
                }).ToList();

                // Сортировка
                if (CmbSort?.SelectedItem != null)
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
                        case 4: // По дате добавления (новые)
                            booksWithDetails = booksWithDetails.OrderByDescending(b =>
                                Core.Context.ReadingLists.First(rl => rl.BookID == b.BookID && rl.UserID == Core.CurrentUser.UserID).AddedAt
                            ).ToList();
                            break;
                        case 5: // По дате добавления (старые)
                            booksWithDetails = booksWithDetails.OrderBy(b =>
                                Core.Context.ReadingLists.First(rl => rl.BookID == b.BookID && rl.UserID == Core.CurrentUser.UserID).AddedAt
                            ).ToList();
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

        private void UpdateSectionButtons(string activeSection)
        {
            // Сбрасываем стили всех кнопок
            var buttons = new[] { BtnPlanned, BtnReading, BtnRead, BtnAbandoned };
            foreach (var btn in buttons)
            {
                btn.Opacity = 0.6;
                btn.FontWeight = FontWeights.Normal;
            }

            // Выделяем активную кнопку
            Button activeBtn = null;
            switch (activeSection)
            {
                case "В планах": activeBtn = BtnPlanned; break;
                case "Читаю": activeBtn = BtnReading; break;
                case "Прочитано": activeBtn = BtnRead; break;
                case "Заброшено": activeBtn = BtnAbandoned; break;
            }

            if (activeBtn != null)
            {
                activeBtn.Opacity = 1;
                activeBtn.FontWeight = FontWeights.Bold;
            }
        }

        private void BtnPlanned_Click(object sender, RoutedEventArgs e)
        {
            currentSection = "В планах";
            UpdateSectionButtons(currentSection);
            LoadBooks();
        }

        private void BtnReading_Click(object sender, RoutedEventArgs e)
        {
            currentSection = "Читаю";
            UpdateSectionButtons(currentSection);
            LoadBooks();
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            currentSection = "Прочитано";
            UpdateSectionButtons(currentSection);
            LoadBooks();
        }

        private void BtnAbandoned_Click(object sender, RoutedEventArgs e)
        {
            currentSection = "Заброшено";
            UpdateSectionButtons(currentSection);
            LoadBooks();
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

        // Переименованный метод для чтения книги
        private void OnReadBookClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var book = button?.Tag as ReadingListBookViewModel;
            if (book != null)
            {
                NavigationService?.Navigate(new BookPage(book.BookID));
            }
        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            selectedBookForMove = button?.Tag as ReadingListBookViewModel;

            if (selectedBookForMove == null) return;

            // Создаем диалог выбора нового списка
            var window = new Window
            {
                Title = "Переместить книгу",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var textBlock = new TextBlock
            {
                Text = $"Переместить \"{selectedBookForMove.Title}\" в список:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(textBlock);

            var sections = new[] { "В планах", "Читаю", "Прочитано", "Заброшено" };
            foreach (var section in sections)
            {
                if (section == currentSection) continue; // Не показываем текущий список

                var btn = new Button
                {
                    Content = section,
                    Height = 40,
                    Margin = new Thickness(0, 5, 0, 5),
                    Background = Brushes.LightGray,
                    Tag = section
                };
                btn.Click += (s, args) =>
                {
                    MoveBookToSection(section);
                    window.Close();
                };
                stackPanel.Children.Add(btn);
            }

            window.Content = stackPanel;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void MoveBookToSection(string newSection)
        {
            try
            {
                var readingListEntry = Core.Context.ReadingLists
                    .FirstOrDefault(rl => rl.ReadingListID == selectedBookForMove.ReadingListID);

                if (readingListEntry != null)
                {
                    readingListEntry.Section = newSection;
                    Core.Context.SaveChanges();
                    MessageBox.Show($"Книга перемещена в раздел '{newSection}'", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var book = button?.Tag as ReadingListBookViewModel;

            if (book == null) return;

            var result = MessageBox.Show($"Удалить книгу \"{book.Title}\" из списка?",
                                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var readingListEntry = Core.Context.ReadingLists
                        .FirstOrDefault(rl => rl.ReadingListID == book.ReadingListID);

                    if (readingListEntry != null)
                    {
                        Core.Context.ReadingLists.Remove(readingListEntry);
                        Core.Context.SaveChanges();
                        MessageBox.Show("Книга удалена из списка", "Успех",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadBooks();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
