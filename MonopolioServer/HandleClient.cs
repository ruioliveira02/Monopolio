using NetworkModel;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonopolioServer
{
    /// <summary>
    /// Class which handles the logic for the client requests after they connected
    /// </summary>
    public class HandleClient
    {
        /// <summary>
        /// The socket the client is listening on
        /// </summary>
        TcpClient ClientSocket { get; set; }

        /// <summary>
        /// The client's name
        /// </summary>
        string ClNo { get; set; }

        /// <summary>
        /// Starts the client communication
        /// </summary>
        /// <param name="inClientSocket">The client's socket</param>
        /// <param name="clientNo">The client's id</param>
        public void StartClient(TcpClient inClientSocket, string clientNo)
        {
            ClientSocket = inClientSocket;
            ClNo = clientNo;
            Thread ctThread = new Thread(Communicate);
            ctThread.Start();
        }

        /// <summary>
        /// Communicates with the client, listening to its requests and responding to them until the server
        /// closes
        /// </summary>
        private void Communicate()
        {
            byte[] bytesFrom = new byte[Server.BufferSize];

            while (Server.Running)
            {
                try
                {
                    NetworkStream networkStream = ClientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, ClientSocket.ReceiveBufferSize);
                    string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);

                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All // To allow derived types
                    };
                    Request request = JsonConvert.DeserializeObject<Request>(dataFromClient, settings);
                    Console.WriteLine(request.Message());
                    Response response = request.Execute() as Response;

                    Console.WriteLine(response.Message());
                    Server.Broadcast(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
