///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2014 - 2017 XLR8 Development Team                                    /
// ---------------------------------------------------------------------------------- /
//   Licensed under the Apache License, Version 2.0 (the "License");                  /
//   you may not use this file except in compliance with the License.                 /
//   You may obtain a copy of the License at                                          /
//                                                                                    /
//       http://www.apache.org/licenses/LICENSE-2.0                                   /
//                                                                                    /
//   Unless required by applicable law or agreed to in writing, software              /
//   distributed under the License is distributed on an "AS IS" BASIS,                /
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.         /
//   See the License for the specific language governing permissions and              /
//   limitations under the License.                                                   /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XLR8.CGLib
{
    /// <summary>
    /// Provides access to dynamic code reflection; resulting code reflection
    /// will be faster than using reflection, but will result in code being
    /// loaded into the current address space.  In other words, this class
    /// builds a proxy around a class and generates MSIL specifically for
    /// the purpose of exposing functionality.
    /// </summary>

    public sealed class FastClass
    {
        /// <summary>
        /// Static sequential id generation for classes
        /// </summary>
        private static int _fastClassId;

        /// <summary>
        /// Unique identifier assigned to each FastClass upon creation.
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// Type that the FastClass is proxying.
        /// </summary>

        private readonly Type _targetType;

        /// <summary>
        /// Maps methods to FastMethods
        /// </summary>

        private readonly Dictionary<MethodInfo, FastMethod> _methodCache;

        /// <summary>
        /// Maps fields to FastFields
        /// </summary>
        private readonly Dictionary<FieldInfo, FastField> _fieldCache;

        /// <summary>
        /// Maps fields to FastProperty
        /// </summary>
        private readonly Dictionary<PropertyInfo, FastProperty> _propertyCache;

        /// <summary>
        /// Maps constructors to FastConstructors
        /// </summary>
        private readonly Dictionary<ConstructorInfo, FastConstructor> _ctorCache;

        /// <summary>
        /// Internal lock
        /// </summary>

        private readonly Object _instanceLock;

        /// <summary>
        /// Gets the type the FastClass is proxying for.
        /// </summary>
        public Type TargetType
        {
            get { return _targetType; }
        }

        /// <summary>
        /// Maps types to their FastClass implementation.
        /// </summary>
        private static readonly Dictionary<Type, FastClass> FastClassCache;

        /// <summary>
        /// Static lock used for the fastClassCache
        /// </summary>

        private static readonly Object FastClassCacheLock;

        /// <summary>
        /// Initializes the <see cref="FastClass"/> class.
        /// </summary>
        static FastClass()
        {
            _fastClassId = 0;
            FastClassCache = new Dictionary<Type, FastClass>();
            FastClassCacheLock = new Object();
        }

        /// <summary>
        /// Gets the unique internally assigned identifier.
        /// </summary>
        /// <value>The id.</value>
        internal int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Creates a FastClass for the specified target type.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static FastClass Create(Type targetType)
        {
            lock(FastClassCacheLock)
            {
                FastClass fastClass;
                if (!FastClassCache.TryGetValue(targetType, out fastClass))
                {
                    fastClass = new FastClass(targetType, _fastClassId++);
                    FastClassCache[targetType] = fastClass;
                }

                return fastClass;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FastClass"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="id">The class id.</param>
        private FastClass(Type targetType, int id)
        {
            _id = id;
            _targetType = targetType;
            _instanceLock = new Object();
            _methodCache = new Dictionary<MethodInfo, FastMethod>(ReferenceEqualityComparer<MethodInfo>.Default);
            _fieldCache = new Dictionary<FieldInfo, FastField>();
            _propertyCache = new Dictionary<PropertyInfo, FastProperty>();
            _ctorCache = new Dictionary<ConstructorInfo, FastConstructor>();
        }

        /// <summary>
        /// News the instance.
        /// </summary>
        /// <returns></returns>
        public object NewInstance()
        {
            try
            {
                var constructor = GetDefaultConstructor();
                if (constructor == null)
                {
                    throw new ArgumentException("type does not support a default constructor");
                }

                return constructor.New();
            } 
            catch( AmbiguousMatchException )
            {
                var bindingFlags =
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.Static;

                var ctor = _targetType.GetConstructors(bindingFlags).FirstOrDefault(constructorInfo => constructorInfo.GetParameters().Length == 0);
                if (ctor == null)
                {
                    throw new ArgumentException("type does not support a default constructor");
                }

                var constructor = GetConstructor(ctor);
                if (constructor == null)
                {
                    throw new ArgumentException("type does not support a default constructor");
                }

                return constructor.New();
            }
        }

        #region "FastConstructor"

        /// <summary>
        /// Gets the default constructor.
        /// </summary>
        /// <returns></returns>
        public FastConstructor GetDefaultConstructor()
        {
            return GetConstructor(new Type[0]);
        }

        /// <summary>
        /// Gets a fast constructor implementation for the given
        /// constructor.
        /// </summary>
        /// <param name="ctor">The constructor.</param>
        /// <returns></returns>
        public FastConstructor GetConstructor(ConstructorInfo ctor)
        {
            lock (_instanceLock)
            {
                FastConstructor fastCtor;
                if (!_ctorCache.TryGetValue(ctor, out fastCtor))
                {
                    fastCtor = new FastConstructor(this, ctor);
                    _ctorCache[ctor] = fastCtor;
                }

                return fastCtor;
            }
        }

        /// <summary>
        /// Gets the constructor.
        /// </summary>
        /// <param name="paramTypes">The param types.</param>
        /// <returns></returns>
        public FastConstructor GetConstructor(params Type[] paramTypes)
        {
            BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static;

            ConstructorInfo ctor = _targetType.GetConstructor(bindingFlags, null, paramTypes, null);
            if (ctor == null)
            {
                return null;
            }
            
            return GetConstructor(ctor);
        }
        #endregion

        #region "FastMethod"
        /// <summary>
        /// Creates the method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static FastMethod CreateMethod(MethodInfo method)
        {
            FastClass fastClass = Create(method.DeclaringType);
            return fastClass.GetMethod(method);
        }

        /// <summary>
        /// Gets a fast method implementation for the given
        /// method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public FastMethod GetMethod(MethodInfo method)
        {
            lock (_instanceLock)
            {
                FastMethod fastMethod;
                if (!_methodCache.TryGetValue(method, out fastMethod))
                {
                    fastMethod = new FastMethod(this, method);
                    _methodCache[method] = fastMethod;
                }

                return fastMethod;
            }
        }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public FastMethod GetMethod(String name)
        {
            BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static;

            MethodInfo method = _targetType.GetMethod(name, bindingFlags);
            if (method == null)
            {
                return null;
            }
            
            return GetMethod(method);
        }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="paramTypes">The param types.</param>
        /// <returns></returns>
        public FastMethod GetMethod(String name, Type[] paramTypes)
        {
            BindingFlags bindingFlags =
                BindingFlags.Public|
                BindingFlags.NonPublic|
                BindingFlags.Instance|
                BindingFlags.Static;

            MethodInfo method = _targetType.GetMethod(name, bindingFlags, null, paramTypes, null);
            if (method == null)
            {
                return null;
            }
            
            return GetMethod(method);
        }
        #endregion

        #region "FastField"
        /// <summary>
        /// Gets a fast field implementation for the given
        /// field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public FastField GetField(FieldInfo field)
        {
            lock (_instanceLock)
            {
                FastField fastField;
                if (!_fieldCache.TryGetValue(field, out fastField))
                {
                    fastField = new FastField(this, field);
                    _fieldCache[field] = fastField;
                }

                return fastField;
            }
        }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public FastField GetField(String name)
        {
            BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static;

            FieldInfo field = _targetType.GetField(name, bindingFlags);
            if (field == null)
            {
                return null;
            }
            
            return GetField(field);
        }
        #endregion

        #region "FastProperty"
        /// <summary>
        /// Gets a fast property implementation for the given
        /// property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public FastProperty GetProperty(PropertyInfo property)
        {
            lock (_instanceLock)
            {
                FastProperty fastProperty;
                if (!_propertyCache.TryGetValue(property, out fastProperty))
                {
                    fastProperty = new FastProperty(this, property);
                    _propertyCache[property] = fastProperty;
                }

                return fastProperty;
            }
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public FastProperty GetProperty(String name)
        {
            BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static;

            PropertyInfo property = _targetType.GetProperty(name, bindingFlags);
            if (property == null)
            {
                return null;
            }

            return GetProperty(property);
        }
        #endregion
    }

    public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public static ReferenceEqualityComparer<T> Default { get; private set; }

        static ReferenceEqualityComparer()
        {
            Default = new ReferenceEqualityComparer<T>();
        }

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
