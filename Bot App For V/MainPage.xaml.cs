namespace Bot_App_For_V
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void Slider_ValueChanged(object sender, EventArgs e)
        {
            Slider slider = sender as Slider;
            labValue.Text = $"{(int)(slider.Value)}";
        }
    }
}
