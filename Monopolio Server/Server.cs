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
using System.Collections.Generic;

namespace Monopolio_Server
{
    /// <summary>
    /// Class which handles the server logic, mainly accepting new connections from clients
    /// </summary>
    internal static class Server
    {
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
        public const int BufferSize = 65536;

        /// <summary>
        /// The maximum number of players the server will accept
        /// </summary>
        public const int MaxClients = 10;

        /// <summary>
        /// The name displayed as the "ID" of the server
        /// </summary>
        public const string ServerName = "SERVER";

        /// <summary>
        /// Whether the server is still running or not
        /// </summary>
        public static bool Running;

        /// <summary>
        /// The server socket
        /// </summary>
        private static TcpListener ServerSocket { get; set; }

        /// <summary>
        /// The table with all the currently connected clients, hashed by the username
        /// </summary>
        public static Dictionary<string, TcpClient> ClientsList { get; }

        /// <summary>
        /// The board to be used in the new game
        /// </summary>
        private static Board board;

        /// <summary>
        /// The game state
        /// </summary>
        public static State State { get; private set; }

        static Server()
        {
            ClientsList = new Dictionary<string, TcpClient>();
        }

        #region input

        /// <summary>
        /// The method running the keyboard input
        /// </summary>
        private static void Input()
        {
            while (Running)
            {
                if (RunCommand(Console.ReadLine()))
                    Log("Command successfully run");
                else
                    Log("Command failed to run");
            }
        }

        public static bool RunCommand(string command)
        {
            int split = command.IndexOf(' ');
            string op = command.Substring(0, split < 1 ? command.Length : split);
            string args = command.Substring(split == command.Length - 1 ? split : split + 1);

            if (command == "exit" || command == "close" || command == "stop")
            {
                Running = false;
                ServerSocket.Stop();
            }
            else if (command == "start")
            {
                if (State != null)
                    return false; //The game has already started
                else
                {
                    string[] players = new string[ClientsList.Count];
                    int i = 0;

                    foreach (string k in ClientsList.Keys)
                        players[i++] = k;

                    lock (State)
                    {
                        State = new State(board, players);
                        SetHandlers();
                        State.Start();
                    }
                }
            }
            else if (op == "say")
            {
                ChatResponse chat = new ChatResponse(ServerName, args);
                Log(chat.Message());
                Broadcast(chat);
            }
            else if (split < 1 || split + 1 == command.Length)
                return false; //Invalid instruction
            else if (op == "kick")
                return Kick(args);
            else if (State == null)
                return false; //The game has't started yet
            else if (op == "save")
            {
                string file = args + ".json";
                if (File.Exists(file))
                {
                    Console.WriteLine("File \"" + file + "\" already exists. Overwite? (y/n)");
                    if (Console.ReadLine().ToLower() == "y")
                        lock (State)
                            State.Save(file);
                    else
                        Console.WriteLine("Operation cancelled");
                }
                else
                    lock (State)
                        State.Save(file);
            }
            else
            {
                try
                {
                    Monopolio.Action a = new Monopolio.Action(State, op, args);
                    lock (State)
                        if (!State.Execute(a))
                            return false; //Action failed
                }
                catch (ArgumentException)
                {
                    try
                    {
                        Event e = new Event(args);
                        Player p = State.GetPlayer(op);
                        if (p != null)
                            lock (State)
                                State.Execute(e, p);
                        else
                            return false; //Player not recognized
                    }
                    catch (ArgumentException)
                    {
                        return false; //Invalid instruction
                    }
                }
            }

            return true;
        }

        #endregion

        #region run

        /// <summary>
        /// Runs the server with no game running (new game)
        /// </summary>
        /// <param name="s">The board</param>
        /// <returns>False if the server was already running. True otherwise</returns>
        public static bool Run(Board b)
        {
            if (Running)
                return false;

            board = b;
            Run();
            return true;
        }

        /// <summary>
        /// Runs the server with a game state (load game)
        /// </summary>
        /// <param name="s">The loaded state</param>
        /// <returns>False if the server was already running. True otherwise</returns>
        public static bool Run(State s)
        {
            if (Running)
                return false;

            board = s.board;
            State = s;
            SetHandlers();
            Run();
            return true;
        }

        /// <summary>
        /// Opens and runs the server until closure
        /// </summary>
        private static void Run()
        {
            Running = true;
            ServerSocket = new TcpListener(IPAddress.Parse(ip), port);

            ServerSocket.Start();
            Log("Monopolio Server Started ....");

            Thread inputThread = new Thread(Input);
            inputThread.Start();

            while (Running)
            {
                TcpClient clientSocket = null;
                IdentRequest request = GetRequest(ServerSocket, ref clientSocket);

                if (request == null)
                    continue;

                Log(request.Message());
                Response response = request.Execute();
                Log(response.Message());

                if (request.Accepted)
                    AddClient(request, response, clientSocket);
            }

            ServerSocket.Stop();
        }

        #region handlers

        private static void SetHandlers()
        {
            State.CardDrawHandler = new State.CardDraw((Card c, int deck) =>
            {
                Response r = new CardDrawResponse(c);
                Log(r.Message());
                Broadcast(r);

                //Thread.Sleep(5000);
            });
            State.DiceThrowHandler = new State.DiceThrow((int[] dice) =>
            {
                Response r = new DiceThrowResponse(dice);
                Log(r.Message());
                Broadcast(r);

                //Thread.Sleep(2000);
            });
            State.MiddleMoneyUpdateHandler = new State.MiddleMoneyUpdate((int middleMoney) =>
            {
                Response r = new MiddleMoneyResponse(middleMoney);
                Log(r.Message());
                Broadcast(r);
            });
            State.PlayerUpdateHandler = new State.PlayerUpdate((Player p) =>
            {
                Response r = new PlayerUpdateResponse(p);
                Log(r.Message());
                Broadcast(r);
            });
            State.PropertyUpdateHandler = new State.PropertyUpdate((PropertyState ps) =>
            {
                Response r = new PropertyUpdateResponse(ps);
                Log(r.Message());
                Broadcast(r);
            });
            State.TurnUpdateHandler = new State.TurnUpdate((int turn) =>
            {
                Response r = new TurnUpdateResponse(turn);
                Log(r.Message());
                Broadcast(r);
            });
        }

        #endregion

        #endregion

        #region log

        /// <summary>
        /// Logs the given line to the console
        /// </summary>
        /// <param name="line">The line to be logged</param>
        public static void Log(string line)
        {
            Console.WriteLine("[{0}]{1}", DateTime.Now.ToString("yy-MM-dd HH:mm:ss"), line);
            //TODO: log to file ??
        }

        /// <summary>
        /// Logs the error sent from the specified sender
        /// </summary>
        /// <param name="sender">The sender ID</param>
        /// <param name="ex">The error</param>
        public static void LogErr(string sender, Exception ex)
        {
            Log(string.Format("ERROR: {0} threw {1}: {2}",
                       sender, ex.GetType(), ex.Message));
        }

        #endregion

        public static bool Kick(string clientID)
        {
            if (!ClientsList.ContainsKey(clientID))
                return false;

            ClientsList[clientID].Close();
            ClientsList.Remove(clientID);
            return true;
        }

        /// <summary>
        /// Gets the permission level of the client
        /// </summary>
        /// <param name="clientID">The ID (name) of the client</param>
        /// <returns>True if the client has command perms</returns>
        public static bool IsOp(string clientID)
        {
            return clientID == "Bace";
        }

        /// <summary>
        /// Broadcasts a server response
        /// </summary>
        public static void Broadcast(Response msg)
        {
            foreach (TcpClient socket in ClientsList.Values)
            {
                NetworkStream broadcastStream = socket.GetStream();

                byte[] broadcastBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

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
        private static IdentRequest GetRequest(TcpListener serverSocket, ref TcpClient clientSocket)
        {
            while (Running)
            {
                try
                {
                    clientSocket = serverSocket.AcceptTcpClient();

                    byte[] bytesFrom = new byte[BufferSize];

                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                    string dataFromClient = Encoding.UTF8.GetString(bytesFrom);

                    return JsonConvert.DeserializeObject<IdentRequest>(dataFromClient);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10004)
                        break;  //server closed
                    else
                        LogErr(ServerName, ex);
                }
                catch (Exception ex)
                {
                    LogErr(ServerName, ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the requested client to the list of clients
        /// </summary>
        /// <param name="request">The client's request</param>
        /// <param name="response">The response of the server</param>
        ///
        /// <paramref name="socket"> The socket the is client listening on</paramref>
        /// 
        private static void AddClient(Request request, Response response, TcpClient socket)
        {
            ClientsList.Add(request.SenderID, socket);
            Broadcast(response);
            HandleClient client = new HandleClient();
            client.StartClient(socket, request.SenderID);
        }
    }
}