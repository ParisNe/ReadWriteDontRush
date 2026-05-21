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
        private dynamic allItems;

        public FrozenItemsPage(dynamic items)
        {
            InitializeComponent();
            allItems = items;

            // Добавляем цвета для типов
            var itemsWithColor = new List<dynamic>();
            foreach (var item in items)
            {
                string color = item.Type == "Книга" ? "#3498DB" :
                              (item.Type == "Пользователь" ? "#E74C3C" : "#F39C12");
                itemsWithColor.Add(new
                {
                    Type = item.Type,
                    Name = item.Name,
                    Reason = item.Reason ?? "Причина не указана",
                    Id = item.Id,
                    TypeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
                });
            }
            allItems = itemsWithColor;
            LoadItems();
        }

        private void LoadItems(string filter = "All")
        {
            var filtered = new List<dynamic>();

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
                    filtered = allItems;
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
            if (radio.IsChecked == true)
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
            var item = button.Tag;

            var result = MessageBox.Show($"Разморозить {item.GetType().GetProperty("Type").GetValue(item)} '{item.GetType().GetProperty("Name").GetValue(item)}'?",
                                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string type = item.GetType().GetProperty("Type").GetValue(item);
                int id = item.GetType().GetProperty("Id").GetValue(item);

                if (type == "Книга")
                {
                    var book = Core.Context.Books.Find(id);
                    if (book != null)
                    {
                        book.IsFrozen = false;
                        book.FreezeReason = null;
                    }
                }
                else if (type == "Пользователь")
                {
                    var user = Core.Context.Users.Find(id);
                    if (user != null)
                    {
                        user.IsFrozen = false;
                        user.FreezeReason = null;
                    }
                }
                else if (type == "Отзыв")
                {
                    var review = Core.Context.Reviews.Find(id);
                    if (review != null)
                    {
                        review.IsFrozen = false;
                        review.FreezeReason = null;
                    }
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Элемент разморожен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Refresh();
            }
        }
    }
}
