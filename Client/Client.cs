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
namespace Client
{
    class Client
    {

        public static Socket master;
        public static string id;

        static bool flag = true;


        static void Main(string[] args)
        {


            master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipE = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);

            try
            {
                master.Connect(ipE);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Thread.Sleep(1000);

            }

            Thread t = new Thread(DataIN);
            t.Start();

            while (true)
            {
                try
                {
                    Console.Write(":: >");
                    string input = Console.ReadLine();
                    Packet p;
                    

                    if (input != "" && flag)
                    {
                        p = new Packet(PacketType.answer, input);
                        master.Send(p.ToBytes());
                        flag = false;
                    }
                    else if(flag == false)
                    {
                        ConsoleColor c = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Wait for your answer");
                        Console.ForegroundColor = c;
                    }

                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
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
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
        }

        static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case PacketType.Registration:
                    SendMassage();
                    break;
                case PacketType.answer:
                    flag = true;
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(p.message);
                    Console.ForegroundColor = c;
                    Console.Write(":: >");
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
                    
            }
        }
        static void SendMessage()
        {

        }

    }
}
