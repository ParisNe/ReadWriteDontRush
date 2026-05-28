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
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();

            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

            LoadUserInfo();
            LoadUserReviews();
            CheckExistingRequest();
        }

        private void LoadUserInfo()
        {
            try
            {
                var user = Core.CurrentUser;

                TxtLogin.Text = user.Login ?? "Не указан";
                TxtDisplayName.Text = user.DisplayName ?? "Не указано";
                TxtEmail.Text = user.Email ?? "Не указан";
                TxtCreatedAt.Text = user.CreatedAt.ToString("dd.MM.yyyy");

                // Определяем роль
                string roleName = "";
                switch (user.RoleID)
                {
                    case 1:
                        roleName = "Пользователь";
                        BtnRequestRole.Visibility = Visibility.Visible;
                        CmbRequestRole.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        roleName = "Автор";
                        CmbRequestRole.SelectedIndex = 1;
                        BtnRequestRole.Visibility = Visibility.Visible;
                        CmbRequestRole.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        roleName = "Администратор";
                        BtnRequestRole.Visibility = Visibility.Collapsed;
                        CmbRequestRole.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        roleName = "Неизвестно";
                        break;
                }
                TxtRole.Text = roleName;

                if (user.IsFrozen == true)
                {
                    TxtStatus.Text = "❌ Аккаунт заморожен";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
                    BorderFreezeWarning.Visibility = Visibility.Visible;
                    TxtFreezeReason.Text = "Причина заморозки: нарушение правил платформы.\n" +
                                           "Вы можете оспорить заморозку, отправив заявку администратору.";
                }
                else
                {
                    TxtStatus.Text = "✅ Аккаунт активен";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
                    BorderFreezeWarning.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных профиля: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckExistingRequest()
        {
            try
            {
                var existingRequest = Core.Context.AuthorRoleRequests
                    .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID && r.IsProcessed == false);

                if (existingRequest != null)
                {
                    string status = existingRequest.IsApproved == null ? "на рассмотрении" :
                                   (existingRequest.IsApproved == true ? "одобрена" : "отклонена");

                    BorderRequestStatus.Visibility = Visibility.Visible;
                    TxtRequestStatus.Text = $"📋 У вас есть поданная заявка на смену роли. Статус: {status}";

                    BtnRequestRole.IsEnabled = false;
                    BtnRequestRole.Content = "Заявка уже подана";
                    CmbRequestRole.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибку
            }
        }

        private void LoadUserReviews()
        {
            try
            {
                var reviews = Core.Context.Reviews
                    .Where(r => r.UserID == Core.CurrentUser.UserID)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                if (reviews != null && reviews.Any())
                {
                    ReviewsItemsControl.Visibility = Visibility.Visible;
                    TxtNoReviews.Visibility = Visibility.Collapsed;
                    ReviewsItemsControl.ItemsSource = reviews;
                }
                else
                {
                    ReviewsItemsControl.Visibility = Visibility.Collapsed;
                    TxtNoReviews.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                ReviewsItemsControl.Visibility = Visibility.Collapsed;
                TxtNoReviews.Visibility = Visibility.Visible;
            }
        }

        private void BtnRequestRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var existingRequest = Core.Context.AuthorRoleRequests
                    .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID && r.IsProcessed == false);

                if (existingRequest != null)
                {
                    MessageBox.Show("Вы уже подавали заявку на смену роли. Ожидайте рассмотрения.",
                                   "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                ComboBoxItem selectedItem = CmbRequestRole.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Выберите желаемую роль", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int targetRoleId = int.Parse(selectedItem.Tag.ToString());
                string targetRoleName = selectedItem.Content.ToString();
                string currentRoleName = TxtRole.Text;

                var result = MessageBox.Show($"Вы уверены, что хотите подать заявку на роль {targetRoleName}?\n" +
                                            $"Текущая роль: {currentRoleName}\n\n" +
                                            "Заявка будет рассмотрена администратором.",
                                            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Сохраняем желаемую роль в поле Reason (как временное решение)
                    AuthorRoleRequests request = new AuthorRoleRequests
                    {
                        UserID = Core.CurrentUser.UserID,
                        RequestDate = DateTime.Now,
                        IsProcessed = false,
                        IsApproved = null
                    };

                    Core.Context.AuthorRoleRequests.Add(request);
                    Core.Context.SaveChanges();

                    MessageBox.Show($"Заявка на роль {targetRoleName} отправлена администратору.\n" +
                                   "Ожидайте рассмотрения.",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    BtnRequestRole.IsEnabled = false;
                    BtnRequestRole.Content = "Заявка отправлена";
                    CmbRequestRole.IsEnabled = false;

                    BorderRequestStatus.Visibility = Visibility.Visible;
                    TxtRequestStatus.Text = $"📋 Заявка на роль {targetRoleName} отправлена. Статус: на рассмотрении";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки заявки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var existingRequest = Core.Context.UnfreezeRequests
                    .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID &&
                                        r.IsProcessed == false &&
                                        r.BookID == null);

                if (existingRequest != null)
                {
                    MessageBox.Show("Вы уже подавали заявку на снятие заморозки. Ожидайте рассмотрения.",
                                   "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var reasonWindow = new ReasonInputWindow("разморозку аккаунта");
                reasonWindow.Title = "Оспаривание заморозки";
                reasonWindow.Owner = Application.Current.MainWindow;

                if (reasonWindow.ShowDialog() == true)
                {
                    UnfreezeRequests request = new UnfreezeRequests
                    {
                        UserID = Core.CurrentUser.UserID,
                        BookID = null,
                        Reason = reasonWindow.Reason,
                        RequestDate = DateTime.Now,
                        IsProcessed = false,
                        IsApproved = null
                    };

                    Core.Context.UnfreezeRequests.Add(request);
                    Core.Context.SaveChanges();

                    MessageBox.Show("Заявка на снятие заморозки отправлена администратору.",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    BtnAppealFreeze.IsEnabled = false;
                    BtnAppealFreeze.Content = "Заявка отправлена";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки заявки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGoToBook_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                int bookId = (int)button.Tag;
                NavigationService?.Navigate(new BookPage(bookId));
            }
        }
    }
}
