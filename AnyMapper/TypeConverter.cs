using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace AnyMapper
{
    internal delegate void Convert<T1, T2>(T1 source, ref T2 destination);

    internal class ConvertingTypeCopier
    {
        internal static Convert<T1, T2> CreateConverter<T1, T2>()
        {
            var type1 = typeof(T1);
            var type2 = typeof(T2);

            var source = Expression.Parameter(type1, "source");
            var destination = Expression.Parameter(type2.MakeByRefType(), "destination");

            var converter =
                Expression.Lambda<Convert<T1, T2>>(
                    Expression.Assign(destination, Expression.Convert(source, destination.Type)),
                    source, destination).Compile();

            return converter;
        }
    }

    internal class ConvertingTypeCopier<T1, T2> : TypeMapperBase<T1, T2>
    {
        private static readonly Lazy<Convert<T1, T2>> _fromT1 = new Lazy<Convert<T1, T2>>(() => ConvertingTypeCopier.CreateConverter<T1, T2>());
        private static readonly Lazy<Convert<T2, T1>> _fromT2 = new Lazy<Convert<T2, T1>>(() => ConvertingTypeCopier.CreateConverter<T2, T1>());

        protected override void ForwardTypeMap(T1 source, ref T2 destination)
        {
            _fromT1.Value(source, ref destination);
        }

        protected override void ReverseTypeMap(T2 source, ref T1 destination)
        {
            _fromT2.Value(source, ref destination);
        }
    }
}
