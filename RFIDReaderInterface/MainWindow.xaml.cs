using System.IO.Ports;
using System.Windows;

namespace RFIDReaderInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serialPort = new();

        private List<string> comPortsName = [];
        private string RFIDTagID = "";

        public MainWindow()
        {
            InitializeComponent();

            serialPort.BaudRate = 9600;
            serialPort.DataBits = 8;

            serialPort.DataReceived += ReceiveMessage;

            CbCommPort.IsEnabled = false;
            BtnConnect.IsEnabled = false;
            BtnDisconnect.IsEnabled = false;
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                CbCommPort.IsEnabled = false;
                BtnConnect.IsEnabled = false;
                BtnDisconnect.IsEnabled = true;
                BtnRefreshPorts.IsEnabled = false;
                MessageBox.Show("COM port is already open.", "Error");
                return;
            }

            if (CbCommPort.SelectedIndex == -1)
            {
                CbCommPort.IsEnabled = true;
                BtnConnect.IsEnabled = true;
                BtnDisconnect.IsEnabled = false;
                BtnRefreshPorts.IsEnabled = true;

                MessageBox.Show("No COM port was selected.\nPlease select COM port from combo box.", "Warning");
                return;
            }

            try
            {
                serialPort.PortName = CbCommPort.SelectedItem.ToString();
                serialPort.Open();

                CbCommPort.IsEnabled = false;
                BtnConnect.IsEnabled = false;
                BtnDisconnect.IsEnabled = true;
                BtnRefreshPorts.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error StackTrace: " + ex.StackTrace + "\nError Message: " + ex.Message, "Critical Error");
            }
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            CbCommPort.IsEnabled = true;
            BtnConnect.IsEnabled = true;
            BtnDisconnect.IsEnabled = false;
            BtnRefreshPorts.IsEnabled = true;

            serialPort.Close();
        }

        private void BtnRefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            comPortsName.Clear();

            foreach (string portName in SerialPort.GetPortNames())
            {
                comPortsName.Add(portName);
            }

            if (comPortsName.Count > 0)
            {
                CbCommPort.ItemsSource = comPortsName;
                CbCommPort.SelectedIndex = 0;
                CbCommPort.IsEnabled = true;
                CbCommPort.Items.Refresh();

                BtnConnect.IsEnabled = true;
                BtnDisconnect.IsEnabled = false;
            }
            else
            {
                CbCommPort.Items.Clear();
                CbCommPort.IsEnabled = false;
                CbCommPort.Items.Refresh();

                BtnConnect.IsEnabled = false;
                BtnDisconnect.IsEnabled = false;

                MessageBox.Show("No com port was found.\nPlease check if device is connect.", "Message");
            }
        }

        private void BtnCopyID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RFIDTagID);
        }

        private void BtnDeleteID_Click(object sender, RoutedEventArgs e)
        {
            RFIDTagID = "";
            TbID.Text = "";
        }

        private void ReceiveMessage(object sender, EventArgs e)
        {
            Thread.Sleep(500);

            SerialPort sp = (SerialPort)sender;
            RFIDTagID = "";

            int byteToRead = sp.BytesToRead;

            bool decOrHexSettings = false;

            Dispatcher.Invoke(() => decOrHexSettings = RbSettingsDec.IsChecked == true);

            for (int i = 0; i < byteToRead; i++)
            {
                int actualByte = sp.ReadByte();

                if (actualByte != 2 && actualByte != 3)
                {
                    if (decOrHexSettings)
                    {
                        RFIDTagID += actualByte.ToString();
                    }
                    else
                    {
                        RFIDTagID += actualByte.ToString("X2");
                    }

                }
            }

            Dispatcher.Invoke(() => TbID.Text = RFIDTagID);
            Dispatcher.Invoke(() => Clipboard.SetText(RFIDTagID));
        }   
    }
}