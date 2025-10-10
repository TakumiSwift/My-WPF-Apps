using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BankingAppWPF.LogInData;

namespace BankingAppWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        string path;
        LogInData login;
        bool flag;

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            tbxPassword.Visibility = Visibility.Hidden;
            pbxPassword.Visibility = Visibility.Visible;
            pbxPassword.Focus();
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (login.InputCheck(tbxLogin.Text, pbxPassword.Password))
            {
                if (flag)
                {
                    WorkerWindow wW = new WorkerWindow(login.GetAuthorizatedUser(tbxLogin.Text) as Worker);
                    wW.Show();
                }
                else
                {
                    Offer o = new();
                    ClientWindow cW = new ClientWindow(login.GetAuthorizatedUser(tbxLogin.Text) as Client);
                    cW.Show();
                }
                    this.Close();
            }
            else
            {
                MessageBox.Show("Неправильный логин или пароль");
                pbxPassword.Password = "";
            }
            tbxLogin.Text = "Логин";
            tbxLogin.Visibility = Visibility.Hidden;
            tbxPassword.Visibility = Visibility.Hidden;
            pbxPassword.Visibility = Visibility.Hidden;
            btnEnter.Visibility = Visibility.Hidden;
            btnEnterWorker.Visibility = Visibility.Visible;
            btnEnterClient.Visibility = Visibility.Visible;
        }

        private void tbxLogin_GotFocus(object sender, RoutedEventArgs e)
        {
            tbxLogin.Text = "";
        }

        private void btnEnterWorker_Click(object sender, RoutedEventArgs e)
        {
            btnEnterWorker.Visibility = Visibility.Hidden;
            btnEnterClient.Visibility = Visibility.Hidden;
            tbxLogin.Visibility = Visibility.Visible;
            tbxPassword.Visibility = Visibility.Visible;
            btnEnter.Visibility = Visibility.Visible;
            path = @"WorkerData/WorkerData.xml";
            login = new(path);
            flag = true;
        }

        private void btnEnterClient_Click(object sender, RoutedEventArgs e)
        {
            btnEnterWorker.Visibility = Visibility.Hidden;
            btnEnterClient.Visibility = Visibility.Hidden;
            tbxLogin.Visibility = Visibility.Visible;
            tbxPassword.Visibility = Visibility.Visible;
            btnEnter.Visibility = Visibility.Visible;
            path = @"ClientData/ClientData.xml";
            login = new(path);
            flag = false;
        }
    }
}