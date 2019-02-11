using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    internal class MdpWorker
    {
        public MdpWorker(string service, byte[] address, DateTime now)
        {
            Service = service;
            Address = address;
            LastHeartbeat = now;
            LastUsage = DateTime.MinValue;
        }

        public byte[] Address { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public string Service { get; }
        public DateTime LastUsage { get; set; }
    }
}
