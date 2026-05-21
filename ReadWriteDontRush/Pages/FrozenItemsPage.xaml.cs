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
    /// Логика взаимодействия для FrozenItemsPage.xaml
    /// </summary>
    public partial class FrozenItemsPage : Page
    {
        private List<FrozenItem> allItems;

        public class FrozenItem
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Reason { get; set; }
            public int Id { get; set; }
            public Brush TypeColor { get; set; }
        }

        public FrozenItemsPage(List<object> items)
        {
            InitializeComponent();

            // Конвертируем в типизированный список
            allItems = new List<FrozenItem>();
            foreach (dynamic item in items)
            {
                string type = item.Type;
                string color = type == "Книга" ? "#3498DB" :
                              (type == "Пользователь" ? "#E74C3C" : "#F39C12");
                allItems.Add(new FrozenItem
                {
                    Type = item.Type,
                    Name = item.Name,
                    Reason = item.Reason ?? "Причина не указана",
                    Id = item.Id,
                    TypeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
                });
            }

            LoadItems();
        }

        private void LoadItems(string filter = "All")
        {
            List<FrozenItem> filtered;

            switch (filter)
            {
                case "Books":
                    filtered = allItems.Where(i => i.Type == "Книга").ToList();
                    break;
                case "Users":
                    filtered = allItems.Where(i => i.Type == "Пользователь").ToList();
                    break;
                case "Reviews":
                    filtered = allItems.Where(i => i.Type == "Отзыв").ToList();
                    break;
                default:
                    filtered = allItems.ToList();
                    break;
            }

            if (filtered.Count == 0)
            {
                TxtNoItems.Visibility = Visibility.Visible;
                FrozenItemsControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoItems.Visibility = Visibility.Collapsed;
                FrozenItemsControl.Visibility = Visibility.Visible;
                FrozenItemsControl.ItemsSource = filtered;
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio != null && radio.IsChecked == true)
            {
                if (radio == RbAll) LoadItems("All");
                else if (radio == RbBooks) LoadItems("Books");
                else if (radio == RbUsers) LoadItems("Users");
                else if (radio == RbReviews) LoadItems("Reviews");
            }
        }

        private void Unfreeze_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as FrozenItem;
            if (item == null) return;

            var result = MessageBox.Show($"Разморозить {item.Type} '{item.Name}'?",
                                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (item.Type == "Книга")
                {
                    var book = Core.Context.Books.Find(item.Id);
                    if (book != null)
                    {
                        book.IsFrozen = false;
                        book.FreezeReason = null;
                    }
                }
                else if (item.Type == "Пользователь")
                {
                    var user = Core.Context.Users.Find(item.Id);
                    if (user != null)
                    {
                        user.IsFrozen = false;
                        user.FreezeReason = null;
                    }
                }
                else if (item.Type == "Отзыв")
                {
                    var review = Core.Context.Reviews.Find(item.Id);
                    if (review != null)
                    {
                        review.IsFrozen = false;
                        review.FreezeReason = null;
                    }
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Элемент разморожен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем страницу
                var adminPage = (Application.Current.MainWindow as MainWindow)?.MainFrame.Content as AdminPage;
                if (adminPage != null)
                {
                    var method = adminPage.GetType().GetMethod("LoadFrozenItems");
                    method?.Invoke(adminPage, null);
                }
            }
        }
    }
}
