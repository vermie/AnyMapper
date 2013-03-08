using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace AnyMapper
{
    public class CollectionMapperBuilder<T1, T1Property, T2>
        where T1 : new()
        where T2 : new()
    {
        TypeMapper<T1, T2> _typeMapper;

        Expression<Func<T1, ICollection<T1Property>>> _property;
        Expression<Func<ICollection<T1Property>>> _initializer;
        IEqualityComparer<T1Property> _comparer;

        internal CollectionMapperBuilder(TypeMapper<T1, T2> typeMapper, Expression<Func<T1, ICollection<T1Property>>> property, Expression<Func<ICollection<T1Property>>> initializer, IEqualityComparer<T1Property> comparer)
        {
            _typeMapper = typeMapper;

            _property = property;
            _initializer = initializer;
            _comparer = comparer;
        }

        public void MapsTo<T2Property>(Expression<Func<T2, ICollection<T2Property>>> property, Expression<Func<ICollection<T2Property>>> initializer, IEqualityComparer<T2Property> comparer)
        {
            new CollectionMapper<T1, T1Property, T2, T2Property>(_typeMapper, _property, _initializer, _comparer, property, initializer, comparer);
        }
    }
}
