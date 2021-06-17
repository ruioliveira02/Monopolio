using Monopolio_Server.Interfaces.Requests;
using Monopolio_Server.Interfaces.Responses;
using NetworkModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monopolio_Server
{
    /// <summary>
    /// Class which handles the logic for the client requests after they connected
    /// </summary>
    public class HandleClient
    {
        /// <summary>
        /// The socket the client is listening on
        /// </summary>
        public TcpClient ClientSocket { get; }

        /// <summary>
        /// The client's name
        /// </summary>
        public string ClNo { get; }

        /// <summary>
        /// The client's thread
        /// </summary>
        public Thread ClientThread { get; }

        /// <summary>
        /// Constructs a new HandleClientObject
        /// </summary>
        /// <param name="inClientSocket">The client's socket</param>
        /// <param name="clientNo">The client's name/ID</param>
        public HandleClient(TcpClient inClientSocket, string clientNo)
        {
            ClientSocket = inClientSocket;
            ClNo = clientNo;
            ClientThread = new Thread(Communicate);
        }

        /// <summary>
        /// Starts the client communication
        /// </summary>
        public void Start() => ClientThread.Start();

        /// <summary>
        /// Communicates with the client, listening to its requests and responding to them until the server
        /// closes
        /// </summary>
        private void Communicate()
        {
            byte[] bytesFrom = new byte[Server.BufferSize];

            while (Server.Running && ClientSocket.Connected && Server.ClientsList.ContainsKey(ClNo))
            {
                try
                {
                    NetworkStream networkStream = ClientSocket.GetStream();
                    int read = networkStream.Read(bytesFrom, 0, ClientSocket.ReceiveBufferSize);

                    if (!ClientSocket.Connected || read == 0) //client disconnected
                        break;

                    string dataFromClient = Encoding.UTF8.GetString(bytesFrom, 0, read);
                    dataFromClient = dataFromClient.Replace("MonopolioGame", "Monopolio_Server");

                    Request request = JsonConvert.DeserializeObject(dataFromClient, Server.JsonSettings) as Request;
                    request.SenderID = ClNo;
                    Server.Log(request.Message());
                    Response response = request.Execute();

                    if (response != null)
                    {
                        Server.Log(response.Message());
                        Server.Broadcast(response);
                    }
                }
                catch (IOException)
                {
                    break;  //client disconected
                }
                catch (Exception ex)
                {
                    Server.LogErr(ClNo, ex);
                }
            }

            if (Server.ClientsList.ContainsKey(ClNo))
                lock (Server.ClientsList)
                    Server.ClientsList.Remove(ClNo);

            if (ClientSocket.Connected)
                ClientSocket.Close();

            DisconnectResponse r = new DisconnectResponse(ClNo);
            Server.Log(r.Message());
            Server.Broadcast(r);
        }
    }
}