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
using System.Windows.Shapes;

namespace ReadWriteDontRush
{
    /// <summary>
    /// Логика взаимодействия для ReasonInputWindow.xaml
    /// </summary>
    public partial class ReasonInputWindow : Window
    {
        public string Reason { get; private set; }

        public ReasonInputWindow(string targetType)
        {
            InitializeComponent();

            // Меняем заголовок в зависимости от того, на что жалуются
            if (targetType == "книгу")
                Title = "Жалоба на книгу";
            else if (targetType == "автора")
                Title = "Жалоба на автора";
            else if (targetType == "отзыв")
                Title = "Жалоба на отзыв";
            else
                Title = $"Жалоба на {targetType}";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtReason.Text))
            {
                MessageBox.Show("Пожалуйста, укажите причину жалобы",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            Reason = TxtReason.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
