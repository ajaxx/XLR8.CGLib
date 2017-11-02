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
    public class FastConstructor : FastBase
    {
        private static int _constructorIdCounter = 0;

        /// <summary>
        /// Class object that this constructor belongs to.
        /// </summary>

        private readonly FastClass _fastClass;

        /// <summary>
        /// Method that is being proxied
        /// </summary>

        private readonly ConstructorInfo _targetConstructor;

        /// <summary>
        /// Dynamic method that is constructed for invocation.
        /// </summary>

        private readonly DynamicMethod _dynamicMethod;

        private readonly Invoker _invoker;

        /// <summary>
        /// Gets the target constructor.
        /// </summary>
        /// <value>The target method.</value>
        public ConstructorInfo Target
        {
            get { return _targetConstructor; }
        }

        /// <summary>
        /// Gets the type of the declaring.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public FastClass DeclaringType
        {
            get { return _fastClass; }
        }

        /// <summary>
        /// Gets the parameter count.
        /// </summary>
        /// <value>The parameter count.</value>
        public int ParameterCount
        {
            get { return _targetConstructor.GetParameters().Length; }
        }

        /// <summary>
        /// Gets the parameter types.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        /// <returns></returns>
        internal static Type[] GetParameterTypes(ConstructorInfo constructor)
        {
            // Get the method parameters
            ParameterInfo[] paramInfoList = constructor.GetParameters();
            // Convert the paramInfoList into raw types
            Type[] paramTypeList = new Type[paramInfoList.Length];
            for (int ii = 0; ii < paramInfoList.Length; ii++)
            {
                paramTypeList[ii] = paramInfoList[ii].ParameterType;
            }

            // Return the list
            return paramTypeList;
        }

        /// <summary>
        /// Constructs a wrapper around the target constructor.
        /// </summary>
        /// <param name="_fastClass">The _fast class.</param>
        /// <param name="constructor">The constructor.</param>

        internal FastConstructor(FastClass _fastClass, ConstructorInfo constructor)
        {
            // Store the class that spawned us
            this._fastClass = _fastClass;

            _targetConstructor = constructor;

            int uid = System.Threading.Interlocked.Increment(ref _constructorIdCounter);

            // Create a unique name for the method
            String dynamicMethodName = "_CGLibC_" + this._fastClass.Id + "_" + uid;
            // Generate the method
            _dynamicMethod = new DynamicMethod(
                dynamicMethodName,
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(Object),
                new Type[] { typeof(Object), typeof(Object[]) },
                _targetConstructor.Module,
                true);

            EmitInvoker(_targetConstructor, _dynamicMethod.GetILGenerator());
            _invoker = (Invoker)_dynamicMethod.CreateDelegate(typeof(Invoker));
        }

        static void EmitInvoker(ConstructorInfo ctor, ILGenerator il)
        {
            il.DeclareLocal(typeof(object));

            // Get the method parameters
            Type[] parameterTypes = GetParameterTypes(ctor);

            // Load method arguments
            for (int ii = 0; ii < parameterTypes.Length; ii++)
            {
                Type paramType = parameterTypes[ii];
                il.Emit(OpCodes.Ldarg_1);
                EmitLoadInt(il, ii);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastConversion(il, paramType);
            }

            // Emit code to construct object
            il.Emit(OpCodes.Newobj, ctor);

            if (ctor.DeclaringType.IsValueType)
            {
                il.Emit(OpCodes.Box, ctor.DeclaringType);
            }

            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            // Emit code for return value
            il.Emit(OpCodes.Ret);
        }
    
        /// <summary>
        /// Creates a new instance of the target using the parameters.
        /// </summary>
        /// <param name="paramList">The param list.</param>
        public Object New(params Object[] paramList)
        {
#if USE_REFLECTION
            return targetMethod.Invoke(target, paramList);
#else
            return _invoker(null, paramList);
#endif
        }
    }
}
