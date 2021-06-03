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

namespace Monopolio_Server
{
    public class Server
    {
        /*private ServerConnectionContainer secureServerConnectionContainer;
        const int Port = 8080;

        internal void Demo()
        {
            //1. Start to listen on a port
            secureServerConnectionContainer = ConnectionFactory.CreateSecureServerConnectionContainer(Port);
            //2. Apply optional settings.

            #region Optional settings

            secureServerConnectionContainer.ConnectionLost += (a, b, c) => Console.WriteLine($"{secureServerConnectionContainer.Count} {b.ToString()} Connection lost {a.IPRemoteEndPoint.Port}. Reason {c.ToString()}");
            secureServerConnectionContainer.ConnectionEstablished += ConnectionEstablished;
#if NET46
            secureServerConnectionContainer.AllowBluetoothConnections = true;
#endif
            secureServerConnectionContainer.AllowUDPConnections = true;
            secureServerConnectionContainer.UDPConnectionLimit = 20;

            #endregion Optional settings
            //Call start here, because we had to enable the bluetooth property at first.
            secureServerConnectionContainer.Start();

            //Don't close the application.
            Console.ReadLine();
        }

        /// <summary>
        /// We got a connection.
        /// </summary>
        /// <param name="connection">The connection we got. (TCP or UDP)</param>
        private void ConnectionEstablished(Connection connection, ConnectionType type)
        {
            Console.WriteLine($"{secureServerConnectionContainer.Count} {connection.GetType()} connected on port {connection.IPRemoteEndPoint.Port}");

            //3. Register packet listeners.
            //connection.RegisterStaticPacketHandler<IdentRequest>(CalculationReceived);
            //connection.RegisterStaticPacketHandler<string>(addStudentReceived);
            //connection.RegisterRawDataHandler("HelloWorld", (rawData, con) => Console.WriteLine($"RawDataPacket received. Data: {rawData.ToUTF8String()}"));
            //connection.RegisterRawDataHandler("BoolValue", (rawData, con) => Console.WriteLine($"RawDataPacket received. Data: {rawData.ToBoolean()}"));
            //connection.RegisterRawDataHandler("DoubleValue", (rawData, con) => Console.WriteLine($"RawDataPacket received. Data: {rawData.ToDouble()}"));
        }

        /// <summary>
        /// If the client sends us a calculation request, it will end up here.
        /// </summary>
        /// <param name="packet">The calculation packet.</param>
        /// <param name="connection">The connection who was responsible for the transmission.</param>
        private static void CalculationReceived(IdentRequest packet, Connection connection)
        {
            Console.WriteLine(string.Format("Incoming Ident Request from user {0}", packet.Username));
            //TODO:: Modify this line to accomodate server logic
            bool accepted = true;
            Console.WriteLine(((accepted) ? "User accepted" : "User denied"));

            connection.Send(new IdentResponse(accepted, packet));
        }*/

        public static Hashtable clientsList = new Hashtable();

            public void Chat()
            {
                TcpListener serverSocket = new TcpListener(IPAddress.Parse("192.168.1.98"), 25565);
                TcpClient clientSocket = default;
                int counter = 0;

                serverSocket.Start();
                Console.WriteLine("Chat Server Started ....");
                counter = 0;
                while ((true))
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();

                    byte[] bytesFrom = new byte[100025];
                    string dataFromClient = null;

                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                    clientsList.Add(dataFromClient, clientSocket);

                    broadcast(dataFromClient + " Joined ", dataFromClient, false);

                    Console.WriteLine(dataFromClient + " Joined chat room ");
                    handleClinet client = new handleClinet();
                    client.startClient(clientSocket, dataFromClient, clientsList);
                }

                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine("exit");
                Console.ReadLine();
            }

            public static void broadcast(string msg, string uName, bool flag)
            {
                foreach (DictionaryEntry Item in clientsList)
                {
                    TcpClient broadcastSocket;
                    broadcastSocket = (TcpClient)Item.Value;
                    NetworkStream broadcastStream = broadcastSocket.GetStream();
                    Byte[] broadcastBytes = null;

                    if (flag == true)
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg);
                    }
                    else
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(msg);
                    }

                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                }
            }  //end broadcast function
        }//end Main class


        public class handleClinet
        {
            TcpClient clientSocket;
            string clNo;
            Hashtable clientsList;

            public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
            {
                this.clientSocket = inClientSocket;
                this.clNo = clineNo;
                this.clientsList = cList;
                Thread ctThread = new Thread(doChat);
                ctThread.Start();
            }

            private void doChat()
            {
                int requestCount = 0;
                byte[] bytesFrom = new byte[100025];
                string dataFromClient = null;
                Byte[] sendBytes = null;
                string serverResponse = null;
                string rCount = null;
                requestCount = 0;

                while ((true))
                {
                    try
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = clientSocket.GetStream();
                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                        Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
                        rCount = Convert.ToString(requestCount);
                    Console.WriteLine(dataFromClient);
                        Server.broadcast(dataFromClient, clNo, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }//end while
            }//end doChat

}
}
