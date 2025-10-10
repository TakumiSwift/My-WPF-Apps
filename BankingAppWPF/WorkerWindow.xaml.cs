using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using static BankingAppWPF.WorkerWindow;

namespace BankingAppWPF
{
    /// <summary>
    /// Логика взаимодействия для WorkerWindow.xaml
    /// </summary>
    public partial class WorkerWindow : Window
    {

        /// <summary>
        /// Делегат возврата должности в string
        /// </summary>
        private LogInData.ReturnTypeString<Position> posToString;

        /// <summary>
        /// Делегат возврата департамента в string
        /// </summary>
        private LogInData.ReturnTypeString<Department> depToString;

        /// <summary>
        /// Экземпляр авторизированного сотрудника Worker
        /// </summary>
        private Worker User;

        private List<Client> wClients;

        public WorkerWindow (Worker user)
        {
            InitializeComponent();
            User = user;
            posToString = (User.Position).PosToString;
            depToString = (User.Department).DepToString;
            wClients = LogInData.GetMyClients(User);
        }

        private void btnLeaveAccount_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow aW = new AuthorizationWindow();
            aW.Show();
            this.Close();
        }

        private void tItemMyCab_GotFocus(object sender, RoutedEventArgs e)
        {
            tbxWorkerPosition.Text = posToString(User.Position);
            for (int i = 0; i < 3; i++)
            {
                tbxWorkerFullname.Text += $"{User.Fullname.Split('%')[i]} ";
            }
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            lsvMyClients.ItemsSource = wClients;
        }

        private void btnNewOffer_Click(object sender, RoutedEventArgs e)
        {
            NewOffer nO = new(lsvMyClients.SelectedItem as Client);
            nO.Show();
        }
    }
}
