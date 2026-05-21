using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TxtLogin.Text.Trim();
                string password = TxtPassword.Password;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string hashedPassword = HashPassword(password);

                var user = Core.Context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == hashedPassword);

                if (user != null)
                {
                    Core.CurrentUser = user;

                    MessageBox.Show($"Добро пожаловать, {user.DisplayName}!", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    (Application.Current.MainWindow as MainWindow)?.MainFrame.Navigate(new CatalogPage());
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TxtRegLogin.Text.Trim();
                string password = TxtRegPassword.Password;
                string confirm = TxtConfirmPassword.Password;
                string displayName = TxtDisplayName.Text.Trim();
                string email = TxtEmail.Text.Trim();

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (password != confirm)
                {
                    MessageBox.Show("Пароли не совпадают!", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка на существующего пользователя
                if (Core.Context.Users.Any(u => u.Login == login))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Core.Context.Users.Any(u => u.Email == email))
                {
                    MessageBox.Show("Пользователь с таким email уже существует!", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создание нового пользователя
                Users newUser = new Users
                {
                    Login = login,
                    PasswordHash = HashPassword(password),
                    DisplayName = displayName,
                    Email = email,
                    RoleID = 2, // Роль "Пользователь"
                    IsFrozen = false,
                    CreatedAt = DateTime.Now
                };

                Core.Context.Users.Add(newUser);
                Core.Context.SaveChanges();

                MessageBox.Show("Регистрация успешна! Теперь войдите в систему.", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем поля
                TxtRegLogin.Text = "";
                TxtRegPassword.Password = "";
                TxtConfirmPassword.Password = "";
                TxtDisplayName.Text = "";
                TxtEmail.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
