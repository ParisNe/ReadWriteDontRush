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
    /// Логика взаимодействия для BookListsPage.xaml
    /// </summary>
    public partial class BookListsPage : Page
    {
        private int currentListTypeId = 1;
        private Button activeButton;

        public class BookListViewModel
        {
            public int BookID { get; set; }
            public string Title { get; set; }
            public string CoverImagePath { get; set; }
            public string AuthorName { get; set; }
            public double AverageRating { get; set; }
            public string GenresString { get; set; }
            public int CurrentListTypeId { get; set; }
        }

        public BookListsPage()
        {
            InitializeComponent();
            LoadGenres();
            SetActiveTab(BtnWantToRead);
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

        private void SetActiveTab(Button active)
        {
            // Сброс стилей всех кнопок
            var buttons = new[] { BtnWantToRead, BtnReading, BtnRead, BtnDropped };
            foreach (var btn in buttons)
            {
                btn.BorderBrush = null;
                btn.BorderThickness = new Thickness(0);
                btn.Background = System.Windows.Media.Brushes.LightGray;
                btn.Foreground = System.Windows.Media.Brushes.Black;
            }

            // Активация выбранной кнопки
            active.BorderBrush = System.Windows.Media.Brushes.Blue;
            active.BorderThickness = new Thickness(0, 0, 0, 3);
            active.Background = System.Windows.Media.Brushes.White;
            active.Foreground = System.Windows.Media.Brushes.Blue;
            activeButton = active;
        }

        private void LoadCounts()
        {
            if (Core.CurrentUser == null) return;

            var userLists = Core.Context.UserBookLists.Where(ubl => ubl.UserID == Core.CurrentUser.UserID);

            TxtWantToReadCount.Text = $"({userLists.Count(ubl => ubl.ListTypeID == 1)})";
            TxtReadingCount.Text = $"({userLists.Count(ubl => ubl.ListTypeID == 2)})";
            TxtReadCount.Text = $"({userLists.Count(ubl => ubl.ListTypeID == 3)})";
            TxtDroppedCount.Text = $"({userLists.Count(ubl => ubl.ListTypeID == 4)})";
        }

        private void LoadBooks()
        {
            if (Core.CurrentUser == null) return;

            LoadCounts();

            var userBooks = Core.Context.UserBookLists
                .Where(ubl => ubl.UserID == Core.CurrentUser.UserID && ubl.ListTypeID == currentListTypeId)
                .Select(ubl => ubl.BookID)
                .ToList();

            var query = Core.Context.Books.Where(b => userBooks.Contains(b.BookID)).ToList();

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
            if (CmbGenre.SelectedItem != null && CmbGenre.SelectedItem is ComboBoxItem selectedGenre && selectedGenre.Tag != null)
            {
                int genreId = (int)selectedGenre.Tag;
                query = query.Where(b => b.BookGenres.Any(bg => bg.GenreID == genreId)).ToList();
            }

            var booksWithDetails = query.Select(b => new BookListViewModel
            {
                BookID = b.BookID,
                Title = b.Title ?? "Без названия",
                CoverImagePath = b.CoverImagePath,
                AuthorName = b.Users?.DisplayName ?? "Неизвестный автор",
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                GenresString = string.Join(", ", b.BookGenres.Select(bg => bg.Genre.GenreName)),
                CurrentListTypeId = currentListTypeId
            }).ToList();

            // Сортировка
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

        private void BtnList_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            currentListTypeId = int.Parse(button.Tag.ToString());
            SetActiveTab(button);
            LoadBooks();
        }

        private void CmbChangeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.SelectedItem == null) return;

            var bookVM = comboBox.DataContext as BookListViewModel;
            if (bookVM == null) return;

            int newListTypeId = int.Parse(((ComboBoxItem)comboBox.SelectedItem).Tag.ToString());

            if (newListTypeId == bookVM.CurrentListTypeId) return;

            var userBookList = Core.Context.UserBookLists
                .FirstOrDefault(ubl => ubl.UserID == Core.CurrentUser.UserID && ubl.BookID == bookVM.BookID);

            if (userBookList != null)
            {
                userBookList.ListTypeID = newListTypeId;
                Core.Context.SaveChanges();

                // Если переместили из текущего списка, обновляем
                if (newListTypeId != currentListTypeId)
                {
                    LoadBooks();
                }
                else
                {
                    LoadCounts();
                    LoadBooks();
                }
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
    }
}
