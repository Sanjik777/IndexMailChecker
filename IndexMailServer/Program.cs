using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace IndexMailServer
{
	class Program
	{		
		static void Main(string[] args)
		{
			//заготовим данные для json файла
			PrepareJsonData();

			//Создадим сервер
			TcpListener socketServer;			
			int port = 12345;
			string ipAddress = "0.0.0.0";
			socketServer = new TcpListener(IPAddress.Parse(ipAddress), port);
			socketServer.Start(100);
			while (true)
			{
				//ждем клиента
				IAsyncResult iAsyncResult = socketServer.BeginAcceptTcpClient(AcceptClientProc, socketServer);
				//ожидание завершения асинхронного соединения со стороны клиента
				iAsyncResult.AsyncWaitHandle.WaitOne();			
			}
		}
		
		static void AcceptClientProc(IAsyncResult iARes)
		{
			TcpListener socketServer = (TcpListener)iARes.AsyncState;
			TcpClient client = socketServer.EndAcceptTcpClient(iARes);//(мы дождались клиента)сокет для обмена данными
			Console.WriteLine($"Клиент прибыл: {client.Client.RemoteEndPoint.ToString()}");
			ThreadPool.QueueUserWorkItem(ClientThreadProc,client);			
		}
		static void ClientThreadProc(object obj)
		{
			TcpClient client = (TcpClient)obj;
			//Считываем данные json файл
			List<Street> jsonList = JsonConvert.DeserializeObject<List<Street>>(File.ReadAllText("address.json"));

			byte[] recBuf = new byte[4 * 1024];
			try
			{
				while (true)
				{
					if (client.Client.Connected == false)
					{
						Console.WriteLine("Клиент отключен");
						break;
					}
					int recSize = client.Client.Receive(recBuf);
					string index = Encoding.UTF8.GetString(recBuf, 0, recSize);
					Console.WriteLine(index);
					string sendMessage = null;
					for (int i = 0; i < jsonList.Count; i++)
					{
						if (index == jsonList[i].Index)
						{
							for (int j = 0; j < jsonList[i].StreetName.Count; j++)
							{
								Console.WriteLine($"Адреса вашего индекса({jsonList[i].Index}): {jsonList[i].StreetName[j]}");
								sendMessage += $"Адреса вашего индекса({jsonList[i].Index}): {jsonList[i].StreetName[j]}\n";
							}
							client.Client.Send(Encoding.UTF8.GetBytes(sendMessage));
						}
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("Ошибка: "+exception);
			}
			
			client.Client.Shutdown(SocketShutdown.Both);
			client.Close();
		}
		#region PrepareJsonData()
		static void PrepareJsonData()
		{
			//заготовим данные для json файла
			List<Street> streets = new List<Street>
			{
				new Street
				{
					Index ="01000",
					StreetName =new List<string>()
					{ "Moldagulova 21","Kenesary 2","Pavlova","Zhumanova"}
				},
				new Street
				{
					Index ="01001",
					StreetName =new List<string>()
					{ "Cheruminskaya","Nagornaya","Ivanova 73","Pushkina 8"}
				},
				new Street
				{
					Index ="01002",
					StreetName =new List<string>()
					{ "Moskovskaya","Kuishidina","Zodiakova","Beketova"}
				}
			};
			// serialize JSON to a string and then write string to a file
			File.WriteAllText("address.json", JsonConvert.SerializeObject(streets));
		}
		#endregion
	}
}
