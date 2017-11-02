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

#if NETFULL
using System.Threading;
#endif

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

#if NETFULL
                AppDomain thisDomain = Thread.GetDomain();

                sAssemblyBuilder = thisDomain.DefineDynamicAssembly(
                    assemblyName, 
                    AssemblyBuilderAccess.Run);
#elif NETSTANDARD
                sAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                    assemblyName, AssemblyBuilderAccess.Run);
#else
#error "building unknown version of codebase"
#endif
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
                sModuleBuilder = assemblyBuilder.DefineDynamicModule("CGLibCapsule");
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

#if NETFULL
            return FastClass.Create(typeBuilder.CreateType());
#elif NETSTANDARD
            return FastClass.Create(typeBuilder.CreateTypeInfo().AsType());
#else
#error "building unknown version of codebase"
#endif
        }
    }
}
