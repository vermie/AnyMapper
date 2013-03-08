using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyMapper
{
    public static partial class Mapper
    {
        public static IFluentTypeMapper<T1, T2> CreateMapper<T1, T2>()
            where T1 : new()
            where T2 : new()
        {
            return Mapper.InternalCreateMapper<T1, T2>();
        }
    }
}
