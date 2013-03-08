using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnyMapper;

namespace HM.PNC.Auto.Data.Prefill.Test.Unit.Mapping
{
    [TestClass]
    public class CollectionMapperTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            Mapper.DebugFlush();
        }

        public class T1
        {
            public ICollection<int> IntCollection { get; set; }
        }

        public class T2
        {
            public ICollection<int> IntCollection { get; set; }
            public ICollection<long> LongCollection { get; set; }
        }

        [TestMethod]
        public void DestinationCollectionHasSameNumberOfItemsAsSource()
        {
            TypeMapper<T1, T2> mapper = new TypeMapper<T1, T2>();

            CollectionMapper<T1, int, T2, int> collectionMapper =
                new CollectionMapper<T1, int, T2, int>(
                    mapper,
                    t => t.IntCollection, () => new List<int>(), null,
                    t => t.IntCollection, () => new List<int>(), null);

            T1 t1 = new T1() { IntCollection = new[] { 1, 2, 3 } };
            T2 t2 = mapper.Forward().Map(t1);
            T1 t3 = mapper.Reverse().Map(t2);

            Assert.AreEqual(t1.IntCollection.Count, t2.IntCollection.Count);
            Assert.AreEqual(t1.IntCollection.Count, t1.IntCollection.Count);
        }

        [TestMethod]
        public void PrimitiveDestinationCollectionHasSameItemsAsSource()
        {
            TypeMapper<T1, T2> mapper = new TypeMapper<T1, T2>();

            CollectionMapper<T1, int, T2, int> collectionMapper =
                new CollectionMapper<T1, int, T2, int>(
                    mapper,
                    t => t.IntCollection, () => new List<int>(), null,
                    t => t.IntCollection, () => new List<int>(), null);

            T1 t1 = new T1() { IntCollection = new[] { 1, 2, 3 } };
            T2 t2 = mapper.Forward().Map(t1);
            T1 t3 = mapper.Reverse().Map(t2);

            Assert.IsFalse(t1.IntCollection.Except(t2.IntCollection).Any());
            Assert.IsFalse(t2.IntCollection.Except(t3.IntCollection).Any());
        }

        [TestMethod]
        public void ConvertedPrimitiveDestinationCollectionHasEquivalentItemsAsSource()
        {
            TypeMapper<T1, T2> mapper = new TypeMapper<T1, T2>();

            CollectionMapper<T1, int, T2, long> collectionMapper =
                new CollectionMapper<T1, int, T2, long>(
                    mapper,
                    t => t.IntCollection, () => new List<int>(), null,
                    t => t.LongCollection, () => new List<long>(), null);

            T1 t1 = new T1() { IntCollection = new[] { 1, 2, 3 } };
            T2 t2 = mapper.Forward().Map(t1);
            T1 t3 = mapper.Reverse().Map(t2);

            Assert.IsTrue(t1.IntCollection.All(i => t2.LongCollection.Any(l => i == l)));
            Assert.IsTrue(t2.LongCollection.All(i => t3.IntCollection.Any(l => i == l)));
        }

        [TestMethod]
        public void CollectionsAreUnioned()
        {
            var typeMapper = Mapper.CreateMapper<T1, T2>();
            typeMapper.Collection(t => t.IntCollection, () => new List<int>(), null).MapsTo(t => t.LongCollection, () => new List<long>(), null);

            var ints = new[] { 1, 2, 3 };
            var longs = new[] { 1L, 2L, 4L };
            T1 t1 = new T1() { IntCollection = ints };
            T2 t2 = new T2() { LongCollection = longs };
            Mapper.Map(t1, t2);

            var longUnion = longs.Union(ints.Select(i => (long)i));

            Assert.IsTrue(t2.LongCollection.SequenceEqual(longUnion));
        }
    }
}
