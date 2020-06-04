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
namespace Client
{




    class Client
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

                    master.Send(p.ToBytes());
                    master.Close();
                    return true;
                default:
                    return false;
            }
        }





        public static Socket master;
        public static string id;
        static IPEndPoint ipE;
        static bool flag = true;


        static void Main(string[] args)
        {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            

            Connecect();

            

            
        }

        static void DataIN()
        {
            byte[] Buffer;
            int readBytes;
            while (true)
            {
                try
                {
                    Buffer = new byte[master.SendBufferSize];
                    readBytes = master.Receive(Buffer);


                    if (readBytes > 0)
                    {
                        DataManager(new Packet(Buffer));
                    }
                }
                catch 
                {
                    ConsoleColor cc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Server Disconnected");
                    Console.ForegroundColor = cc;
                    master.Close();
                    Connecect();
                }
            }
        }

        static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case PacketType.Registration:
                    Console.Write(":: >");
                    Console.WriteLine("You Have Registered");
                    Console.Write(":: >");
                    SendMessage();

                    break;
                case PacketType.answer:
                    flag = true;
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(":: >");
                    Console.WriteLine(p.message);
                    Console.ForegroundColor = c;
                    Console.Write(":: >");
                    SendMessage();

                    break;
                case PacketType.busy:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Server Is Busy");
                    Console.ReadLine();
                    Environment.Exit(0);
                    break;
                case PacketType.timeout:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You are timed out");
                    Console.ReadLine();
                    Environment.Exit(0);
                    break;
                case PacketType.dissconnect:
                    ConsoleColor cc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Server Disconnected");
                    Console.ForegroundColor = cc;
                    master.Close();
                    Connecect();
                    break;
                    
            }
        }
        static void SendMessage()
        {
            while (true)
            {


                try
                {
                    
                    string input = Console.ReadLine();
                    Packet p;


                    if (input != "")
                    {
                        p = new Packet(PacketType.answer, input);
                        master.Send(p.ToBytes());
                        
                        break;
                    }


                }
                catch (SocketException ex)
                {
                    ConsoleColor cc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Server Disconnected");
                    Console.ForegroundColor = cc;
                    master.Close();
                    Connecect();
                }
                catch
                {
                    Environment.Exit(0);
                }
            }

            
        }


        static void Connecect()
        {

           
            while (true)
            {
                master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ipE = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
                try
                {
                    master.Connect(ipE);
                    Thread t = new Thread(DataIN);
                    t.Start();
                    break;

                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10061)
                    {
                        ConsoleColor c = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("===============================================");
                        Console.WriteLine("Can not connect to the server");
                        Console.WriteLine("Maybe server is down");
                        Console.WriteLine("reconnect in 2 sec");
                        Console.ForegroundColor = c;
                        Thread.Sleep(2000);



                    }
                    else
                    {
                        Console.WriteLine(ex.ErrorCode);
                        Console.WriteLine(ex.ToString());
                        Thread.Sleep(2000);

                    }

                }
            }
        }
    }
}
