using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Demo
{
    public class Box<T>
    {
        public T Value { get; set; }
    }

    public interface IDemoContract
    {
        Task<Box<int>> AddAsync(List<int> addends);
    }
}
