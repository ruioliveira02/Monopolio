﻿using System;
using System.Collections.Generic;
using System.Text;
using Monopolio;

namespace NetworkModel.Responses
{
    public interface ICardDrawResponse : IResponse
    {
        //player?

        //só título e texto?
        Card Card { get; set; }
    }
}
