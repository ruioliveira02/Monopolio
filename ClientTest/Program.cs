using Monopolio_Server.Interfaces.Requests;
using Monopolio_Server.Interfaces.Responses;
using Network;
using NetworkModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
        static TcpClient socket = new TcpClient();

        static JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.All
            };

        static void Send(Request r)
        {
            NetworkStream s = socket.GetStream();
            string inter = JsonConvert.SerializeObject(r, JsonSettings);
            byte[] outStream = Encoding.UTF8.GetBytes(inter);

            Console.WriteLine(r.Message());

            s.Write(outStream, 0, outStream.Length);
            s.Flush();
        }

        static void Listen()
        {
            byte[] bytesFrom = new byte[65536];

            while (socket.Connected)
            {
                NetworkStream networkStream = socket.GetStream();
                int read = networkStream.Read(bytesFrom, 0, socket.ReceiveBufferSize);

                string data = Encoding.UTF8.GetString(bytesFrom, 0, read);
                //data = data.Replace("Monopolio_Server", "ClientTest"); //no need since
                                                        //we're using the server's classes
                Response r = JsonConvert.DeserializeObject(data, JsonSettings) as Response;

                Console.WriteLine(r.Message());
            }
        }

        static void Main(string[] args)
        {
            Console.Write("Name: ");
            IdentRequest r = new IdentRequest();
            r.SenderID = Console.ReadLine();

            socket.Connect("192.168.1.98", 25565);
            new Thread(Listen).Start();
            Send(r);

            while (true)
            {
                ChatRequest chat = new ChatRequest();
                chat.Msg = Console.ReadLine();
                Send(chat);
            }







            /*
            Program p = new Program();
            p.button2_Click();
            p.button1_Click();
            Console.ReadLine();
            */
            }

        System.Net.Sockets.TcpClient clientSocket  = new System.Net.Sockets.TcpClient();
        NetworkStream serverStream  = default(NetworkStream);
        string readData = null;


        public void button1_Click()
        {
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes("ola$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }

        public void button2_Click()
        {
            readData = "Conected to Chat Server ...";
            clientSocket.Connect("2.80.236.204", 25565);
            serverStream = clientSocket.GetStream();

            byte[] outStream = System.Text.Encoding.ASCII.GetBytes("ola$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            Thread ctThread = new Thread(getMessage);
            ctThread.Start();
        }

        private void getMessage()
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[100025];
                buffSize = clientSocket.ReceiveBufferSize;
                serverStream.Read(inStream, 0, buffSize);
                string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                readData = "" + returndata;
                Console.WriteLine(readData);
            }
        }
    }
}
