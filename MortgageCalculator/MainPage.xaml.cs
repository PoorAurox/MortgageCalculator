using Microsoft.Maui.Animations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MortgageCalculator
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            OnLoanAmountChanged(null, null);
        }
        private async void OnLoanAmountChanged(object sender, EventArgs e)
        {
            if(double.TryParse(InterestRate.Text, out var rate) & int.TryParse(LoanAmount.Text, out var loan)) 
            {
                var payment = CalculateMonthlyPayment(loan, rate) + ((loan / .8) * double.Parse(TaxRate.Text) / 100 / 12) + double.Parse(HOAFee.Text);
                MonthlyPayment.TextChanged -= OnMonthlyPaymentChanged;
                MonthlyPayment.Text = payment.ToString("F");
                MonthlyPayment.TextChanged += OnMonthlyPaymentChanged;
            }
        }
        private async void OnMonthlyPaymentChanged(object sender, EventArgs e)
        {
            if ((e as TextChangedEventArgs)?.OldTextValue is null) return;
            
            if(InterestRateCheckBox.IsChecked & double.TryParse(MonthlyPayment.Text, out var payment) & double.TryParse(LoanAmount.Text, out var loan))
            {
                UpdateInterest(payment, loan);
            }
            else if(double.TryParse(MonthlyPayment.Text, out payment) & double.TryParse(InterestRate.Text, out var rate))
            {
                rate = rate / 100 / 12;
                var numerator = payment * (Math.Pow(1 + rate, 360) - 1);
                var denominator = rate * Math.Pow(1 + rate, 360);
                LoanAmount.TextChanged -= OnLoanAmountChanged;
                LoanAmount.Text = (numerator / denominator).ToString("F");
                LoanAmount.TextChanged += OnLoanAmountChanged;
            }
        }
        private void UpdateInterest(double payment, double loan)
        {
            var tax = ((loan / .8) * double.Parse(TaxRate.Text) / 100) / 12;
            payment = payment - tax - double.Parse(HOAFee.Text);
            var lowestPayment = MinimumMonthlyPayment(loan);
            var maxPayment = CalculateMonthlyPayment(loan, 100);
            if (lowestPayment > payment)
            {
                return;
            }

            if (lowestPayment == payment)
            {
                InterestRate.TextChanged -= OnLoanAmountChanged;
                InterestRate.Text = "0.00";
                InterestRate.TextChanged += OnLoanAmountChanged;
                return;
            }
            if (maxPayment < payment)
            {
                return;
            }
            if (maxPayment == payment)
            {
                InterestRate.TextChanged -= OnLoanAmountChanged;
                InterestRate.Text = "100";
                InterestRate.TextChanged += OnLoanAmountChanged;
                return;
            }
            var rate = CalculateInterestRate(loan, payment);
            InterestRate.TextChanged -= OnLoanAmountChanged;
            InterestRate.Text = rate.ToString("0.0000");
            InterestRate.TextChanged += OnLoanAmountChanged;
        }
        private double CalculateMonthlyPayment(double loan, double rate, int paymentPeriods = 360)
        {
            rate = rate / 100 / 12;
            var numerator = rate * loan * Math.Pow((1 + rate), paymentPeriods);
            var denominator = (Math.Pow(1 + rate, paymentPeriods) - 1);
            return numerator / denominator;
        }
        private double CalculateInterestRate(double loan, double payment, int paymentPeriods = 360)
        {


            double lowest = 0;
            double rate = 8;
            double highest = 100;

            for (var calculatedPayment = CalculateMonthlyPayment(loan, rate); Math.Abs(calculatedPayment - payment) > 1; calculatedPayment = CalculateMonthlyPayment(loan, rate))
            {
                if (calculatedPayment < payment)
                {
                    lowest = rate;
                    rate = (rate + highest) / 2;
                }
                if (calculatedPayment > payment)
                {
                    highest = rate;
                    rate = (rate + lowest) / 2;
                }
            }
            return rate;
        }
        private double MinimumMonthlyPayment(double loan, int paymentPeriods = 360)
        {
            return loan / paymentPeriods;
        }

        private async void LoanCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (LoanCheckBox is null | InterestRateCheckBox is null) return;
            if(LoanCheckBox.IsChecked == true)
            {
                InterestRateCheckBox.IsEnabled = false;
            }
            else
            { 
                InterestRateCheckBox.IsEnabled = true;
            }
        }
        private async void InterestRateCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (LoanCheckBox is null | InterestRateCheckBox is null) return;
            if (InterestRateCheckBox.IsChecked == true)
            {
                LoanCheckBox.IsEnabled = false;
            }
            else 
            { 
                LoanCheckBox.IsEnabled = true;
            }
        }
    }
}