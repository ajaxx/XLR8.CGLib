using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace XLR8.CGLib
{
    public class FastMethod : FastBase
    {
        /// <summary>
        /// Class object that this method belongs to.
        /// </summary>

        private readonly FastClass _fastClass;

        /// <summary>
        /// Method that is being proxied
        /// </summary>

        private readonly MethodInfo _targetMethod;

        /// <summary>
        /// Dynamic method that is constructed for invocation.
        /// </summary>

        private readonly DynamicMethod _dynamicMethod;

        private readonly Invoker _invoker;

        private readonly Action<object>[] _preInvocationTypeCheck;

        private readonly bool _isExtension;

        /// <summary>
        /// Gets the target method.
        /// </summary>
        /// <value>The target method.</value>
        public MethodInfo Target
        {
            get { return _targetMethod; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public String Name
        {
            get { return _targetMethod.Name; }
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ReturnType
        {
            get { return _targetMethod.ReturnType; }
        }

        /// <summary>
        /// Gets the type of the declaring.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public FastClass DeclaringType
        {
            get { return _fastClass;  }            
        }

        /// <summary>
        /// Gets the parameter count.
        /// </summary>
        /// <value>The parameter count.</value>
        public int ParameterCount
        {
            get { return _targetMethod.GetParameters().Length; }
        }

        /// <summary>
        /// Constructs a wrapper around the target method.
        /// </summary>
        /// <param name="fastClass">The _fast class.</param>
        /// <param name="method">The method.</param>

        internal FastMethod(FastClass fastClass, MethodInfo method)
        {
            // Store the class that spawned us
            _fastClass = fastClass;

            _targetMethod = method;

            _isExtension = _targetMethod.IsDefined(typeof (ExtensionAttribute), true);

            // Create a unique name for the method
            String dynamicMethodName = "_CGLibM_" + _fastClass.Id + "_" + method.Name;
            // Generate the method
            _dynamicMethod = new DynamicMethod(
                dynamicMethodName,
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(Object),
                new Type[]{ typeof(Object), typeof(Object[]) }, 
                _targetMethod.Module,
                true);

            EmitInvoker(method, _dynamicMethod.GetILGenerator());
            _invoker = (Invoker) _dynamicMethod.CreateDelegate(typeof (Invoker));

            _preInvocationTypeCheck = GetParameterTypes(method)
                .Select(CreateInvocationCheck)
                .ToArray();
        }

        private static readonly Action<object> PrimitiveInvocationCheck =
            o =>
            {
                if (o == null)
                {
                    throw new TargetInvocationException(
                        new ArgumentException("invalid assignment of null to primitive type"));
                }
            };

        private static readonly Action<object> EmptyInvocationCheck =
            o => { };

        static Action<object> CreateInvocationCheck(Type parameterType)
        {
            if (parameterType.IsPrimitive)
            {
                return PrimitiveInvocationCheck;
            }
            else
            {
                return EmptyInvocationCheck;
            }
        }

        static void EmitInvoker(MethodInfo method, ILGenerator il)
        {
            il.DeclareLocal(typeof(object));

            // Get the method parameters
            Type[] parameterTypes = GetParameterTypes(method);

            // Is the method non-static
            if ( !method.IsStatic )
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, method.DeclaringType);
            }

            // Load method arguments
            for( int ii = 0 ; ii < parameterTypes.Length ; ii++ )
            {
                Type paramType = parameterTypes[ii];
                il.Emit(OpCodes.Ldarg_1);
                EmitLoadInt(il, ii);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastConversion(il, paramType);
            }
            
            // Emit code to call the method
            il.Emit(OpCodes.Call, method);

            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if ( method.ReturnType.IsValueType )
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            // Emit code for return value
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Invokes the method on the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="paramList">The param list.</param>
        public Object Invoke(Object target, params Object[] paramList)
        {
#if USE_REFLECTION
            return targetMethod.Invoke(target, paramList);
#else
            if (_isExtension && (target != null))
            {
                var argList = new object[paramList.Length + 1];
                argList[0] = target;
                Array.Copy(paramList, 0, argList, 1, paramList.Length);

                for (int ii = 0; ii < _preInvocationTypeCheck.Length; ii++)
                {
                    _preInvocationTypeCheck[ii].Invoke(argList[ii]);
                }

                return _invoker(null, argList);
            }
            else
            {
                for (int ii = 0; ii < _preInvocationTypeCheck.Length; ii++)
                {
                    _preInvocationTypeCheck[ii].Invoke(paramList[ii]);
                }

                return _invoker(target, paramList);
            }
#endif
        }

        /// <summary>
        /// Invokes the method on the specified target.
        /// </summary>
        /// <param name="paramList">The param list.</param>
        /// <returns></returns>
        public Object InvokeStatic(params Object[] paramList)
        {
            return _invoker(null, paramList);
        }
    }
}
