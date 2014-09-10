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
