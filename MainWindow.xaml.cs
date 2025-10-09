using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
using unitechRFID;
using unitechRFID.Diagnositics;
using unitechRFID.Params;
using unitechRFID.Reader;
using unitechRFID.Transport;
using unitechRFID.UHF.Params;
using RJCP.IO.Ports;
using unitechRFID.Util.Diagnotics;
using unitechRFIDSample.ViewModels;
using System.Text.RegularExpressions;

namespace unitechRFIDSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private void NumberChecker(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsNumber(e.Text);
        }

        public bool IsNumber(string data)
        {
            Regex regex = new Regex("[^0-9]+");
            return regex.IsMatch(data);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((MainViewModel)DataContext).Dispose();
        }
    }
}
