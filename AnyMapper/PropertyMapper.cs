using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace AnyMapper
{
    internal static class PropertyMappingHelper
    {
        static PropertyMappingHelper()
        {
            Expression<Func<object, object>> mapMethodExpression = o => Mapper.Map<object, object>(o);
            _mapMethod = ((MethodCallExpression)mapMethodExpression.Body).Method.GetGenericMethodDefinition();

            Expression<Func<object, object>> copyMethodExpression = o => Mapper.InternalCopy<object>(o);
            _copyMethod = ((MethodCallExpression)copyMethodExpression.Body).Method.GetGenericMethodDefinition();

            Expression<Func<object, object>> convertMethodExpression = o => Mapper.InternalConvert<object, object>(o);
            _convertMethod = ((MethodCallExpression)convertMethodExpression.Body).Method.GetGenericMethodDefinition();
        }

        private static readonly MethodInfo _mapMethod;
        private static readonly MethodInfo _copyMethod;
        private static readonly MethodInfo _convertMethod;

        internal static Action<TSource, TDestination> BuildPropertyMappingLambda<TSource, TSourceProperty, TDestination, TDestinationProperty>(MemberExpression property1, MemberExpression property2)
        {
            var sourcePropertyType = typeof(TSourceProperty);
            var destinationPropertyType = typeof(TDestinationProperty);

            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var destinationParam = Expression.Parameter(typeof(TDestination), "destination");

            var source = Expression.Property(sourceParam, (PropertyInfo)property1.Member);
            var destination = Expression.Property(destinationParam, (PropertyInfo)property2.Member);

            MethodInfo mapMethod;
            if (sourcePropertyType.IsSimpleType() && sourcePropertyType == destinationPropertyType)
                mapMethod = _copyMethod.MakeGenericMethod(destinationPropertyType);
            else if (sourcePropertyType.IsSimpleType() && destinationPropertyType.IsSimpleType())
                mapMethod = _convertMethod.MakeGenericMethod(sourcePropertyType, destinationPropertyType);
            else
                mapMethod = _mapMethod.MakeGenericMethod(sourcePropertyType, destinationPropertyType);

            var lambda =
                Expression.Lambda<Action<TSource, TDestination>>(
                    Expression.Assign(
                        destination,
                        Expression.Call(mapMethod, source)),
                    sourceParam, destinationParam);

            return lambda.Compile();
        }
    }

    internal class PropertyMapper<T1, T1Property, T2, T2Property> : IMemberMapper<T1, T2>
        where T1 : new()
        where T2 : new()
    {
        Lazy<Action<T1, T2>> _oneAsSource;
        Lazy<Action<T2, T1>> _twoAsSource;

        public PropertyMapper(TypeMapper<T1, T2> typeMapper, Expression<Func<T1, T1Property>> property1, Expression<Func<T2, T2Property>> property2)
        {
            var member1 = property1.GetPropertyExpression();
            var member2 = property2.GetPropertyExpression();

            _oneAsSource = new Lazy<Action<T1, T2>>(() => PropertyMappingHelper.BuildPropertyMappingLambda<T1, T1Property, T2, T2Property>(member1, member2));
            _twoAsSource = new Lazy<Action<T2, T1>>(() => PropertyMappingHelper.BuildPropertyMappingLambda<T2, T2Property, T1, T1Property>(member2, member1));

            typeMapper.Add(member1.Member, this);
        }

        public virtual void Map(T1 source, T2 destination)
        {
            _oneAsSource.Value(source, destination);
        }

        public virtual void Map(T2 source, T1 destination)
        {
            _twoAsSource.Value(source, destination);
        }
    }
}
