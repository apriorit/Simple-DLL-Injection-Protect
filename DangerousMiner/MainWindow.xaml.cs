using System;
using System.Threading.Tasks;
using System.Windows;

namespace DangerousMiner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool isMining = false;
        private async void BtnStartMining(object sender, RoutedEventArgs e)
        {
            if (!isMining)
            {
                isMining = true;
                btnStartMining.Content = "Stop Mining";

                // Start the mining operation in a background task
                await Task.Run(() => Mine());
            }
            else
            {
                isMining = false;
                btnStartMining.Content = "Start Mining";
            }
        }

        private void Mine()
        {
            while (isMining)
            {
                // Simulate a time-consuming operation
                CalculateRandomNumbers();

                // Add a short delay to reduce CPU usage
                System.Threading.Thread.Sleep(10);
            }
        }

        private void CalculateRandomNumbers()
        {
            Random random = new Random();
            int a = random.Next(10000);
            int b = random.Next(10000);
            int result = a * b;
        }
    }
}
