using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyMapper
{
    internal interface IMemberMapper<T1, T2>
    {
        void Map(T1 source, T2 destination);
        void Map(T2 source, T1 destination);
    }
}
