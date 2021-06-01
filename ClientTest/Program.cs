using Network;
using NetworkModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.button2_Click();
            p.button1_Click();
            Console.ReadLine();
        }

        TcpClient clientSocket  = new TcpClient();
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
            //clientSocket.Connect("2.80.236.204", 25565);
            clientSocket.Connect("127.0.0.1", 25565);
            serverStream = clientSocket.GetStream();

            IdentRequest request = new IdentRequest("membro");

            byte[] outStream = Encoding.ASCII.GetBytes(request.ToJSON());
            serverStream.Write(outStream, 0, outStream.Length);
            //serverStream.Flush();

            Thread ctThread = new Thread(GetMessage);
            ctThread.Start();
        }

        private void GetMessage()
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[100025];
                buffSize = clientSocket.ReceiveBufferSize;
                serverStream.Read(inStream, 0, buffSize);
                string returndata = Encoding.ASCII.GetString(inStream);
                if(returndata != null && returndata != "")
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                        TypeNameHandling = TypeNameHandling.All
                    };
                    returndata = returndata.Replace("MonopolioServer", "ClientTest").Replace("Monopolio Server", "ClientTest");
                    Response response = JsonConvert.DeserializeObject(returndata, settings) as Response;
                    response.Execute();
                }
            }
        }
    }
}
