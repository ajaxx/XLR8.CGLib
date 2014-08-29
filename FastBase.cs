using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace XLR8.CGLib
{
    public class FastBase
    {
        /// <summary>
        /// Gets the parameter types.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static Type[] GetParameterTypes(MethodInfo method)
        {
            // Get the method parameters
            ParameterInfo[] paramInfoList = method.GetParameters();
            // Convert the paramInfoList into raw types
            Type[] paramTypeList = new Type[paramInfoList.Length];
            for (int ii = 0; ii < paramInfoList.Length; ii++)
            {
                paramTypeList[ii] = paramInfoList[ii].ParameterType;
            }

            // Return the list
            return paramTypeList;
        }

        internal static Dictionary<Type, MethodInfo> typeConversionTable;
        internal static MethodInfo genericConverter;

        static FastBase()
        {
            Type typeConverter = typeof (Convert);
            Type[] typeParams = new Type[] {typeof (Object)};

            typeConversionTable = new Dictionary<Type, MethodInfo>();
            typeConversionTable[typeof(System.Boolean)] = typeConverter.GetMethod("ToBoolean", typeParams);
            typeConversionTable[typeof(System.Char)] = typeConverter.GetMethod("ToChar", typeParams);
            typeConversionTable[typeof(System.Decimal)] = typeConverter.GetMethod("ToDecimal", typeParams);
            typeConversionTable[typeof(System.Int16)] = typeConverter.GetMethod("ToInt16", typeParams);
            typeConversionTable[typeof(System.Int32)] = typeConverter.GetMethod("ToInt32", typeParams);
            typeConversionTable[typeof(System.Int64)] = typeConverter.GetMethod("ToInt64", typeParams);
            typeConversionTable[typeof(System.UInt16)] = typeConverter.GetMethod("ToUInt16", typeParams);
            typeConversionTable[typeof(System.UInt32)] = typeConverter.GetMethod("ToUInt32", typeParams);
            typeConversionTable[typeof(System.UInt64)] = typeConverter.GetMethod("ToUInt64", typeParams);
            typeConversionTable[typeof(System.Single)] = typeConverter.GetMethod("ToSingle", typeParams);
            typeConversionTable[typeof(System.Double)] = typeConverter.GetMethod("ToDouble", typeParams);
            typeConversionTable[typeof(System.DateTime)] = typeConverter.GetMethod("ToDateTime", typeParams);

            genericConverter = typeConverter.GetMethod("ChangeType", new Type[] {typeof (Object), typeof (Type)});
        }

        /// <summary>
        /// Emits the cast or conversion.
        /// </summary>
        /// <param name="il">The il.</param>
        /// <param name="type">The type.</param>
        internal static void EmitCastConversion(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                MethodInfo typeConverter;

                if (typeConversionTable.TryGetValue(type, out typeConverter))
                {
                    il.Emit(OpCodes.Call, typeConverter);
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, type); // Correct type must be passed
                }
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        /// <summary>
        /// Emits the load int.
        /// </summary>
        /// <param name="il">The il.</param>
        /// <param name="value">The value.</param>
        internal static void EmitLoadInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    il.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }
    }
}
