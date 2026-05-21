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
    /// Логика взаимодействия для ComplaintsPage.xaml
    /// </summary>
    public partial class ComplaintsPage : Page
    {
        private dynamic complaintsData;

        public ComplaintsPage(dynamic complaints)
        {
            InitializeComponent();
            complaintsData = complaints;
            LoadComplaints();
        }

        private void LoadComplaints()
        {
            if (complaintsData == null || complaintsData.Count == 0)
            {
                TxtNoItems.Visibility = Visibility.Visible;
                ComplaintsItemsControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoItems.Visibility = Visibility.Collapsed;
                ComplaintsItemsControl.Visibility = Visibility.Visible;
                ComplaintsItemsControl.ItemsSource = complaintsData;
            }
        }

        private void AcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int complaintId = (int)button.Tag;

            var complaint = Core.Context.Complaints.Find(complaintId);
            if (complaint != null)
            {
                complaint.Status = "Принята";

                // Замораживаем книгу или отзыв
                if (complaint.BookID != null)
                {
                    var book = Core.Context.Books.Find(complaint.BookID);
                    if (book != null)
                    {
                        book.IsFrozen = true;
                        book.FreezeReason = $"Заморожена по жалобе #{complaintId}: {complaint.Reason}";
                    }
                }
                else if (complaint.ReviewID != null)
                {
                    var review = Core.Context.Reviews.Find(complaint.ReviewID);
                    if (review != null)
                    {
                        review.IsFrozen = true;
                        review.FreezeReason = $"Заморожен по жалобе #{complaintId}: {complaint.Reason}";
                    }
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба принята", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем список
                NavigationService?.Refresh();
            }
        }

        private void RejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int complaintId = (int)button.Tag;

            var complaint = Core.Context.Complaints.Find(complaintId);
            if (complaint != null)
            {
                complaint.Status = "Отклонена";
                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Refresh();
            }
        }
    }
}
