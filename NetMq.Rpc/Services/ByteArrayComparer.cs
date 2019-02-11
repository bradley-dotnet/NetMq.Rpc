using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Services
{
    class ByteArrayComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] x, byte[] y)
        {
            return x?.SequenceEqual(y) ?? y == null;
        }

        public override int GetHashCode(byte[] obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (obj.Length >= 4)
            {
                return BitConverter.ToInt32(obj, 0);
            }
            int value = obj.Length;
            foreach (var b in obj)
            {
                value <<= 8;
                value += b;
            }
            return value;
        }
    }
}
