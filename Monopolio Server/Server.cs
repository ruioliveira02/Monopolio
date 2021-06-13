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
using Monopolio_Server.Interfaces.Requests;
using Monopolio_Server.Interfaces.Responses;
using Monopolio;
using System.IO;

namespace Monopolio_Server
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
        private const string ip = "127.0.0.1";

        /// <summary>
        /// The port being used
        /// </summary>
        private const int port = 25565;

        /// <summary>
        /// The size of the buffer used to read bytes from the network stream
        /// </summary>
        public static readonly int BufferSize = 65536;


        private Thread inputThread;

        /// <summary>
        /// The board to be used in the new game
        /// </summary>
        private Board board;

        /// <summary>
        /// The game state
        /// </summary>
        public State State { get; private set; }

        /// <summary>
        /// Server with no game running (new game)
        /// </summary>
        public Server(Board b)
        {
            board = b;
            inputThread = new Thread(Input);
        }

        /// <summary>
        /// Server with a game state (load game)
        /// </summary>
        /// <param name="s">The loaded state</param>
        public Server(State s)
        {
            board = s.board;
            State = s;
            inputThread = new Thread(Input);
        }

        /// <summary>
        /// Opens and runs the server until closure
        /// </summary>
        public void Run()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Parse(ip), port);
            TcpClient clientSocket = new TcpClient();

            serverSocket.Start();
            Console.WriteLine("Monopolio Server Started ....");
            inputThread.Start();

            while (Running)
            {
                IdentRequest request = GetRequest(ref serverSocket, ref clientSocket);

                Console.WriteLine(request.Message());
                Response response = request.Execute();
                Console.WriteLine(response.Message());

                if (request.Accepted)
                {
                    AddClient(request, response, ref clientSocket);
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
        }

        #region input

        /// <summary>
        /// The method running the keyboard input
        /// </summary>
        private void Input()
        {
            while (Running)
            {
                string s = Console.ReadLine();
                int split = s.IndexOf(' ');
                string op = s.Substring(0, split < 1 ? s.Length : split);
                string args = s.Substring(split == s.Length - 1 ? split : split + 1);

                if (s == "exit" || s == "close" || s == "stop")
                    break;
                else if (s == "start")
                {
                    if (State == null)
                        Console.WriteLine("The game has already started");
                    else
                    {
                        string[] players = new string[ClientsList.Count];
                        int i = 0;

                        foreach (var k in ClientsList.Keys)
                            players[i++] = (string)k;

                        State = new State(board, players);
                        State.Start();
                    }
                }
                else if (State == null)
                    Console.WriteLine("The game has't started yet");
                else if (split < 1 || split + 1 == s.Length)
                    Console.WriteLine("Invalid instruction");
                else if (op == "save")
                {
                    string file = args + ".json";
                    if (File.Exists(file))
                    {
                        Console.WriteLine("File \"" + file + "\" already exists. Overwite? (y/n)");
                        if (Console.ReadLine().ToLower() == "y")
                            State.Save(file);
                        else
                            Console.WriteLine("Operation cancelled");
                    }
                    else
                        State.Save(file);
                }
                else
                {
                    try
                    {
                        Monopolio.Action a = new Monopolio.Action(State, op, args);
                        if (!State.Execute(a))
                            Console.WriteLine("Action failed");
                    }
                    catch (ArgumentException)
                    {
                        try
                        {
                            Event e = new Event(args);
                            Player p = State.GetPlayer(op);
                            if (p != null)
                                State.Execute(e, p);
                            else
                                Console.WriteLine("Player not recognized");
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Invalid instruction");
                        }
                    }
                }
            }

            if (Running)
            {
                //TODO: Stop server
                Running = false;
            }
        }

        #endregion

        /// <summary>
        /// Broadcasts a server response
        /// </summary>
        public static void Broadcast(Response msg)
        {
            foreach (DictionaryEntry Item in ClientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();

                byte[] broadcastBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg));

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }

        /// <summary>
        /// Gets the latest connection request from the socket.
        /// </summary>
        /// <paramref name="serverSocket"/> The socket the is listening on
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