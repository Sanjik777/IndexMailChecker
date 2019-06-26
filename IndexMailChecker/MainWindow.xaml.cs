using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IndexMailChecker
{
	/*
	 *Используя класс Socket реализовать клиент-серверное приложение, 
	 * которое позволяет клиенту по почтовому индексу получить список улиц,
	 * соответствующих этому индексу.
	 * Данные об улицах могут хранится в файловой или реляционной базе данных или в xml файле.
	 * Клиент - Windows Forms или WPF приложение, сервер - консольное приложение. 
	 * Запрос клиента выполняется в отдельном (не интерфейсном) потоке, сервер - асинронный.
	 */
	public partial class MainWindow : Window
	{
		string sender;//для textBox через диспатчер
		string result;//вывод результата textBlock через диспатчера
		Thread threadSendReceiveData;
		TcpClient clientSocket;

		public MainWindow()
		{
			InitializeComponent();
			clientSocket = new TcpClient();
		}
		
		private void SearchStreetButtonClick(object sender, RoutedEventArgs e)
		{
			if (!clientSocket.Client.Connected)
			{
				MessageBox.Show("Вы не подключились к серверу!");
			}
			else
			{
				threadSendReceiveData = new Thread(new ThreadStart(StartSendReceivingThrades));
				threadSendReceiveData.IsBackground = true;
				threadSendReceiveData.Start();
			}
		}
		private void ConnectButtonClick(object sender, RoutedEventArgs e)
		{
			int port = 12345;
			string ipServer = "127.0.0.1";
			clientSocket.Connect(ipServer, port);
			if (clientSocket.Client.Connected)
			{
				MessageBox.Show("Сервер подключен. Отправьте индекс (от 01000 до 01002)");
			}
			else { MessageBox.Show("нет сигнала"); }
		}
		void ExtractElement()
		{
			Dispatcher.Invoke(new Action(
				() => { sender = indexTextBox.Text; }
				));
		}
		void ShowTextBlock(string str)
		{
			Dispatcher.Invoke(new Action(
				() => { mailTextBlock.Text = str; }
				));
		}
		private void StartSendReceivingThrades()
		{
			byte[] recBuf = new byte[4 * 1024];
			ExtractElement();
			clientSocket.Client.Send(Encoding.UTF8.GetBytes(sender));
			int recSize = clientSocket.Client.Receive(recBuf);
			result = Encoding.UTF8.GetString(recBuf, 0, recSize);
			MessageBox.Show(result);
			ShowTextBlock(result);
		}
	}
}
