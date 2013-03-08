using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnyMapper;

namespace HM.PNC.Auto.Data.Prefill.Test.Unit.Mapping
{
    [TestClass]
    public class MapperTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            Mapper.DebugFlush();
        }

        public class T1
        {
            public int Property { get; set; }
        }

        public class T2
        {
            public int Property { get; set; }
        }

        [TestMethod]
        public void CreateTypeMapperAndGetTypeMapperReturnSameObject()
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();

            var returnedTypeMapper = Mapper.GetTypeMapper<T1, T2>();
            Assert.AreSame(typeMapper, returnedTypeMapper);
        }
    }
}
