using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace AnyMapper
{
    internal static class MappingHelper
    {
        public static MemberExpression GetPropertyExpression<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null || !(body.Member is PropertyInfo))
                throw new ArgumentException("expression must return a property", "expression");

            return body;
        }

        public static readonly ConstantExpression Null = Expression.Constant(null);

        #region Type helpers

        internal static bool IsNullableValueType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        internal static bool TypeAllowsNullValue(this Type type)
        {
            return !type.IsValueType || IsNullableValueType(type);
        }

        internal static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive ||
                   type.Equals(typeof(string)) ||
                   type.Equals(typeof(DateTime)) ||
                   type.Equals(typeof(Decimal)) ||
                   type.Equals(typeof(Guid)) ||
                   type.Equals(typeof(DateTimeOffset)) ||
                   type.Equals(typeof(TimeSpan));
        }

        internal static bool IsSimpleUnderlyingType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            return MappingHelper.IsSimpleType(type);
        }

        #endregion
    }

    internal static class TypeMapperKey<T1, T2>
        where T1 : new()
        where T2 : new()
    {
        private static TypeMapperKeyHolder _keyHolder;

        public static TypeMapperKeyHolder Initialize()
        {
            if (_keyHolder == null || _keyHolder.Get() == null)
                _keyHolder = new TypeMapperKeyHolder();

            return _keyHolder;
        }

        public static object GetKey()
        {
            return _keyHolder == null
                ? null
                : _keyHolder.Get();
        }
    }

    internal class TypeMapperKeyHolder
    {
        private object _key;

        public TypeMapperKeyHolder()
        {
            _key = new object();
        }

        public object Get()
        {
            return _key;
        }

        public void Invalidate()
        {
            _key = null;
        }
    }

    public partial class Mapper
    {
        #region Type copying

        static Dictionary<Type, object> _typeCopiers = new Dictionary<Type, object>();
        static Dictionary<Type, Dictionary<Type, object>> _typeConverters = new Dictionary<Type, Dictionary<Type, object>>();

        private static TypeCopier<T> GetTypeCopier<T>()
        {
            var typeToCopy = typeof(T);

            object copierAsObject;
            if (!_typeCopiers.TryGetValue(typeToCopy, out copierAsObject))
            {
                copierAsObject = new TypeCopier<T>();
                _typeCopiers[typeToCopy] = copierAsObject;
            }

            return copierAsObject as TypeCopier<T>;
        }

        private static ITypeMapper<TSource, TDestination> GetTypeConverter<TSource, TDestination>()
        {
            var typeToCopy = typeof(TSource);
            var typeToCreate = typeof(TDestination);

            object copierAsObject;
            Dictionary<Type, object> converters;
            if (!_typeConverters.TryGetValue(typeToCopy, out converters))
            {
                converters = new Dictionary<Type, object>();
                _typeConverters[typeToCopy] = converters;
            }

            if (!converters.TryGetValue(typeToCreate, out copierAsObject))
            {
                copierAsObject = TypeCopierHelper.Create<TSource, TDestination>();
                converters[typeToCreate] = copierAsObject;
            }

            return copierAsObject as ITypeMapper<TSource, TDestination>;
        }

        internal static T InternalCopy<T>(T source)
        {
            return GetTypeCopier<T>().Map(source);
        }

        internal static TDestination InternalConvert<TSource, TDestination>(TSource source)
        {
            return GetTypeConverter<TSource, TDestination>().Map(source);
        }

        #endregion

        private static Dictionary<object, object> _typeMappers = new Dictionary<object, object>();
        private static HashSet<TypeMapperKeyHolder> _keyHolders = new HashSet<TypeMapperKeyHolder>();

        internal static ITypeMapper<T1, T2> GetTypeMapper<T1, T2>()
            where T1 : new()
            where T2 : new()
        {
            lock (typeof(Mapper))
            {
                var type1 = typeof(T1);
                var type2 = typeof(T2);

                var key = TypeMapperKey<T1, T2>.GetKey();

                if (key != null)
                    return _typeMappers[key] as ITypeMapper<T1, T2>;

                key = TypeMapperKey<T2, T1>.GetKey();
                if (key != null)
                {
                    var typeMapper = _typeMappers[key] as TypeMapper<T2, T1>;
                    return new InverseTypeMapper<T2, T1>(typeMapper);
                }

                throw new InvalidOperationException(string.Format(@"type ""{0}"" has not been mapped to type ""{1}"".", type1.Name, type2.Name));
            }
        }

        private static TypeMapper<T1, T2> InternalCreateMapper<T1, T2>()
            where T1 : new()
            where T2 : new()
        {
            lock (typeof(Mapper))
            {
                var key = TypeMapperKey<T1, T2>.GetKey();

                if (key != null)
                {
                    var type1 = typeof(T1);
                    var type2 = typeof(T2);
                    throw new InvalidOperationException(string.Format(@"type ""{0}"" has already been mapped to type ""{1}"".", type1.Name, type2.Name));
                }

                var keyHolder = TypeMapperKey<T1, T2>.Initialize();
                _keyHolders.Add(keyHolder);

                var typeMapper = new TypeMapper<T1, T2>();

                key = keyHolder.Get();
                _typeMappers.Add(key, typeMapper);

                return typeMapper;
            }
        }

        public static TDestination Map<TSource, TDestination>(TSource source)
            where TSource : new()
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            var destination = new TDestination();
            Map<TSource, TDestination>(source, destination);
            return destination;
        }

        public static void Map<TSource, TDestination>(TSource source, TDestination destination)
            where TSource : new()
            where TDestination : new()
        {
            if (source == null)
                destination = default(TDestination);
            else
            {
                var typeMapper = GetTypeMapper<TSource, TDestination>();
                typeMapper.Map(source, ref destination);
            }
        }

        internal static void DebugFlush()
        {
            foreach (var keyHolder in _keyHolders)
                keyHolder.Invalidate();

            _keyHolders.Clear();
            _typeMappers.Clear();
        }
    }
}
