using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;

namespace BankingAppWPF
{
    /// <summary>
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {

        public static event Func<Client,(List<BankAccountType>,List<string>)> OfferShow;

        Client User;

        bool flag;

        List<BankAccountType> offers;

        BankAccountType offer;

        List<string> idAll;

        int index;

        public ClientWindow(Client user)
        {
            User = user;
            try
            {
                offers = OfferShow(User).Item1;
                idAll = OfferShow(User).Item2;
            }
            catch { }
            InitializeComponent();
        }

        private void cbxAccounts_DropDownOpened(object sender, EventArgs e)
        {
            (sender as ComboBox).ItemsSource = null;
            (sender as ComboBox).ItemsSource = User.AccountNumbers;
        }

        private void cbxAccounts_DropDownClosed(object sender, EventArgs e)
        {
            if (cbxAccounts.SelectedItem != null)
            {
                tbkAccountInfo.Visibility = Visibility.Visible;
                gridInfo.Visibility = Visibility.Visible;
                lbxAccountInfo.Items[0] = User.GetAccountString(cbxAccounts.SelectedIndex, User);
                lbxAccountInfo.Items[1] = User.AccountNumbers[cbxAccounts.SelectedIndex]
                                              .AccountNumber;
                lbxAccountInfo.Items[2] = User.AccountNumbers[cbxAccounts.SelectedIndex]
                                              .MoneyAmount;
                lbxAccountInfo.Items[3] = User.AccountNumbers[cbxAccounts.SelectedIndex]
                                              .AccountType
                                              .GetOpeningDate(User.AccountNumbers[cbxAccounts.SelectedIndex]
                                                                  .AccountType);
                switch (User.AccountNumbers[cbxAccounts.SelectedIndex].AccountType)
                {
                    case DebitAccount:
                        lbxAccountInfo.Items[4] = "Бессрочный";
                        lbxAccountInfo.Items[5] = "Данный Счет не является Сберегательным/Кредитным";
                        break;
                    default:
                        lbxAccountInfo.Items[4] = User.AccountNumbers[cbxAccounts.SelectedIndex]
                                                      .AccountType
                                                      .GetPaymentPeriod(User.AccountNumbers[cbxAccounts.SelectedIndex]
                                                                            .AccountType) + " месяцев";
                        lbxAccountInfo.Items[5] = User.AccountNumbers[cbxAccounts.SelectedIndex]
                                                      .AccountType
                                                      .GetInterestRate(User.AccountNumbers[cbxAccounts.SelectedIndex]
                                                                            .AccountType) + "%";
                        break;
                }
            }
        }

        private void btnNewAccount_Click(object sender, RoutedEventArgs e)
        {
            gridNewAccount.Visibility = Visibility.Visible;
            btnDebit.Click += btnDebit_Click;
            btnCredit.Click += btnCredit_Click;
            btnSavings.Click += btnSavings_Click;
            btnConfirm.Click += btnConfirm_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnSavings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Банк предлагает Вам Вкладовый счет на 12 месяцев под 15%, с суммой 50000");
            btnConfirm.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            gridNewAccount.Visibility = Visibility.Hidden;
            flag = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            btnConfirm.Visibility = Visibility.Hidden;
            btnCancel.Visibility = Visibility.Hidden;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (flag)
            {
                User.AddNewAccount(User,
                                   new CreditAccount(""),
                                   LogInData.GetLastAccountNumber(new CreditAccount("")),
                                   100000, 20, new TimeSpan(24 * 30, 0, 0, 0));
            }
            else
            {
                User.AddNewAccount(User,
                                   new SavingsAccount(""),
                                   LogInData.GetLastAccountNumber(new SavingsAccount("")),
                                   50000, 15, new TimeSpan(12 * 30, 0, 0, 0));
            }
            MessageBox.Show("Новый счет успешно открыт, вы сможете увидеть его в списке ваших счетов!");
            btnConfirm.Visibility = Visibility.Hidden;
            btnCancel.Visibility = Visibility.Hidden;
        }

        private void btnCredit_Click(object sender, RoutedEventArgs e)
        {
            if (User.Salary < 40000)
            {
                MessageBox.Show("На данный момент Банк отклоняет Вашу заявку.");
                return;
            }
            else { MessageBox.Show("Вам предлагается Кредит на 100 000 руб. на 24 месяца под 20% годовых"); }
            btnConfirm.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            gridNewAccount.Visibility = Visibility.Hidden;
            flag = true;
        }

        private void btnDebit_Click(object sender, RoutedEventArgs e)
        {
            User.AddNewAccount(User,
                               new DebitAccount(""),
                               LogInData.GetLastAccountNumber(new DebitAccount("")),
                               0, 0, new TimeSpan(0));
            MessageBox.Show("Новый счет успешно открыт, он уже в вашем списке счетов!");
            gridNewAccount.Visibility = Visibility.Hidden;
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            gridTransfer.Visibility = Visibility.Visible;
            btnTransfer.Visibility = Visibility.Hidden;
            btnNewAccount.Visibility = Visibility.Hidden;
        }

        private void btnConfirmTransfer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BankAccount bA = cbxAccountToTransfer.SelectedItem as BankAccount;
                uint r = bA.AccountNumber;
                LogInData.ChangeMoneyAmount(User, r,
                                            Convert.ToUInt32(tbxTransferAccount.Text),
                                            Convert.ToUInt32(tbxTransferSummary.Text));
                MessageBox.Show("Перевод осуществлен");
            }
            catch
            {
                MessageBox.Show("Не выбран счет списания. Или неправильно введены данные.");
            }
            gridTransfer.Visibility = Visibility.Hidden;
            btnTransfer.Visibility = Visibility.Visible;
            btnNewAccount.Visibility = Visibility.Visible;
        }

        private void btnLeaveWindow_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow aW = new AuthorizationWindow();
            aW.Show();
            this.Close();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            var item = radioButton.DataContext;
            lbxOffers.SelectedItem = item;
            index = lbxOffers.SelectedIndex;
            offer = lbxOffers.SelectedItem as BankAccountType;
            btnAllowOffer.Visibility = Visibility.Visible;
            btnDeclineOffer.Visibility = Visibility.Visible;
        }

        private void btnAllowOffer_Click(object sender, RoutedEventArgs e)
        {
            btnDeclineOffer.Visibility = Visibility.Hidden;
            btnAllowOffer.Visibility = Visibility.Hidden;
            tbkAmount.Visibility = Visibility.Visible;
            tbxAmount.Visibility = Visibility.Visible;
            btnAmountConfirm.Visibility = Visibility.Visible;
        }

        private void btnAmountConfirm_Click(object sender, RoutedEventArgs e)
        {
            BankAccountType type = offer;
            User.AddNewAccount(User,
                               type,
                               LogInData.GetLastAccountNumber(type),
                               Convert.ToUInt64(tbxAmount.Text),
                               Convert.ToByte(type.GetInterestRate(type)),
                               new TimeSpan(Convert.ToInt32(type.GetPaymentPeriod(type))*30,0,0,0));
            Offer.ChangeOfferStatus("accept", idAll[index]);
            offers.Remove(offer);
            idAll.RemoveAt(index);
            lbxOffers.ItemsSource = null;
            lbxOffers.ItemsSource = offers;
            tbkAmount.Visibility = Visibility.Hidden;
            tbxAmount.Visibility = Visibility.Hidden;
            btnAmountConfirm.Visibility = Visibility.Hidden;
        }

        private void TabItem_GotFocus(object sender, EventArgs e)
        {
            if (offers != null)
            {
                lbxOffers.ItemsSource = offers;
                lbxOffers.Visibility = Visibility.Visible;
            }
        }

        private void btnDeclineOffer_Click(object sender, RoutedEventArgs e)
        {
            Offer.ChangeOfferStatus("decline", idAll[index]);
            offers.Remove(offer);
            idAll.RemoveAt(index);
            lbxOffers.ItemsSource = null;
            lbxOffers.ItemsSource = offers;
            btnAllowOffer.Visibility = Visibility.Hidden;
            btnDeclineOffer.Visibility = Visibility.Hidden;
        }

        private void lbxOffers_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lbxOffers.SelectedItem as BankAccountType != null)
            { offer = lbxOffers.SelectedItem as BankAccountType; }
        }
    }
}
