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
using System.Reflection;
using System.Reflection.Emit;

namespace XLR8.CGLib
{
    /// <summary>
    /// Provides access to property information and creates dynamic method to access
    /// properties rather than relying upon reflection.
    /// </summary>

    public class FastProperty : FastBase
    {
        /// <summary>
        /// Class object that this method belongs to.
        /// </summary>

        private readonly FastClass fastClass;

        /// <summary>
        /// Property that is being proxied
        /// </summary>
        private readonly PropertyInfo targetProperty;

        /// <summary>
        /// Dynamic method that is constructed for invocation.
        /// </summary>

        private DynamicMethod dynamicGetMethod;
        private DynamicMethod dynamicSetMethod;

        private Getter getInvoker;
        private Setter setInvoker;

        /// <summary>
        /// Gets the target property.
        /// </summary>
        /// <value>The target property.</value>
        public PropertyInfo Target
        {
            get { return targetProperty; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public String Name
        {
            get { return targetProperty.Name; }
        }

        /// <summary>
        /// Gets the property type.
        /// </summary>
        /// <value>The property type</value>
        public Type PropertyType
        {
            get { return targetProperty.PropertyType; }
        }

        /// <summary>
        /// Gets the type of the declaring.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public FastClass DeclaringType
        {
            get { return fastClass; }
        }

        /// <summary>
        /// Constructs a wrapper around the target property.
        /// </summary>
        /// <param name="_fastClass">The _fast class.</param>
        /// <param name="property">The property.</param>

        internal FastProperty(FastClass _fastClass, PropertyInfo property)
        {
            // Store the class that spawned us
            fastClass = _fastClass;
            // Property we will be get/setting
            targetProperty = property;

            CreateDynamicGetMethod(property);
            CreateDynamicSetMethod(property);
        }

        /// <summary>
        /// Creates the dynamic get method.
        /// </summary>
        /// <param name="property">The property.</param>
        private void CreateDynamicGetMethod(PropertyInfo property)
        {
            MethodInfo getMethod = property.GetGetMethod(false);
            if (getMethod == null)
            {
                return;
            }

            Module module = targetProperty.Module;
            // Create a unique name for the method
            String dynamicMethodName = "_CGLibPG_" + fastClass.Id + "_" + property.Name;

            // Generate the method
            dynamicGetMethod = new DynamicMethod(
                dynamicMethodName,
                //MethodAttributes.Public | MethodAttributes.Static,
                //CallingConventions.Standard,
                typeof(Object),
                new Type[]{ typeof(Object) },
                module,
                true);

            EmitGetMethod(getMethod, dynamicGetMethod.GetILGenerator());

            getInvoker = (Getter) dynamicGetMethod.CreateDelegate(typeof (Getter));
        }

        private static void EmitGetMethod(MethodInfo getMethod, ILGenerator il)
        {
            il.DeclareLocal(typeof(object));

            // Is this calling an instance method; if so, load 'this'
            if (!getMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, getMethod.DeclaringType);
            }

            if (getMethod.IsVirtual)
            {
                il.Emit(OpCodes.Callvirt, getMethod);
            }
            else
            {
                il.Emit(OpCodes.Call, getMethod);
            }

            // Emit code for return value
            if (getMethod.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, getMethod.ReturnType);	//Box if necessary
            }

            //il.Emit(OpCodes.Stloc_0);
            //il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates the dynamic set method.
        /// </summary>
        /// <param name="property">The property.</param>
        private void CreateDynamicSetMethod(PropertyInfo property)
        {
            MethodInfo setMethod = property.GetSetMethod(false);
            if (setMethod == null)
            {
                return;
            }

            Module module = targetProperty.Module;
            // Create a unique name for the method
            String dynamicMethodName = "_CGLibPS_" + fastClass.Id + "_" + property.Name;

            // Generate the method
            dynamicSetMethod = new DynamicMethod(
                dynamicMethodName,
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(void),
                new Type[]{ typeof(Object), typeof(Object) }, 
                module,
                true);

            EmitSetMethod(property, setMethod, dynamicSetMethod.GetILGenerator());

            setInvoker = (Setter)dynamicSetMethod.CreateDelegate(typeof(Setter));
        }

        private static void EmitSetMethod(PropertyInfo property, MethodInfo setMethod, ILGenerator il)
        {
            il.DeclareLocal(typeof(object));

            // Is this calling an instance method; if so, load 'this'
            if (!setMethod.IsStatic)
            {
                // Load the object instance
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, setMethod.DeclaringType);
            }

            // Load the value
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, property.PropertyType);
            // Call the set method
            il.Emit(OpCodes.Callvirt, setMethod);

            il.Emit(OpCodes.Ret);
        }

        public Getter GetGetInvoker()
        {
            return getInvoker;
        }

        /// <summary>
        /// Gets the value of the property
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public Object Get(Object target)
        {
            return getInvoker(target);
        }

        /// <summary>
        /// Gets the value of a static property
        /// </summary>
        /// <returns></returns>
        public Object GetStatic()
        {
            return getInvoker(null);
        }

        /// <summary>
        /// Sets the value of an instance property.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public void Set(Object target, Object value)
        {
            setInvoker(target, value);
        }

        /// <summary>
        /// Sets the value of a static property.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetStatic(Object value)
        {
            setInvoker(null, value);
        }
    }
}
