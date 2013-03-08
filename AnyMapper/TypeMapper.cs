using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace AnyMapper
{
    public interface IForwardTypeMapper<T1, T2>
    {
        T2 Map(T1 source);
        void Map(T1 source, ref T2 destination);
    }

    public interface IReverseTypeMapper<T1, T2>
    {
        T1 Map(T2 source);
        void Map(T2 source, ref T1 destination);
    }

    public interface ITypeMapper<T1, T2> : IForwardTypeMapper<T1, T2>, IReverseTypeMapper<T1, T2>
    {
    }

    public abstract class TypeMapperBase<T1, T2> : ITypeMapper<T1, T2>
    {
        T2 IForwardTypeMapper<T1, T2>.Map(T1 source)
        {
            T2 destination = default(T2);
            ForwardTypeMap(source, ref destination);
            return destination;
        }

        void IForwardTypeMapper<T1, T2>.Map(T1 source, ref T2 destination)
        {
            ForwardTypeMap(source, ref destination);
        }

        protected abstract void ForwardTypeMap(T1 source, ref T2 destination);

        T1 IReverseTypeMapper<T1, T2>.Map(T2 source)
        {
            T1 destination = default(T1);
            ReverseTypeMap(source, ref destination);
            return destination;
        }

        void IReverseTypeMapper<T1, T2>.Map(T2 source, ref T1 destination)
        {
            ReverseTypeMap(source, ref destination);
        }

        protected abstract void ReverseTypeMap(T2 source, ref T1 destination);
    }

    internal partial class TypeMapper<T1, T2> : TypeMapperBase<T1, T2>
        where T1 : new()
        where T2 : new()
    {
        protected Dictionary<MemberInfo, IMemberMapper<T1, T2>> _propertyMappings;

        public TypeMapper()
        {
            _propertyMappings = new Dictionary<MemberInfo, IMemberMapper<T1, T2>>();
        }

        internal void Add(MemberInfo member, IMemberMapper<T1, T2> mapping)
        {
            _propertyMappings[member] = mapping;
        }

        protected override void ForwardTypeMap(T1 source, ref T2 destination)
        {
            if (source == null)
            {
                destination = default(T2);
                return;
            }

            if (destination == null || object.Equals(destination, default(T2)))
                destination = new T2();

            foreach (var mapping in _propertyMappings.Values)
                mapping.Map(source, destination);
        }

        protected override void ReverseTypeMap(T2 source, ref T1 destination)
        {
            if (source == null)
            {
                destination = default(T1);
                return;
            }

            if (destination == null || object.Equals(destination, default(T1)))
                destination = new T1();

            foreach (var mapping in _propertyMappings.Values)
                mapping.Map(source, destination);
        }
    }

    internal class InverseTypeMapper<T1, T2> : TypeMapperBase<T2, T1>
        where T1 : new()
        where T2 : new()
    {
        IReverseTypeMapper<T1, T2> _reverse;
        IForwardTypeMapper<T1, T2> _forward;

        public InverseTypeMapper(TypeMapper<T1, T2> typeMapper)
        {
            _reverse = (IReverseTypeMapper<T1, T2>)typeMapper;
            _forward = (IForwardTypeMapper<T1, T2>)typeMapper;
        }

        protected override void ForwardTypeMap(T2 source, ref T1 destination)
        {
            _reverse.Map(source, ref destination);
        }

        protected override void ReverseTypeMap(T1 source, ref T2 destination)
        {
            _forward.Map(source, ref destination);
        }
    }

    internal static class TypeMapperExtensions
    {
        public static IForwardTypeMapper<T1, T2> Forward<T1, T2>(this TypeMapper<T1, T2> typeMapper)
            where T1 : new()
            where T2 : new()
        {
            return (IForwardTypeMapper<T1, T2>)typeMapper;
        }

        public static IReverseTypeMapper<T1, T2> Reverse<T1, T2>(this TypeMapper<T1, T2> typeMapper)
            where T1 : new()
            where T2 : new()
        {
            return (IReverseTypeMapper<T1, T2>)typeMapper;
        }

        public static ITypeMapper<T2, T1> Inverse<T1, T2>(this TypeMapper<T1, T2> typeMapper)
            where T1 : new()
            where T2 : new()
        {
            return new InverseTypeMapper<T1, T2>(typeMapper);
        }
    }
}
