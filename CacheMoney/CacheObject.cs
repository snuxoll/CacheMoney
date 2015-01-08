using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheMoney
{
    public class CacheObject
    {
        public CacheObject(object o)
        {
            Created = DateTime.Now;
            Object = o;
        }

        public DateTime Created { get; private set; }

        public object Object { get; private set; }
    }
}
