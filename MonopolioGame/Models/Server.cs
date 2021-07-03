using MonopolioGame.Interfaces.Requests;
using MonopolioGame.Interfaces.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonopolioGame.Models
{
    public class Server
    {
        public event EventHandler<Response> NewResponseEvent;
        TcpClient clientSocket;
        NetworkStream? serverStream;
        Thread ctThread;

        static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            TypeNameHandling = TypeNameHandling.All
        };

        public Server()
        {
            clientSocket = new TcpClient();
            ctThread = new Thread(GetResponses);
        }

        public bool Connect(string ip, int port)
        {
            try
            {
                clientSocket.Connect(ip, port);
                serverStream = clientSocket.GetStream();
            }
            catch(Exception)
            {
                return false;
            }

            ctThread.Start();
            return true;
        }

        public void Disconnect() => clientSocket.Close();

        public bool Send(Request request)
        {
            byte[] outStream = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request, Settings));
            try
            {
                serverStream = clientSocket.GetStream();
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        private void GetResponses()
        {
            bool over = false;
            const int bufferSize = 65536;
            while (!over && clientSocket.Connected)
            {
                try
                {
                    serverStream = clientSocket.GetStream();
                    byte[] inStream = new byte[bufferSize];
                    int buffSize = clientSocket.ReceiveBufferSize;
                    int read = serverStream.Read(inStream, 0, buffSize);
                    if (read == 0)
                        continue;

                    string data = Encoding.UTF8.GetString(inStream, 0, read);

                    if (data != null && data != "")
                        over = !ProcessData(data);
                }
                catch (Exception)
                {
                    over = true;
                }
            }

            if (clientSocket.Connected)
                clientSocket.Close();
        }

        private bool ProcessData(string data)
        {
            //Change namespaces in JSON of response
            //If this isn't done the Json API will not recognize the types
            data = data.Replace("Monopolio_Server", "MonopolioGame").Replace("Monopolio Server", "MonopolioGame");
            try
            {
                if (JsonConvert.DeserializeObject(data, Settings) is Response response)
                    NewResponseEvent(this, response);
            }
            catch (JsonException)
            {
                return false;
                //TODO:: Add Notification to UI
            }
            return true;
        }
    }
}
