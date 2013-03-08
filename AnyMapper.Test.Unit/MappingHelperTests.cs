using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnyMapper;

namespace HM.PNC.Auto.Data.Prefill.Test.Unit.Mapping
{
    [TestClass]
    public class TypeCopierTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            Mapper.DebugFlush();
        }

        #region Direct copy

        #region Signed integers

        [TestMethod]
        public void Int8Copier()
        {
            sbyte source = 1, destination = 0;
            TypeCopierHelper.Create<sbyte>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void Int16Copier()
        {
            short source = 1, destination = 0;
            TypeCopierHelper.Create<short>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void Int32Copier()
        {
            int source = 1, destination = 0;
            TypeCopierHelper.Create<int>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void Int64Copier()
        {
            long source = 1, destination = 0;
            TypeCopierHelper.Create<long>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        #endregion

        #region Unsigned integers

        [TestMethod]
        public void UInt8Copier()
        {
            byte source = 1, destination = 0;
            TypeCopierHelper.Create<byte>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void UInt16Copier()
        {
            ushort source = 1, destination = 0;
            TypeCopierHelper.Create<ushort>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void UInt32Copier()
        {
            uint source = 1, destination = 0;
            TypeCopierHelper.Create<uint>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void UInt64Copier()
        {
            ulong source = 1, destination = 0;
            TypeCopierHelper.Create<ulong>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        #endregion

        #region Floating point

        [TestMethod]
        public void SingleCopier()
        {
            float source = 1, destination = 0;
            TypeCopierHelper.Create<float>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void DoubleCopier()
        {
            double source = 1, destination = 0;
            TypeCopierHelper.Create<double>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void DecimalCopier()
        {
            decimal source = 1, destination = 0;
            TypeCopierHelper.Create<decimal>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        #endregion

        #region Date (DateTime, TimeSpan, DateTimeOffset)

        [TestMethod]
        public void DateTimeCopier()
        {
            DateTime source = DateTime.Now, destination = default(DateTime);
            TypeCopierHelper.Create<DateTime>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void TimeSpanCopier()
        {
            TimeSpan source = DateTime.Now.TimeOfDay, destination = default(TimeSpan);
            TypeCopierHelper.Create<TimeSpan>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void DateTimeOffsetCopier()
        {
            DateTimeOffset source = DateTimeOffset.Now, destination = default(DateTimeOffset);
            TypeCopierHelper.Create<DateTimeOffset>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        #endregion

        #region Nullable

        [TestMethod]
        public void NullableCopier()
        {
            DateTime? source = DateTime.Now, destination = null;
            TypeCopierHelper.Create<DateTime?>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        #endregion

        #region String

        [TestMethod]
        public void StringCopier()
        {
            string source = "foo", destination = null;
            TypeCopierHelper.Create<string>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        #endregion

        #region Guid

        [TestMethod]
        public void GuidCopier()
        {
            Guid source = Guid.NewGuid(), destination = default(Guid);
            TypeCopierHelper.Create<Guid>().Map(source, ref destination);

            Assert.AreEqual(source, destination);
        }

        #endregion

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void NonSimpleCopierThrows()
        {
            object source = new object(), destination = null;
            TypeCopierHelper.Create<object>().Map(source, ref destination);
        }

        #endregion

        #region Simple casts/conversions

        [TestMethod]
        public void WideningConversion()
        {
            int source = 1, destination2 = 0;
            long destination = 0;

            var copier = TypeCopierHelper.Create<int, long>();
            copier.Map(source, ref destination);
            copier.Map(destination, ref destination2);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void NarrowingConversion()
        {
            long source = 1, destination2 = 0;
            int destination = 0;

            var copier = TypeCopierHelper.Create<long, int>();
            copier.Map(source, ref destination);
            copier.Map(destination, ref destination2);

            Assert.AreEqual(source, destination);
        }

        [TestMethod]
        public void IntToFloatConversion()
        {
            int source = 1, destination2 = 0;
            float destination = 0;

            var copier = TypeCopierHelper.Create<int, float>();
            copier.Map(source, ref destination);
            copier.Map(destination, ref destination2);

            Assert.AreEqual(source, destination);
            Assert.AreEqual(source, destination2);
        }

        [TestMethod]
        public void FloatToIntConversion()
        {
            float source = 1.1f, destination2 = 0;
            int destination = 0;

            var copier = TypeCopierHelper.Create<float, int>();
            copier.Map(source, ref destination);
            copier.Map(destination, ref destination2);

            Assert.AreEqual((int)source, destination);
            Assert.AreEqual((int)source, destination2);
        }

        #endregion
    }
}
