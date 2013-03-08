using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace AnyMapper
{
    public interface IFluentTypeMapper<T1, T2>
        where T1 : new()
        where T2 : new()
    {
        CollectionMapperBuilder<T1, T1Property, T2> Collection<T1Property>(Expression<Func<T1, ICollection<T1Property>>> property, Expression<Func<ICollection<T1Property>>> initializer, IEqualityComparer<T1Property> comparer);
        PropertyMapperBuilder<T1, T1Property, T2> Property<T1Property>(Expression<Func<T1, T1Property>> property);
    }

    internal partial class TypeMapper<T1, T2> : IFluentTypeMapper<T1, T2>
    {
        #region Collection mapping

        public CollectionMapperBuilder<T1, T1Property, T2> Collection<T1Property>(Expression<Func<T1, ICollection<T1Property>>> property, Expression<Func<ICollection<T1Property>>> initializer, IEqualityComparer<T1Property> comparer)
        {
            return new CollectionMapperBuilder<T1, T1Property, T2>(this, property, initializer, comparer);
        }

        #endregion

        #region Property mapping

        public PropertyMapperBuilder<T1, T1Property, T2> Property<T1Property>(Expression<Func<T1, T1Property>> property)
        {
            return new PropertyMapperBuilder<T1, T1Property, T2>(this, property);
        }

        #region Nullable mapping

        public PropertyMapperBuilder<T1, T1Property, T2> Property<T1Property>(Expression<Func<T1, Nullable<T1Property>>> property)
            where T1Property : struct
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
