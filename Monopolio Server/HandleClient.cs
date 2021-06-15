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

            while (Server.Running && Server.ClientsList.ContainsKey(ClNo))
            {
                try
                {
                    NetworkStream networkStream = ClientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, ClientSocket.ReceiveBufferSize);

                    if (!ClientSocket.Connected) //client disconnected
                        break;

                    string dataFromClient = Encoding.UTF8.GetString(bytesFrom);

                    Request request = JsonConvert.DeserializeObject<Request>(dataFromClient, new RequestConverter());
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
            {
                ClientSocket.Close();
                Server.ClientsList.Remove(ClNo);
            }

            Server.Log(string.Format("{0} disconnected", ClNo));
        }
    }

    #region jsonRequestConverter

    public class RequestConverter : JsonConverter
    {
        static JsonSerializer Serializer = new JsonSerializer();
        static Type[] types = Assembly.GetAssembly(typeof(Request)).GetTypes().Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(typeof(Request))).ToArray();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            foreach (Type t in types.Where(TheType => TheType == objectType || TheType.IsSubclassOf(objectType)))
            {
                bool fits = true;

                foreach (Type i in t.GetInterfaces())
                {
                    foreach (var pi in i.GetProperties())
                    {
                        if (!jObject.ContainsKey(pi.Name))
                        {
                            fits = false;
                            break;
                        }
                    }

                    if (!fits)
                        break;
                }

                if (fits)
                    return Serializer.Deserialize(jObject.CreateReader(), t);
            }

            throw new ArgumentException("No Request derived class match was found for the JSON object");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Request) == objectType || typeof(Request).IsAssignableFrom(objectType);
        }

        public override bool CanWrite { get => false; }
    }

    #endregion
}