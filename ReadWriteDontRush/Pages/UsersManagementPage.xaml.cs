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
using System.Security.Cryptography;

namespace ReadWriteDontRush.Pages
{
    /// <summary>
    /// Логика взаимодействия для UsersManagementPage.xaml
    /// </summary>
    public partial class UsersManagementPage : Page
    {
        private List<Users> allUsers;

        // Класс для отображения пользователя в списке
        public class UserViewModel
        {
            public int UserID { get; set; }
            public string Login { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsFrozen { get; set; }
            public string RoleName { get; set; }
            public string Status { get; set; }
            public Brush StatusColor { get; set; }
            public int RoleID { get; set; }
        }

        public UsersManagementPage(List<Users> users)
        {
            InitializeComponent();
            allUsers = users;
            LoadUsers();
        }

        private void LoadUsers()
        {
            string searchText = TxtSearch?.Text.Trim().ToLower() ?? "";

            var query = allUsers.AsEnumerable();

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(u => u.Login.ToLower().Contains(searchText) ||
                                        u.DisplayName.ToLower().Contains(searchText) ||
                                        u.Email.ToLower().Contains(searchText));
            }

            var userViewModels = query.Select(u => new UserViewModel
            {
                UserID = u.UserID,
                Login = u.Login,
                DisplayName = u.DisplayName,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                IsFrozen = u.IsFrozen ?? false,
                RoleName = u.RoleID == 1 ? "Администратор" : (u.RoleID == 2 ? "Пользователь" : "Автор"),
                Status = (u.IsFrozen ?? false) ? "Заморожен" : "Активен",
                StatusColor = (u.IsFrozen ?? false) ? Brushes.Red : Brushes.Green,
                RoleID = u.RoleID ?? 2
            }).OrderBy(u => u.DisplayName).ToList();

            UsersItemsControl.ItemsSource = userViewModels;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadUsers();

        private void CmbChangeRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.SelectedItem == null) return;

            // Получаем данные пользователя из DataContext
            var userVM = comboBox.DataContext as UserViewModel;
            if (userVM == null) return;

            int newRoleId = int.Parse(((ComboBoxItem)comboBox.SelectedItem).Tag.ToString());

            if (userVM.RoleID != newRoleId)
            {
                var result = MessageBox.Show($"Изменить роль пользователя {userVM.DisplayName} на {(newRoleId == 1 ? "Администратора" : (newRoleId == 2 ? "Пользователя" : "Автора"))}?",
                                            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var user = Core.Context.Users.Find(userVM.UserID);
                    if (user != null)
                    {
                        user.RoleID = newRoleId;
                        Core.Context.SaveChanges();
                        MessageBox.Show("Роль изменена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                }
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var userVM = button?.Tag as UserViewModel;
            if (userVM == null) return;

            var user = Core.Context.Users.Find(userVM.UserID);
            if (user == null) return;

            var window = new Window
            {
                Title = $"Смена пароля для {user.DisplayName}",
                Width = 400,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Введите новый пароль:",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.Bold
            });

            var passwordBox = new PasswordBox { Height = 35, Margin = new Thickness(0, 0, 0, 15) };
            stackPanel.Children.Add(passwordBox);

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Подтвердите пароль:",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.Bold
            });

            var confirmBox = new PasswordBox { Height = 35, Margin = new Thickness(0, 0, 0, 15) };
            stackPanel.Children.Add(confirmBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var okBtn = new Button { Content = "Сохранить", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
            okBtn.Click += (s, args) =>
            {
                string newPassword = passwordBox.Password;
                string confirm = confirmBox.Password;

                if (string.IsNullOrEmpty(newPassword))
                {
                    MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPassword != confirm)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                user.PasswordHash = HashPassword(newPassword);
                Core.Context.SaveChanges();

                MessageBox.Show("Пароль изменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void FreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var userVM = button?.Tag as UserViewModel;
            if (userVM == null) return;

            var user = Core.Context.Users.Find(userVM.UserID);
            if (user == null) return;

            var window = new Window
            {
                Title = $"Заморозка пользователя {user.DisplayName}",
                Width = 450,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Укажите причину заморозки:",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.Bold
            });

            var textBox = new TextBox
            {
                Height = 80,
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

            var okBtn = new Button { Content = "Заморозить", Width = 100, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
            okBtn.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Укажите причину заморозки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                user.IsFrozen = true;
                user.FreezeReason = textBox.Text;
                Core.Context.SaveChanges();

                MessageBox.Show("Пользователь заморожен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                window.Close();
                LoadUsers();
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
