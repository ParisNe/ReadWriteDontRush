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
    /// Логика взаимодействия для UserProfilePage.xaml
    /// </summary>
    public partial class UserProfilePage : Page
    {
        public UserProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            LoadUserReviews();

            // Если пользователь не автор, показываем кнопку заявки
            if (Core.CurrentUser.RoleID == 2 || Core.CurrentUser.RoleID == 1)
            {
                BtnRequestAuthor.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Проверяем, не подавал ли пользователь уже заявку
                var existingRequest = Core.Context.AuthorRequests
                    .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID && r.Status == "На рассмотрении");

                if (existingRequest == null)
                    BtnRequestAuthor.Visibility = Visibility.Visible;
            }
        }

        private void LoadUserData()
        {
            var user = Core.CurrentUser;

            TxtDisplayName.Text = user.DisplayName;
            TxtLogin.Text = user.Login;
            TxtEmail.Text = user.Email;

            string roleName = user.RoleID == 1 ? "Администратор" :
                             (user.RoleID == 2 ? "Пользователь" : "Автор");
            TxtRole.Text = roleName;
            TxtCreatedAt.Text = user.CreatedAt.ToString("dd.MM.yyyy");

            // Исправлено: проверка на null
            bool isFrozen = user.IsFrozen.HasValue && user.IsFrozen.Value;
            if (isFrozen)
            {
                FreezeWarning.Visibility = Visibility.Visible;
                TxtFreezeReason.Text = user.FreezeReason ?? "Причина не указана";
            }
        }

        private void LoadUserReviews()
        {
            var reviews = Core.Context.Reviews
                .Where(r => r.UserID == Core.CurrentUser.UserID && r.IsFrozen == false)
                .Select(r => new
                {
                    r.ReviewID,
                    BookTitle = r.Books.Title,
                    r.Rating,
                    r.ReviewText,
                    r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            if (reviews.Any())
            {
                ReviewsItemsControl.ItemsSource = reviews;
                TxtNoReviews.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoReviews.Visibility = Visibility.Visible;
            }
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Заявка на роль автора",
                Width = 450,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Расскажите о себе и почему хотите стать автором:",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.Bold
            });

            var textBox = new TextBox
            {
                Height = 100,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            stackPanel.Children.Add(textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var okBtn = new Button { Content = "Отправить", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
            okBtn.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Напишите сообщение для администратора", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AuthorRequests request = new AuthorRequests
                {
                    UserID = Core.CurrentUser.UserID,
                    Message = textBox.Text,
                    Status = "На рассмотрении",
                    CreatedAt = DateTime.Now
                };

                Core.Context.AuthorRequests.Add(request);
                Core.Context.SaveChanges();

                MessageBox.Show("Заявка отправлена администратору", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                BtnRequestAuthor.Visibility = Visibility.Collapsed;
                window.Close();
            };

            var cancelBtn = new Button { Content = "Отмена", Width = 80, Height = 30 };
            cancelBtn.Click += (s, args) => window.Close();

            buttonPanel.Children.Add(okBtn);
            buttonPanel.Children.Add(cancelBtn);
            stackPanel.Children.Add(buttonPanel);

            window.Content = stackPanel;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void BtnAppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Оспаривание заморозки аккаунта",
                Width = 450,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Укажите причину для разморозки аккаунта:",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.Bold
            });

            var textBox = new TextBox
            {
                Height = 100,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            stackPanel.Children.Add(textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var okBtn = new Button { Content = "Отправить", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
            okBtn.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Укажите причину", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UnfreezeRequests request = new UnfreezeRequests
                {
                    UserID = Core.CurrentUser.UserID,
                    Reason = textBox.Text,
                    Status = "На рассмотрении",
                    CreatedAt = DateTime.Now
                };

                Core.Context.UnfreezeRequests.Add(request);
                Core.Context.SaveChanges();

                MessageBox.Show("Заявка на разморозку отправлена администратору", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                window.Close();
            };

            var cancelBtn = new Button { Content = "Отмена", Width = 80, Height = 30 };
            cancelBtn.Click += (s, args) => window.Close();

            buttonPanel.Children.Add(okBtn);
            buttonPanel.Children.Add(cancelBtn);
            stackPanel.Children.Add(buttonPanel);

            window.Content = stackPanel;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
    }
}
