using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace XLR8.CGLib
{
    /// <summary>
    /// Creates classes from specifications.
    /// </summary>
    public class CapsuleEmitter
    {
        private static AssemblyBuilder sAssemblyBuilder = null;
        private static ModuleBuilder sModuleBuilder = null;

        /// <summary>
        /// Gets the assembly builder.
        /// </summary>
        /// <returns></returns>
        private static AssemblyBuilder GetAssemblyBuilder()
        {
            if ( sAssemblyBuilder == null ) {
                AssemblyName assemblyName = new AssemblyName();
                assemblyName.Name = "CGLibCapsule";

                AppDomain thisDomain = Thread.GetDomain();

                sAssemblyBuilder = thisDomain.DefineDynamicAssembly(
                    assemblyName, 
                    AssemblyBuilderAccess.Run);
            }
 
            return sAssemblyBuilder;
        }

        /// <summary>
        /// Gets the module builder.
        /// </summary>
        /// <returns></returns>
        private static ModuleBuilder GetModuleBuilder()
        {
            if ( sModuleBuilder == null ) {
                AssemblyBuilder assemblyBuilder = GetAssemblyBuilder();
                sModuleBuilder = assemblyBuilder.DefineDynamicModule(
                    "CGLibCapsule", false);
            }

            return sModuleBuilder;
        }

        /// <summary>
        /// Creates the capsule.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        public static FastClass CreateCapsule( String name, params CapsuleField[] fields )
        {
            ModuleBuilder moduleBuilder = GetModuleBuilder();
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                name,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                typeof (object));

            foreach( CapsuleField field in fields ) {
                typeBuilder.DefineField(
                    field.Name,
                    field.Type,
                    FieldAttributes.Public);
            }

            return FastClass.Create(typeBuilder.CreateType());
        }
    }
}
