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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();

            LoadComplaints();
            LoadUsers();
        }

        private void LoadComplaints()
        {
            ComplaintsGrid.ItemsSource =
                Core.Context.Complaints.ToList();
        }

        private void LoadUsers()
        {
            UsersGrid.ItemsSource =
                Core.Context.Users.ToList();
        }

        private void FreezeUserBtn_Click(object sender, RoutedEventArgs e)
        {
            Users selectedUser =
                UsersGrid.SelectedItem as Users;

            if (selectedUser != null)
            {
                selectedUser.IsFrozen = true;

                Core.Context.SaveChanges();

                MessageBox.Show("Пользователь заморожен");

                LoadUsers();
            }
        }

        private void UnfreezeUserBtn_Click(object sender, RoutedEventArgs e)
        {
            Users selectedUser =
                UsersGrid.SelectedItem as Users;

            if (selectedUser != null)
            {
                selectedUser.IsFrozen = false;

                Core.Context.SaveChanges();

                MessageBox.Show("Пользователь разморожен");

                LoadUsers();
            }
        }
    }
}
