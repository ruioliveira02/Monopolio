using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkModel.Responses
{
    public interface IDiceThrowResponse : IResponse
    {
        //player?

        int[] Dice { get; set; }
    }
}
