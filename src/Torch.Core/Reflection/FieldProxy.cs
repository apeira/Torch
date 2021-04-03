using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Torch.Core.Reflection
{
    public class FieldProxy
    {
        protected static readonly Dictionary<string, FieldProxy> Cache = new Dictionary<string, FieldProxy>();

        /// <summary>
        /// Clears the internal cache of dynamically generated field accessors.
        /// </summary>
        public static void ClearCache() => Cache.Clear();

        protected FieldProxy() { }
    }

    /// <summary>
    /// Provides a method of obtaining a reference to an otherwise inaccessible field.
    /// </summary>
    /// <typeparam name="TValue">Field type</typeparam>
    public class FieldProxy<TValue> : FieldProxy
    {
        private readonly GetByRefDel _accessor;
        private readonly bool _isStatic;
        private readonly bool _isReadOnly;

        private delegate ref TValue GetByRefDel(object? instance);

        /// <summary>
        /// Get or create a shadow accessor for a private field of the given type.
        /// </summary>
        /// <param name="typeName">The assembly qualified name of the type containing the field.</param>
        /// <param name="fieldName">The name of the field to shadow.</param>
        /// <returns>A shadow accessor for the field.</returns>
        public static FieldProxy<TValue> Of(string typeName, string fieldName) => Of(Type.GetType(typeName), fieldName);

        /// <summary>
        /// Get or create a shadow accessor for a private field of the given type.
        /// </summary>
        /// <typeparam name="TType">The type containing the field.</typeparam>
        /// <param name="fieldName">The name of the field to shadow.</param>
        /// <returns>A shadow accessor for the field.</returns>
        public static FieldProxy<TValue> Of<TType>(string fieldName) => Of(typeof(TType), fieldName);

        /// <summary>
        /// Get or create a shadow accessor for the private field of the given type.
        /// </summary>
        /// <param name="t">The type containing the field.</param>
        /// <param name="fieldName">The name of the field to shadow.</param>
        /// <returns>A shadow accessor for the field.</returns>
        public static FieldProxy<TValue> Of(Type t, string fieldName)
        {
            var methodName = $"{t.FullName}.{fieldName}_byRefProxy";

            if (Cache.TryGetValue(methodName, out var shadow))
                return (FieldProxy<TValue>)shadow;

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            var field = t.GetField(fieldName, flags);

            if (field == null)
                throw new InvalidOperationException("Field not found on the given type.");

            // Memory may not be accessed safely if the field is treated as one of a different type.
            if (field.FieldType != typeof(TValue))
                throw new InvalidOperationException("Field type does not match ShadowField type.");

            shadow = new FieldProxy<TValue>(field, methodName);
            Cache.Add(methodName, shadow);

            return (FieldProxy<TValue>)shadow;
        }

        /// <summary>
        /// Obtains a read-only reference to the static field this shadow represents.
        /// </summary>
        /// <returns>A read-only reference to a static field.</returns>
        public ref readonly TValue GetValue() => ref GetValue(null);

        /// <summary>
        /// Obtains a read-only reference to the field of the given instance.
        /// </summary>
        /// <param name="instance">An instance of the object containing the field.</param>
        /// <returns>A read-only reference to the instance of the field.</returns>
        /// <exception cref="ArgumentException">The instance parameter has a value when it shouldn't or vice versa.</exception>
        public ref readonly TValue GetValue(object? instance)
        {
            if (!_isStatic && instance == null)
                throw new NullReferenceException("Accessing an instance field requires an instance.");

            return ref _accessor(instance);
        }

        /// <summary>
        /// Obtains a reference to the static field this shadow represents.
        /// </summary>
        /// <returns>A reference to a static field.</returns>
        public ref TValue GetRef() => ref GetRef(null);

        /// <summary>
        /// Obtains a reference to the field of the given instance.
        /// </summary>
        /// <param name="instance">An instance of the object containing the field.</param>
        /// <returns>A reference to the instance of the field.</returns>
        /// <exception cref="ArgumentException">The instance parameter has a value when it shouldn't or vice versa.</exception>
        public ref TValue GetRef(object? instance)
        {
            if (_isReadOnly)
                throw new InvalidOperationException($"The field is read-only. Use {nameof(GetValue)} instead of ${nameof(GetRef)}.");

            if (!_isStatic && instance == null)
                throw new NullReferenceException("Accessing an instance field requires an instance.");

            return ref _accessor(instance);
        }

        private FieldProxy(FieldInfo f, string name)
        {
            _isStatic = f.IsStatic;
            _isReadOnly = f.IsInitOnly;
            _accessor = CreateAccessor(f, name);
        }

        /// <summary>
        /// Creates a delegate to a dynamic method that returns a reference to the private field of a given object.
        /// </summary>
        private static GetByRefDel CreateAccessor(FieldInfo f, string name)
        {
            var dm = new DynamicMethod(name, typeof(TValue), new[] { typeof(object) }, f.DeclaringType);
            dm.ConvertToRefReturn();

            var il = dm.GetILGenerator();
            if (f.IsStatic)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, f.DeclaringType);
            }

            il.Emit(OpCodes.Ldflda, f);
            il.Emit(OpCodes.Ret);

            return (GetByRefDel)dm.CreateDelegate(typeof(GetByRefDel));
        }
    }

    internal static class DynamicMethodExtensions
    {
        private const BindingFlags BF = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        private static readonly FieldInfo _dynamicMethodReturnType = typeof(DynamicMethod).GetField("m_returnType", BF);

        /// <summary>
        /// Workaround that converts a DynamicMethod that returns T to one that returns ref T.
        /// </summary>
        public static void ConvertToRefReturn(this DynamicMethod m)
        {
            _dynamicMethodReturnType.SetValue(m, m.ReturnType.MakeByRefType());
        }
    }
}
