using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyMapper.Test.Spec
{
    class T1
    {
        public T2 T2 { get; set; }

        public int Int { get; set; }
    }

    class T2
    {
        public T1 T1 { get; set; }

        public int Int { get; set; }
    }
}
