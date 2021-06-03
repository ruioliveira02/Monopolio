using Network;
using Network.Enums;
using System;
using NetworkModel;
using Network.Extensions;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace MonopolioServer
{
    /// <summary>
    /// Class which handles the server logic, mainly accepting new connections from clients
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Whether the server is still running or not
        /// </summary>
        public static bool Running = true;

        /// <summary>
        /// The table with all the currently connected clients, hashed by the username
        /// </summary>
        public static Hashtable ClientsList = new Hashtable();

        /// <summary>
        /// The ip of the server we are using
        /// </summary>
        private const string ip = "127.0.0.1";//"2.80.236.204";

        /// <summary>
        /// The port being used
        /// </summary>
        private const int port = 25565;

        /// <summary>
        /// The size of the buffer used to read bytes from the network stream
        /// </summary>
        public static readonly int BufferSize = 65536;

        /// <summary>
        /// Opens and runs the server until closure
        /// </summary>
        public void Run()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Parse(ip), port);
            TcpClient clientSocket = new TcpClient();

            serverSocket.Start();
            Console.WriteLine("Monopolio Server Started ....");

            while (Running)
            {
                IdentRequest request = GetRequest(ref serverSocket, ref clientSocket);

                Console.WriteLine(request.Message());
                Response response = request.Execute() as Response;
                Console.WriteLine(response.Message());

                if (request.Accepted)
                {
                    AddClient(request, response, ref clientSocket);
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();
        }

        /// <summary>
        /// Broadcasts a server response
        /// </summary>
        public static void Broadcast(Response msg)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.All
            };
            byte[] broadcastBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg, settings));

            foreach (DictionaryEntry Item in ClientsList)
            {
                TcpClient broadcastSocket = Item.Value as TcpClient;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
           

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }

        /// <summary>
        /// Gets the latest connection request from the socket.
        /// </summary>
        /// <paramref name="serverSocket"/> The socket the server is listening on
        /// <paramref name="clientSocket"/> The socket the client is listening on
        /// 
        /// <returns>The lastest <cref>IdentRequest</cref> from the socket</returns>
        private IdentRequest GetRequest(ref TcpListener serverSocket, ref TcpClient clientSocket)
        {
            clientSocket = serverSocket.AcceptTcpClient();

            byte[] bytesFrom = new byte[BufferSize];

            NetworkStream networkStream = clientSocket.GetStream();
            networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
            string dataFromClient = Encoding.ASCII.GetString(bytesFrom);

            return JsonConvert.DeserializeObject<IdentRequest>(dataFromClient);
        }

        /// <summary>
        /// Adds the requested client to the list of clients
        /// </summary>
        /// <param name="request">The client's request</param>
        /// <param name="response">The response of the server</param>
        ///
        /// <paramref name="socket"> The socket the is client listening on</paramref>
        /// 
        private void AddClient(Request request, Response response, ref TcpClient socket)
        {
            ClientsList.Add(request.SenderID, socket);
            Broadcast(response);
            HandleClient client = new HandleClient();
            client.StartClient(socket, request.SenderID);
        }
    }     
}