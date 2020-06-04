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
using System.Runtime.InteropServices;
namespace Server
{


    class Server
    {

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    Packet p = new Packet(PacketType.dissconnect, "");
                    foreach (ClientData cd in clients)
                    {
                        cd.clientSocket.Send(p.ToBytes());
                        cd.clientSocket.Close();
                    }
                    
                    return true;
                default:
                    return false;
            }
        }



        static Socket listenerSocket;
        static List<ClientData> clients;
        static void Main(string[] args)
        {


            /////////////////////////////////////////
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            /////////////////////////////////////////


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
                catch 
                {
                    
                    
                    break;
                }
            }
        }
        public static void DataManager(Packet p, ClientData cd)
        {
            switch (p.packetType)
            {
                
                case PacketType.answer:
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(":: >");
                    Console.WriteLine(p.message);
                    Console.ForegroundColor = c;
                    Console.Write(":: >");
                    SendMessage(cd);

                    break;
                case PacketType.dissconnect:
                    Console.WriteLine("A Client disconnected!");
                    cd.clientSocket.Close();
                    clients.Remove(cd);
                    Console.WriteLine("Number Of Current Client: " + clients.Count);
                    break;
            }
        }

        public static void SendMessage(ClientData cd)
        {
            while (true)
            {
                string input = Console.ReadLine();
                Packet p;

                try
                {

                    if (input != "")
                    {
                        p = new Packet(PacketType.answer, input);
                        cd.clientSocket.Send(p.ToBytes());
                        break;
                    }
                    
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("A Client disconnected!");
                    cd.clientSocket.Close();
                    clients.Remove(cd);
                    Console.WriteLine("Number Of Current Client: " + clients.Count);
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                catch
                {
                    Environment.Exit(0);
                }
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

