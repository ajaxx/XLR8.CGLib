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

namespace XLR8.CGLib
{
    public class FastClassUtil
    {
        /// <summary>
        /// Finds a property with the given name.  Once found, extracts the Get method
        /// and returns the method.  This method searches for non-public get methods
        /// if one can not be found.  Eventually, this should get wrapped with a
        /// FastMethod.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propName">Name of the prop.</param>
        /// <returns></returns>
        
        public static MethodInfo GetGetMethodForProperty(Type type, String propName)
        {
            BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static;
            PropertyInfo property = type.GetProperty(propName, bindingFlags);
            if (property != null)
            {
                MethodInfo tempMethod = property.GetGetMethod(false);
                if (tempMethod != null)
                {
                    return tempMethod;
                }
            }

            return null;
        }
    }
}
