///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2014 XLR8 Development Team                                           /
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
    public class FastField
    {
        /// <summary>
        /// Class object that this method belongs to.
        /// </summary>

        private readonly FastClass fastClass;

        /// <summary>
        /// Field that is being proxied
        /// </summary>

        private readonly FieldInfo targetField;

        /// <summary>
        /// Dynamic method that is constructed for invocation.
        /// </summary>

        private DynamicMethod dynamicGetMethod;
        private DynamicMethod dynamicSetMethod;

        private Getter getInvoker;
        private Setter setInvoker;

        /// <summary>
        /// Gets the target field.
        /// </summary>
        /// <value>The target field.</value>
        public FieldInfo Target
        {
            get { return targetField; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public String Name
        {
            get { return targetField.Name; }
        }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        /// <value>The field type</value>
        public Type FieldType
        {
            get { return targetField.FieldType; }
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
        /// Constructs a wrapper around the target field.
        /// </summary>
        /// <param name="_fastClass">The _fast class.</param>
        /// <param name="field">The field.</param>

        internal FastField(FastClass _fastClass, FieldInfo field)
        {
            // Store the class that spawned us
            fastClass = _fastClass;
            // Field we will be get/setting
            targetField = field;

            CreateDynamicGetMethod(field);
            CreateDynamicSetMethod(field);
        }

        /// <summary>
        /// Creates the dynamic get method.
        /// </summary>
        /// <param name="field">The field.</param>
        private void CreateDynamicGetMethod(FieldInfo field)
        {
            Module module = targetField.Module;
            // Create a unique name for the method
            String dynamicMethodName = "_CGLibFG_" + fastClass.Id + "_" + field.Name;
            // Generate the method
            dynamicGetMethod = new DynamicMethod(
                dynamicMethodName,
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(Object),
                new Type[]{ typeof(Object) }, 
                module,
                true);

            // Create the IL generator
            ILGenerator il = dynamicGetMethod.GetILGenerator();

            il.DeclareLocal(typeof(object));

            // Is this calling an instance method; if so, load 'this'
            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, field.DeclaringType);
                il.Emit(OpCodes.Ldfld, targetField);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, targetField);
            }

            // Emit code for return value
            if (field.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, field.FieldType);	//Box if necessary
            }

            // Emit code for return value
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            getInvoker = (Getter) dynamicGetMethod.CreateDelegate(typeof (Getter));
        }

        /// <summary>
        /// Creates the dynamic set method.
        /// </summary>
        /// <param name="field">The field.</param>
        private void CreateDynamicSetMethod(FieldInfo field)
        {
            Module module = targetField.Module;
            // Create a unique name for the method
            String dynamicMethodName = "_CGLibFS_" + fastClass.Id + "_" + field.Name;

            // Generate the method
            dynamicSetMethod = new DynamicMethod(
                dynamicMethodName,
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(void),
                new Type[]{ typeof(Object), typeof(Object) },
                module,
                true);

            // Create the IL generator
            ILGenerator il = dynamicSetMethod.GetILGenerator();
            // Is this calling an instance method; if so, load 'this'
            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, field.DeclaringType);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, field.FieldType);
                il.Emit(OpCodes.Stfld, targetField);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, field.FieldType);
                il.Emit(OpCodes.Stsfld, targetField);
            }

            il.Emit(OpCodes.Ret);

            setInvoker = (Setter)dynamicSetMethod.CreateDelegate(typeof(Setter));
        }

        /// <summary>
        /// Gets the value of the field
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public Object Get(Object target)
        {
            return getInvoker(target);
            //return targetField.GetValue(target);
        }

        /// <summary>
        /// Gets the value of a static field
        /// </summary>
        /// <returns></returns>
        public Object GetStatic()
        {
            return getInvoker(null);
            //return targetField.GetValue(null);
        }

        /// <summary>
        /// Sets the value of an instance field.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public void Set( Object target, Object value )
        {
            setInvoker(target, value);
            //targetField.SetValue(target, value);
        }

        /// <summary>
        /// Sets the value of a static field.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetStatic( Object value )
        {
            setInvoker(null, value);
            //targetField.SetValue(null, value);
        }
    }
}
