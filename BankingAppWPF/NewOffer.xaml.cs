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

namespace BankingAppWPF
{
    /// <summary>
    /// Логика взаимодействия для NewOffer.xaml
    /// </summary>
    public partial class NewOffer : Window
    {

        BankAccountType newOffer;

        Client offerRecipient;

        Offer offers;

        public NewOffer(Client client)
        {
            InitializeComponent();
            offerRecipient = client;
            offers = new Offer();
        }

        private void cbxOfferType_DropDownOpened(object sender, EventArgs e)
        {
            cbxOfferType.ItemsSource = null;
            cbxOfferType.ItemsSource = new ObservableCollection<BankAccountType> { new CreditAccount(""),
                                                                                   new SavingsAccount("") };

        }

        private void cbxOfferType_DropDownClosed(object sender, EventArgs e)
        {
            tbk1.Visibility = Visibility.Visible;
            tbk2.Visibility = Visibility.Visible;
            tbxInterestRate.Visibility = Visibility.Visible;
            tbxPaymentPeriod.Visibility = Visibility.Visible;
            btnConfirm.Visibility = Visibility.Visible;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (cbxOfferType.SelectedItem)
                {
                    case CreditAccount:
                        newOffer = new CreditAccount(Convert.ToByte(tbxInterestRate.Text),
                                                     DateTime.Now,
                                                     new TimeSpan(Convert.ToInt32(tbxPaymentPeriod.Text) * 30, 0, 0, 0));
                        break;
                    case SavingsAccount:
                        newOffer = new SavingsAccount(Convert.ToByte(tbxInterestRate.Text),
                                                     DateTime.Now,
                                                     new TimeSpan(Convert.ToInt32(tbxPaymentPeriod.Text) * 30, 0, 0, 0));
                        break;
                }
                offers.AddNewOffer(newOffer, offerRecipient);
                MessageBox.Show("Предложение успешно отправлено!");
                this.Close();
            }
            catch
            {
                MessageBox.Show("Неправильный формат введенных данных.Перепроверьте введенные данные");
            }
        }

    }
}
