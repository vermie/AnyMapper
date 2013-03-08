using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyMapper.Test.Spec
{
    class ApiTest
    {
        static void Main(string[] args)
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();
            typeMapper.Property(t1 => t1.Int).MapsTo(t2 => t2.Int);
            typeMapper.Property(t1 => t1.T2).MapsTo(t2 => t2.T1);
        }
    }
}
