using Microsoft.Win32;
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
    /// Логика взаимодействия для AddEditBookPage.xaml
    /// </summary>
    public partial class AddEditBookPage : Page
    {
        private int? editingBookId = null;
        private List<int> selectedGenreIds = new List<int>();

        // Конструктор для добавления новой книги
        public AddEditBookPage()
        {
            InitializeComponent();
            LoadGenres();
        }

        // Конструктор для редактирования существующей книги
        public AddEditBookPage(int bookId) : this()
        {
            editingBookId = bookId;
            TxtTitle.Text = "Редактирование книги";
            LoadBookData();
        }

        private void LoadGenres()
        {
            try
            {
                var genres = Core.Context.Genres.ToList();
                ListGenres.ItemsSource = genres;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жанров: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBookData()
        {
            try
            {
                if (editingBookId == null) return;

                var book = Core.Context.Books.Find(editingBookId);
                if (book == null)
                {
                    MessageBox.Show("Книга не найдена", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                TxtBookTitle.Text = book.Title;
                TxtDescription.Text = book.Description;
                TxtCoverPath.Text = book.CoverImagePath;
                TxtContent.Text = book.Content;

                // Отмечаем выбранные жанры
                selectedGenreIds = book.BookGenres.Select(bg => bg.GenreID).ToList();

                // Обновляем CheckBox в ListBox
                foreach (var item in ListGenres.Items)
                {
                    var genre = item as Genres;
                    if (genre != null && selectedGenreIds.Contains(genre.GenreID))
                    {
                        var container = ListGenres.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                        if (container != null)
                        {
                            var checkBox = FindVisualChild<CheckBox>(container);
                            if (checkBox != null)
                                checkBox.IsChecked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книги: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : FrameworkElement
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                    return (T)child;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void GenreCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag != null)
            {
                int genreId = (int)checkBox.Tag;
                if (!selectedGenreIds.Contains(genreId))
                    selectedGenreIds.Add(genreId);
            }
        }

        private void GenreCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag != null)
            {
                int genreId = (int)checkBox.Tag;
                if (selectedGenreIds.Contains(genreId))
                    selectedGenreIds.Remove(genreId);
            }
        }

        private void BtnBrowseCover_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите обложку книги",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TxtCoverPath.Text = openFileDialog.FileName;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(TxtBookTitle.Text))
                {
                    MessageBox.Show("Введите название книги", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtContent.Text))
                {
                    MessageBox.Show("Введите текст книги", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!selectedGenreIds.Any())
                {
                    MessageBox.Show("Выберите хотя бы один жанр", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Books book;

                if (editingBookId == null)
                {
                    // Создание новой книги
                    book = new Books
                    {
                        Title = TxtBookTitle.Text,
                        Description = TxtDescription.Text,
                        CoverImagePath = TxtCoverPath.Text,
                        Content = TxtContent.Text,
                        AuthorID = Core.CurrentUser.UserID,
                        IsFrozen = false,
                        CreatedAt = DateTime.Now
                    };
                    Core.Context.Books.Add(book);
                    Core.Context.SaveChanges();

                    // Добавляем жанры
                    foreach (var genreId in selectedGenreIds)
                    {
                        var bookGenre = new BookGenres
                        {
                            BookID = book.BookID,
                            GenreID = genreId,
                            Date = DateTime.Now
                        };
                        Core.Context.BookGenres.Add(bookGenre);
                    }
                }
                else
                {
                    // Редактирование существующей книги
                    book = Core.Context.Books.Find(editingBookId);
                    if (book == null)
                    {
                        MessageBox.Show("Книга не найдена", "Ошибка",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    book.Title = TxtBookTitle.Text;
                    book.Description = TxtDescription.Text;
                    book.CoverImagePath = TxtCoverPath.Text;
                    book.Content = TxtContent.Text;

                    // Обновляем жанры
                    var existingGenres = Core.Context.BookGenres.Where(bg => bg.BookID == book.BookID).ToList();
                    foreach (var existing in existingGenres)
                    {
                        Core.Context.BookGenres.Remove(existing);
                    }

                    foreach (var genreId in selectedGenreIds)
                    {
                        var bookGenre = new BookGenres
                        {
                            BookID = book.BookID,
                            GenreID = genreId,
                            Date = DateTime.Now
                        };
                        Core.Context.BookGenres.Add(bookGenre);
                    }
                }

                Core.Context.SaveChanges();

                MessageBox.Show("Книга успешно сохранена", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

