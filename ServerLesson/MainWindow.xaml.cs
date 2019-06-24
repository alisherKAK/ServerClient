using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerLesson
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ipsComboBox.Items.Add("0.0.0.0");
            ipsComboBox.Items.Add("127.0.0.1");
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                ipsComboBox.Items.Add(ip.ToString());
            }
            ipsComboBox.SelectedIndex = 0;

            startStopButton.Tag = false;
        }

        Thread serverThread;
        TcpListener serverSocket;

        private void StartStopButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(bool)startStopButton.Tag)
            {
                serverSocket = new TcpListener(
                    new IPEndPoint(IPAddress.Parse(ipsComboBox.SelectedItem.ToString()),
                    int.Parse(portTextBox.Text)));
                serverSocket.Start(100);

                serverThread = new Thread(ServerThreadRoutine);
                //serverThread.IsBackground = true;
                serverThread.Start(serverSocket);

                startStopButton.Content = "Stop";
                startStopButton.Tag = true;
            }
            else
            {
                startStopButton.Content = "Start";
                startStopButton.Tag = false;
            }
        }

        bool isStopServer;

        private void ServerThreadRoutine(object socket)
        {
            TcpListener server = socket as TcpListener;

            //while(true)
            //{
            //    TcpClient client = server.AcceptTcpClient();
            //    ThreadPool.QueueUserWorkItem(ClientThreadRoutine, client);
            //}

            while (true)
            {
                IAsyncResult asyncResult = server.BeginAcceptTcpClient(ClientThreadRoutine, server);

                while (asyncResult.AsyncWaitHandle.WaitOne(200) == false)
                {
                    if(isStopServer)
                    {
                        
                    }
                }
            }
        }

        private void ClientThreadRoutine(IAsyncResult ia)
        {
            TcpListener server = ia.AsyncState as TcpListener;
            TcpClient client = server.EndAcceptTcpClient(ia);

            ThreadPool.QueueUserWorkItem(ClientThreadRoutine2, client);
        }

        private void ClientThreadRoutine2(object state)
        {
            TcpClient client = state as TcpClient;

            Dispatcher.Invoke(() => logTextBox.AppendText($"Успешное соединеие клиента {client.Client.RemoteEndPoint.ToString()}\n"));

            byte[] buf = new byte[4*1024];
            int recSize = client.Client.Receive(buf);

            Dispatcher.Invoke(() => logTextBox.AppendText($"Имя клинета: {Encoding.UTF8.GetString(buf, 0, recSize)}\n"));

            string clientName = Encoding.UTF8.GetString(buf, 0, recSize);
            client.Client.Send(Encoding.UTF8.GetBytes("Hello "+clientName));


            while(true)
            {
                recSize = client.Client.Receive(buf);

                Dispatcher.Invoke(() => logTextBox.AppendText($"{Encoding.UTF8.GetString(buf, 0, recSize)}\n"));

                client.Client.Send(buf, recSize, SocketFlags.None);
            }
        }
    }
}
