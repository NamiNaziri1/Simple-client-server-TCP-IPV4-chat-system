using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Data;
namespace Server
{
    class Server
    {
        static Socket listenerSocket;
        static List<ClientData> clients;
        static bool flag = false;
        static void Main(string[] args)
        {
            clients = new List<ClientData>();

            
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            listenerSocket.Bind(ip);


            Thread listenThread = new Thread(new ThreadStart(ListenThread));
            listenThread.Start();

        }


        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);


                Socket acc = listenerSocket.Accept();
                if (clients.Count == 4)
                {
                    Packet p = new Packet(PacketType.busy, "");

                    acc.Send(p.ToBytes());
                    acc.Close();
                }
                else
                {
                    clients.Add(new ClientData(acc));
                    Packet p = new Packet(PacketType.Registration, "");
                    acc.Send(p.ToBytes());
                    if (clients.Count > 0)
                    {

                        Console.WriteLine("Client Accepted With port 12345");
                        Console.WriteLine("Number Of Current Client: " + clients.Count);
                    }

                }

            }
        }


        public static void DataIN(object ob)
        {
            ClientData cd = (ClientData)ob;
            Socket clientSocket = cd.clientSocket;

            clientSocket.ReceiveTimeout = 10000;
            byte[] Buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    Buffer = new byte[clientSocket.SendBufferSize];


                    readBytes = clientSocket.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        Packet packet = new Packet(Buffer);
                        DataManager(packet, cd);
                    }
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }
                catch (SocketException ex)
                {

                    if (ex.ErrorCode == 10060)
                    {
                        Console.WriteLine("Client timed out!");
                        Packet p = new Packet(PacketType.timeout, "server");
                        clientSocket.Send(p.ToBytes());
                        clientSocket.Close();
                        clients.Remove(cd);
                        Console.WriteLine("Number Of Current Client: " + clients.Count);
                    }
                    else
                    {
                        Console.WriteLine(ex.ErrorCode);
                        Console.WriteLine("A Client disconnected!");
                        clientSocket.Close();
                        clients.Remove(cd);
                        Console.WriteLine("Number Of Current Client: " + clients.Count);
                    }


                    break;
                }
            }
        }
        public static void DataManager(Packet p, ClientData cd)
        {
            switch (p.packetType)
            {
                
                case PacketType.answer:
                    flag = false;
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(p.message);
                    Console.ForegroundColor = c;
                    Console.Write(":: >");
                    SendMessage(cd);

                    break;

            }
        }

        public static void SendMessage(ClientData cd)
        {
            string input = Console.ReadLine();
            Packet p;

            try
            {
                if (input != "" && !flag)
                {
                    p = new Packet(PacketType.answer, input);
                    cd.clientSocket.Send(p.ToBytes());
                    flag = true;
                }
                else if (!flag == false)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Wait for your answer");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Server has disconnected");
                Console.ReadLine();
                Environment.Exit(0);
            }
            catch
            {
                Environment.Exit(0);
            }
        }

    }



    class ClientData
    {
        public string message;
        public Socket clientSocket;
        public Thread clientThread;


        public ClientData()
        {

            /////////////
            clientThread = new Thread(Server.DataIN);
            clientThread.Start(this);
        }
        public ClientData(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            clientThread = new Thread(Server.DataIN);
            clientThread.Start(this);
        }


    }


}

