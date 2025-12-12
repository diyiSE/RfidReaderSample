using System.Windows;
using System.Windows.Input;
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

            //<timmy> sets title wtih version
            this.Title += " " + VersionTag.Ver + " Rev [" + VersionTag.Rev + "]";

            DataContext = new MainViewModel();
            
            //<timmy> try to set UI objects
            //((MainViewModel)DataContext).mainWindow = this;
            //PresetListItems();
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

        //:: PS.似乎要在ViewModel控制被xaml Binding的屬性值, 不宜直接在window.xmal.cs中直接控制
        //<timmy>added
        //void PresetListItems()
        //{
        //    //<timmy>
        //    ListRfidTags.Items.Clear();
        //    //lstTags.Items.Add(new ListItem("123"));
        //    ListRfidTags.Items.Add("test1");
        //    //ListRfidTags.Items.Add("2");
        //    //ListRfidTags.Items.Add("3");
        //}


        //<timmy>added
        //public void AddItem(string text)
        //{
        //    RfidTags.Items.Add(text);
        //}

    }
}
