using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace AnyMapper
{
    public class PropertyMapperBuilder<T1, T1Property, T2>
        where T1 : new()
        where T2 : new()
    {
        TypeMapper<T1, T2> _typeMapper;
        Expression<Func<T1, T1Property>> _property;

        internal PropertyMapperBuilder(TypeMapper<T1, T2> typeMapper, Expression<Func<T1, T1Property>> property)
        {
            _typeMapper = typeMapper;
            _property = property;
        }

        public void MapsTo<T2Property>(Expression<Func<T2, T2Property>> property)
        {
            new PropertyMapper<T1, T1Property, T2, T2Property>(_typeMapper, _property, property);
        }

        public void MapsTo<T2Property>(Expression<Func<T2, Nullable<T2Property>>> property)
            where T2Property : struct
        {
            throw new NotImplementedException();
        }
    }
}
