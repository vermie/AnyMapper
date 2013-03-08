using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnyMapper.Tests.Unit
{
    [TestClass]
    public class PropertyMapperTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            Mapper.DebugFlush();
        }

        public class T1
        {
            public int Property { get; set; }
            public T2 T2 { get; set; }
        }

        public class T2
        {
            public int Property { get; set; }
            public T1 T1 { get; set; }
        }

        [TestMethod]
        public void MemberPrimitivesAreMapped()
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();
            typeMapper.Property(t => t.Property).MapsTo(t => t.Property);

            var t1 = new T1() { Property = 1 };
            var t2 = Mapper.Map<T1, T2>(t1);

            Assert.AreEqual(t1.Property, t2.Property);
        }

        [TestMethod]
        public void MemberObjectsAreMapped()
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();
            typeMapper.Property(t => t.Property).MapsTo(t => t.Property);
            typeMapper.Property(t => t.T2).MapsTo(t => t.T1);

            var t1 = new T1() { T2 = new T2() { Property = 1 } };
            var t2 = Mapper.Map<T1, T2>(t1);

            Assert.IsNotNull(t2.T1);
            Assert.AreEqual(t1.T2.Property, t2.T1.Property);

            var t3 = Mapper.Map<T2, T1>(t2);
            Assert.IsNotNull(t3);
            Assert.IsNotNull(t2.T1);
            Assert.AreEqual(t1.T2.Property, t3.T2.Property);
        }

        [TestMethod]
        public void MemberObjectsAreMappedBothWays()
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();
            typeMapper.Property(t => t.Property).MapsTo(t => t.Property);
            typeMapper.Property(t => t.T2).MapsTo(t => t.T1);

            var t1 = new T1() { T2 = new T2() { Property = 1 } };
            var t2 = Mapper.Map<T1, T2>(t1);
            var t3 = Mapper.Map<T2, T1>(t2);

            Assert.IsNotNull(t3);
            Assert.IsNotNull(t2.T1);
            Assert.AreEqual(t1.T2.Property, t3.T2.Property);
        }

        [TestMethod]
        public void OriginalObjectsAreNotDestroyed()
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();
            typeMapper.Property(t => t.Property).MapsTo(t => t.Property);

            var t1 = new T1();
            var t2 = new T2();
            var originalT2 = t2;
            Mapper.Map<T1, T2>(t1, t2);

            Assert.AreSame(originalT2, t2);
        }

        [TestMethod]
        public void OriginalObjectsAreMerged()
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();
            typeMapper.Property(t => t.Property).MapsTo(t => t.Property);

            var t1 = new T1() { Property = 1 };
            var t2 = new T2();
            var originalT2 = t2;
            Mapper.Map<T1, T2>(t1, t2);

            Assert.AreEqual(t1.Property, originalT2.Property);
        }
    }
}
