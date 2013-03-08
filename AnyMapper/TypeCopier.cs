using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace AnyMapper
{
    internal static class TypeCopierHelper
    {
        public static void Copy<T>(T source, ref T destination)
        {
            if (!typeof(T).IsSimpleUnderlyingType())
                throw new ArgumentException("must be a simple type, as determined by IsSimpleType, or a nullable with a simple underlying type", "typeparam T");

            destination = source;
        }

        public static ITypeMapper<T> Create<T>()
        {
            return new TypeCopier<T>();
        }

        public static ITypeMapper<T1, T2> Create<T1, T2>()
        {
            return new ConvertingTypeCopier<T1, T2>();
        }
    }

    internal interface ITypeMapper<T>
    {
        T Map(T source);
        void Map(T source, ref T destination);
    }

    internal abstract class TypeMapperBase<T> : ITypeMapper<T>
    {
        public T Map(T source)
        {
            T destination = default(T);
            Map(source, ref destination);
            return destination;
        }

        public abstract void Map(T source, ref T destination);
    }

    internal class TypeCopier<T> : TypeMapperBase<T>
    {
        public override void Map(T source, ref T destination)
        {
            TypeCopierHelper.Copy(source, ref destination);
        }
    }
}
