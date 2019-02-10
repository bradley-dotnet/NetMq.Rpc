using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    static class NetMqHelper
    {
        public static byte[] EmptyFrame => new byte[0];

        public static byte[] ConvertStringToBytes(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }
    }
}
