using MonopolioGame.Models;
using NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolioGame.Interfaces.Responses
{
    public abstract class Response : IResponse
    {
        public abstract void Execute(GameState game);
    }
}
