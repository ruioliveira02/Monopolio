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

        public Server()
        {
            clientSocket = new TcpClient();
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

            Thread ctThread = new Thread(GetResponses);
            ctThread.Start();

            return true;
        }

        public bool Send(Request request)
        {
            try
            {
               serverStream = clientSocket.GetStream();
            }
            catch(IOException)
            {
                return false;
            }

            byte[] outStream = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request));
            try
            {
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
            while (!over)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[bufferSize];
                buffSize = clientSocket.ReceiveBufferSize;
                serverStream.Read(inStream, 0, buffSize);
                string data = Encoding.ASCII.GetString(inStream);

                if (data != null && data != "")
                {
                    over = !ProcessData(data);          
                }            
            }
        }

        private bool ProcessData(string data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.All
            };
            //Change namespaces in JSON of response
            //If this isn't done the Json API will not recognize the types
            data = data.Replace("MonopolioServer", "MonopolioGame").Replace("Monopolio Server", "MonopolioGame");
            try
            {
                if (JsonConvert.DeserializeObject(data, settings) is Response response)
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
