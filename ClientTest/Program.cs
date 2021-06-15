using Monopolio_Server.Interfaces.Requests;
using Monopolio_Server.Interfaces.Responses;
using Network;
using NetworkModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest
{
    class AlternativeChatRequest : Request, IChatRequest
    {
        public string Msg { get; set; }

        public override Response Execute()
        {
            throw new NotImplementedException();
        }

        public override string Message()
        {
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Send(TcpClient socket, Request r)
        {
            NetworkStream s = socket.GetStream();
            string inter = JsonConvert.SerializeObject(r);
            Console.WriteLine(inter);
            byte[] outStream = Encoding.UTF8.GetBytes(inter);

            s.Write(outStream, 0, outStream.Length);
            s.Flush();
        }

        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient();
            clientSocket.Connect("127.0.0.1", 25565);

            IdentRequest r = new IdentRequest();
            r.SenderID = "Bace";

            Send(clientSocket, r);

            while (true)
            {
                AlternativeChatRequest chat = new AlternativeChatRequest();
                chat.Msg = Console.ReadLine();
                Send(clientSocket, chat);
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
