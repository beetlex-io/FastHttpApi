using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{
    public enum DataPacketType : byte
    {
        continuation = 0x0,
        text = 0x1,
        binary = 0x2,
        non_control3 = 0x3,
        non_control4 = 0x4,
        non_control5 = 0x5,
        non_control6 = 0x6,
        non_control7 = 0x7,
        connectionClose = 0x8,
        ping = 0x9,
        pong = 0xA,
        controlB = 0xB,
        controlE = 0xE,
        controlF = 0xF
    }
}
