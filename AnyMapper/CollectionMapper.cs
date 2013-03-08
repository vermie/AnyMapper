using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace AnyMapper
{
    internal static class CollectionMappingHelper
    {
        private static readonly MethodInfo _mapCollectionMethod = typeof(CollectionMappingHelper).GetMethod("MapCollection", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _copyCollectionMethod = typeof(CollectionMappingHelper).GetMethod("CopyCollection", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _convertCollectionMethod = typeof(CollectionMappingHelper).GetMethod("ConvertCollection", BindingFlags.NonPublic | BindingFlags.Static);

        private static void MergeCollections<TDestination>(IEnumerable<TDestination> source, ICollection<TDestination> destination, IEqualityComparer<TDestination> comparer)
        {
            if (MappingHelper.IsSimpleUnderlyingType(typeof(TDestination)))
            {
                destination.Clear();
                foreach (var item in source)
                    destination.Add(item);
            }
            else
            {
                // remove from destination items that are no longer in source
                var itemsToRemove = new List<TDestination>();
                foreach (var item in destination)
                    if (!source.Contains(item, comparer))
                        itemsToRemove.Add(item);
                foreach (var item in itemsToRemove)
                    destination.Remove(item);

                // add items to destination which are missing
                foreach (var item in source)
                    if (!destination.Contains(item, comparer))
                        destination.Add(item);
            }
        }

        private static void MapCollection<TSource, TDestination>(ICollection<TSource> source, ICollection<TDestination> destination, IEqualityComparer<TDestination> comparer)
            where TSource : new()
            where TDestination : new()
        {
            var converted = source.Select(i => Mapper.Map<TSource, TDestination>(i));
            MergeCollections(converted, destination, comparer);
        }

        private static void CopyCollection<T>(ICollection<T> source, ICollection<T> destination, IEqualityComparer<T> comparer)
        {
            var converted = source.Select(i => Mapper.InternalCopy<T>(i));
            MergeCollections(converted, destination, comparer);
        }

        private static void ConvertCollection<TSource, TDestination>(ICollection<TSource> source, ICollection<TDestination> destination, IEqualityComparer<TDestination> comparer)
        {
            var converted = source.Select(i => Mapper.InternalConvert<TSource, TDestination>(i));
            MergeCollections(converted, destination, comparer);
        }

        internal static Action<TSource, TDestination> BuildCollectionMappingLambda<TSource, TSourceProperty, TDestination, TDestinationProperty>(MemberExpression property1, MemberExpression property2, Expression<Func<ICollection<TDestinationProperty>>> initializer, IEqualityComparer<TDestinationProperty> comparer)
        {
            var sourcePropertyType = typeof(TSourceProperty);
            var destinationPropertyType = typeof(TDestinationProperty);

            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var destinationParam = Expression.Parameter(typeof(TDestination), "destination");

            var sourceCollection = Expression.Property(sourceParam, (PropertyInfo)property1.Member);
            var destinationCollection = Expression.Property(destinationParam, (PropertyInfo)property2.Member);

            MethodInfo mapCollectionMethod;
            if (sourcePropertyType.IsSimpleType() && sourcePropertyType == destinationPropertyType)
                mapCollectionMethod = _copyCollectionMethod.MakeGenericMethod(destinationPropertyType);
            else if (sourcePropertyType.IsSimpleType() && destinationPropertyType.IsSimpleType())
                mapCollectionMethod = _convertCollectionMethod.MakeGenericMethod(sourcePropertyType, destinationPropertyType);
            else
                mapCollectionMethod = _mapCollectionMethod.MakeGenericMethod(sourcePropertyType, destinationPropertyType);

            var lambda =
                Expression.Lambda<Action<TSource, TDestination>>(
                    Expression.Block(
                        Expression.IfThen(Expression.Equal(destinationCollection, MappingHelper.Null),
                            Expression.Assign(destinationCollection, initializer.Body)),
                        Expression.Call(mapCollectionMethod, sourceCollection, destinationCollection, Expression.Constant(comparer, typeof(IEqualityComparer<TDestinationProperty>)))),
                    sourceParam, destinationParam);

            return lambda.Compile();
        }
    }

    internal class CollectionMapper<T1, T1Property, T2, T2Property> : IMemberMapper<T1, T2>
        where T1 : new()
        where T2 : new()
    {
        MemberExpression _property1;
        Expression<Func<ICollection<T1Property>>> _initializer1;
        IEqualityComparer<T1Property> _comparer1;

        MemberExpression _property2;
        Expression<Func<ICollection<T2Property>>> _initializer2;
        IEqualityComparer<T2Property> _comparer2;

        Lazy<Action<T1, T2>> _oneAsSource;
        Lazy<Action<T2, T1>> _twoAsSource;

        internal CollectionMapper(TypeMapper<T1, T2> typeMapper, Expression<Func<T1, ICollection<T1Property>>> property1, Expression<Func<ICollection<T1Property>>> initializer1, IEqualityComparer<T1Property> comparer1, Expression<Func<T2, ICollection<T2Property>>> property2, Expression<Func<ICollection<T2Property>>> initializer2, IEqualityComparer<T2Property> comparer2)
        {
            _property1 = property1.GetPropertyExpression();
            _initializer1 = initializer1;
            _comparer1 = comparer1;

            _property2 = property2.GetPropertyExpression();
            _initializer2 = initializer2;
            _comparer2 = comparer2;

            _oneAsSource = new Lazy<Action<T1, T2>>(() => CollectionMappingHelper.BuildCollectionMappingLambda<T1, T1Property, T2, T2Property>(_property1, _property2, _initializer2, _comparer2));
            _twoAsSource = new Lazy<Action<T2, T1>>(() => CollectionMappingHelper.BuildCollectionMappingLambda<T2, T2Property, T1, T1Property>(_property2, _property1, _initializer1, _comparer1));

            typeMapper.Add(_property1.Member, this);
        }

        public void Map(T1 source, T2 destination)
        {
            _oneAsSource.Value(source, destination);
        }

        public void Map(T2 source, T1 destination)
        {
            _twoAsSource.Value(source, destination);
        }
    }
}
