using System;
using System.Reflection.Emit;

namespace XLR8.CGLib
{
    internal class FastEmitter
    {
        public static void EmitLoadArg(ILGenerator il, int arg)
        {
            switch (arg)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    il.Emit(OpCodes.Ldarg, arg);
                    break;
            }
        }
    }
}
