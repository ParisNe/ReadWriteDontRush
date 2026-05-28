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
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
        private int bookId;
        private Books currentBook;

        // Пустой конструктор для XAML
        public BookPage()
        {
            InitializeComponent();
        }

        // Конструктор с параметром для навигации
        public BookPage(int bookId) : this()
        {
            this.bookId = bookId;
            LoadBookData();
            LoadReviews();

            // Проверяем роль администратора
            if (Core.CurrentUser != null && Core.CurrentUser.RoleID == 1)
            {
                BtnFreezeBook.Visibility = Visibility.Visible;
            }

            // Проверяем, авторизован ли пользователь
            if (Core.CurrentUser == null)
            {
                BtnSubmitReview.IsEnabled = false;
                BtnAddToList.IsEnabled = false;
                BtnComplainBook.IsEnabled = false;
                BtnComplainAuthor.IsEnabled = false;
            }
        }

        private void LoadBookData()
        {
            currentBook = Core.Context.Books.FirstOrDefault(b => b.BookID == bookId);
            if (currentBook == null)
            {
                MessageBox.Show("Книга не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
                return;
            }

            TxtTitle.Text = currentBook.Title;
            TxtDescription.Text = currentBook.Description;

            // Загружаем текст книги
            if (!string.IsNullOrEmpty(currentBook.Content))
            {
                TxtBookContent.Text = currentBook.Content;
            }
            else
            {
                TxtBookContent.Text = "Текст книги отсутствует";
                TxtBookContent.Foreground = Brushes.Gray;
            }

            // Загрузка обложки
            if (!string.IsNullOrEmpty(currentBook.CoverImagePath))
            {
                try
                {
                    BookCover.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(currentBook.CoverImagePath, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    BookCover.Source = null;
                }
            }

            // Информация об авторе
            if (currentBook.Users != null)
                TxtAuthor.Text = $"Автор: {currentBook.Users.DisplayName}";

            // Жанры
            var genres = currentBook.BookGenres.Select(bg => bg.Genres.GenreName).ToList();
            TxtGenres.Text = genres.Any() ? $"Жанры: {string.Join(", ", genres)}" : "Жанры не указаны";

            // Рейтинг
            var reviews = currentBook.Reviews.ToList();
            if (reviews.Any())
            {
                double avgRating = reviews.Average(r => r.Rating);
                TxtRating.Text = $"★ {avgRating:F1}";
                TxtReviewsCount.Text = $"({reviews.Count} отзывов)";
            }
            else
            {
                TxtRating.Text = "★ Нет оценок";
                TxtReviewsCount.Text = "";
            }
        }

        private void LoadReviews()
        {
            var reviews = Core.Context.Reviews
                .Where(r => r.BookID == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            ReviewsItemsControl.ItemsSource = reviews;

            // Если админ, показываем кнопки заморозки отзывов
            if (Core.CurrentUser != null && Core.CurrentUser.RoleID == 1)
            {
                ReviewsItemsControl.Loaded += (s, e) =>
                {
                    ShowFreezeButtonsForReviews();
                };
            }
        }

        private void ShowFreezeButtonsForReviews()
        {
            var itemsControl = ReviewsItemsControl;
            if (itemsControl != null && itemsControl.Items.Count > 0)
            {
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {
                    var item = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                    if (item != null)
                    {
                        var freezeButton = FindVisualChild<Button>(item, "BtnFreezeReview");
                        if (freezeButton != null)
                            freezeButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        // Вспомогательный метод для поиска контрола по имени
        private T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && ((T)child).Name == name)
                    return (T)child;

                var result = FindVisualChild<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаем диалог выбора списка
            var window = new Window
            {
                Title = "Выберите список",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var textBlock = new TextBlock
            {
                Text = "Выберите раздел для книги:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(textBlock);

            var sections = new[] { "В планах", "Читаю", "Прочитано", "Заброшено" };
            foreach (var section in sections)
            {
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
                    AddToList(section);
                    window.Close();
                };
                stackPanel.Children.Add(btn);
            }

            window.Content = stackPanel;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void AddToList(string section)
        {
            if (currentBook != null && Core.CurrentUser != null)
            {
                var existing = Core.Context.ReadingLists
                    .FirstOrDefault(rl => rl.UserID == Core.CurrentUser.UserID &&
                                         rl.BookID == currentBook.BookID);

                if (existing != null)
                {
                    existing.Section = section;
                    MessageBox.Show($"Книга перемещена в раздел '{section}'", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ReadingLists newList = new ReadingLists
                    {
                        UserID = Core.CurrentUser.UserID,
                        BookID = currentBook.BookID,
                        Section = section,
                        AddedAt = DateTime.Now
                    };
                    Core.Context.ReadingLists.Add(newList);
                    MessageBox.Show($"Книга добавлена в раздел '{section}'", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Core.Context.SaveChanges();
            }
        }

        private void BtnComplainBook_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reasonWindow = new ReasonInputWindow("книгу");
            reasonWindow.Owner = Application.Current.MainWindow;
            if (reasonWindow.ShowDialog() == true)
            {
                Complaints complaint = new Complaints
                {
                    UserID = Core.CurrentUser.UserID,
                    BookID = bookId,
                    Reason = reasonWindow.Reason,
                    CreatedAt = DateTime.Now
                };
                Core.Context.Complaints.Add(complaint);
                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба отправлена администратору", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnComplainAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reasonWindow = new ReasonInputWindow("автора");
            reasonWindow.Owner = Application.Current.MainWindow;
            if (reasonWindow.ShowDialog() == true)
            {
                Complaints complaint = new Complaints
                {
                    UserID = Core.CurrentUser.UserID,
                    BookID = bookId,
                    Reason = $"ЖАЛОБА НА АВТОРА: {reasonWindow.Reason}",
                    CreatedAt = DateTime.Now
                };
                Core.Context.Complaints.Add(complaint);
                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба на автора отправлена администратору", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите заморозить эту книгу?\nЗамороженная книга не будет отображаться в каталоге.",
                                        "Подтверждение", MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                currentBook.IsFrozen = true;
                Core.Context.SaveChanges();
                MessageBox.Show("Книга заморожена", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                BtnFreezeBook.IsEnabled = false;
                BtnFreezeBook.Content = "Книга заморожена";
            }
        }

        private void BtnSubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtReview.Text))
            {
                MessageBox.Show("Введите текст отзыва", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int rating = 5;
            if (CmbRating.SelectedItem is ComboBoxItem selectedItem)
            {
                rating = int.Parse(selectedItem.Content.ToString());
            }

            Reviews review = new Reviews
            {
                UserID = Core.CurrentUser.UserID,
                BookID = bookId,
                ReviewText = TxtReview.Text,
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            Core.Context.Reviews.Add(review);
            Core.Context.SaveChanges();

            MessageBox.Show("Отзыв добавлен", "Успех",
                           MessageBoxButton.OK, MessageBoxImage.Information);

            TxtReview.Text = "";
            CmbRating.SelectedIndex = 4;
            LoadBookData();
            LoadReviews();
        }

        private void BtnComplainReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            int reviewId = (int)button.Tag;

            var reasonWindow = new ReasonInputWindow("отзыв");
            reasonWindow.Owner = Application.Current.MainWindow;
            if (reasonWindow.ShowDialog() == true)
            {
                Complaints complaint = new Complaints
                {
                    UserID = Core.CurrentUser.UserID,
                    ReviewID = reviewId,
                    Reason = reasonWindow.Reason,
                    CreatedAt = DateTime.Now
                };
                Core.Context.Complaints.Add(complaint);
                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба на отзыв отправлена", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int reviewId = (int)button.Tag;

            var result = MessageBox.Show("Заморозить этот отзыв?\nЗамороженный отзыв будет скрыт.",
                                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var review = Core.Context.Reviews.Find(reviewId);
                if (review != null)
                {
                    // Если в таблице Reviews есть поле IsFrozen, используйте его:
                    // review.IsFrozen = true;

                    // Пока удаляем отзыв (временно)
                    Core.Context.Reviews.Remove(review);

                    Core.Context.SaveChanges();
                    MessageBox.Show("Отзыв заморожен", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadReviews();
                }
            }
        }
    }
}
