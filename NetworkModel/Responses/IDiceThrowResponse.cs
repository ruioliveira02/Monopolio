using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Responses
{
    interface IDiceThrowResponse : IResponse
    {
        //player?

        int[] Dice { get; set; }
    }
}
