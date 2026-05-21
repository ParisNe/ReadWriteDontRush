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
        private Books editingBook;
        private bool isEditMode;

        public AddEditBookPage(Books book)
        {
            InitializeComponent();
            editingBook = book;
            isEditMode = book != null;

            LoadAuthors();
            LoadGenres();

            if (isEditMode)
            {
                TxtFormTitle.Text = "✏️ Редактирование книги";
                LoadBookData();
            }
        }

        private void LoadAuthors()
        {
            var authors = Core.Context.Users.Where(u => u.RoleID == 3).ToList();
            CmbAuthor.ItemsSource = authors;
            CmbAuthor.SelectedValuePath = "UserID";
        }

        private void LoadGenres()
        {
            var genres = Core.Context.Genres.ToList();
            LstGenres.ItemsSource = genres;
        }

        private void LoadBookData()
        {
            TxtTitle.Text = editingBook.Title;
            TxtDescription.Text = editingBook.Description;
            TxtContent.Text = editingBook.Content;
            TxtCoverPath.Text = editingBook.CoverImagePath;

            if (editingBook.CoverImagePath != null)
            {
                try
                {
                    ImgPreview.Source = new BitmapImage(new Uri(editingBook.CoverImagePath, UriKind.RelativeOrAbsolute));
                }
                catch { }
            }

            // Выбираем автора
            CmbAuthor.SelectedValue = editingBook.AuthorID;

            // Выбираем жанры
            var bookGenres = editingBook.BookGenres.Select(bg => bg.GenreID).ToList();
            foreach (var item in LstGenres.Items)
            {
                var genre = item as Genres;
                if (bookGenres.Contains(genre.GenreID))
                {
                    LstGenres.SelectedItems.Add(item);
                }
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите изображение обложки"
            };

            if (dialog.ShowDialog() == true)
            {
                TxtCoverPath.Text = dialog.FileName;
                try
                {
                    ImgPreview.Source = new BitmapImage(new Uri(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtTitle.Text))
                {
                    MessageBox.Show("Введите название книги", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CmbAuthor.SelectedValue == null)
                {
                    MessageBox.Show("Выберите автора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (LstGenres.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы один жанр", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!isEditMode)
                {
                    editingBook = new Books();
                    Core.Context.Books.Add(editingBook);
                }

                editingBook.Title = TxtTitle.Text;
                editingBook.Description = TxtDescription.Text;
                editingBook.Content = TxtContent.Text;
                editingBook.CoverImagePath = TxtCoverPath.Text;
                editingBook.AuthorID = (int)CmbAuthor.SelectedValue;
                editingBook.IsFrozen = false;
                editingBook.CreatedAt = DateTime.Now;

                if (!isEditMode)
                {
                    editingBook.ViewsCount = 0;
                }

                // Обновляем жанры
                if (isEditMode)
                {
                    var existingGenres = editingBook.BookGenres.ToList();
                    foreach (var eg in existingGenres)
                    {
                        Core.Context.BookGenres.Remove(eg);
                    }
                }

                foreach (var item in LstGenres.SelectedItems)
                {
                    var genre = item as Genres;
                    editingBook.BookGenres.Add(new BookGenres
                    {
                        BookID = editingBook.BookID,
                        GenreID = genre.GenreID
                    });
                }

                Core.Context.SaveChanges();

                MessageBox.Show(isEditMode ? "Книга обновлена" : "Книга добавлена", "Успех",
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
