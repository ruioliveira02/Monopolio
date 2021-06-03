using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioServer
{
    /// <summary>
    /// Class containing the main.
    /// </summary> 
    class Program
    {
        /// <summary>
        /// The entry point of the application
        /// </summary> 
        /// 
        /// <param name="args">The program's arguments</param>
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Run();
            Console.ReadLine();
        }
    }
}
