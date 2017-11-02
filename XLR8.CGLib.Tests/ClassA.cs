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

namespace XLR8.CGLib.Tests
{
    public class ClassA
    {
        public int publicIntValue;
        internal int internalIntValue;
        private int privateIntValue;

        public ClassA() { }
        public ClassA(int intValue)
        {
            publicIntValue = intValue;
        }
        public ClassA(int intValue1, int intValue2)
        {
            publicIntValue = intValue1;
            privateIntValue = intValue2;
        }

        public int PrivateIntValue
        {
            get { return privateIntValue; }
            set { privateIntValue = value; }
        }

        public void TrySetPrivateIntValue(int value)
        {
            privateIntValue = value;
        }

        public static void StaticSetPrivateIntValue(object instance, object value)
        {
            ((ClassA)instance).privateIntValue = (int)value;
        }

        public int InstanceMethod4(int value1, int value2, int value3, int value4)
        {
            return value1 + value2 + value3 + value4;
        }

        public int InstanceMethod3(int value1, int value2, int value3)
        {
            return value1 + value2 + value3;
        }

        public int InstanceMethod2(int value1, int value2)
        {
            return value1 + value2;
        }

        public object InstanceObject(object input)
        {
            return input;
        }

        public double InstanceDouble(double input)
        {
            return input;
        }

        public float InstanceFloat(float input)
        {
            return input;
        }

        public long InstanceInt64(long input)
        {
            return input;
        }

        public int InstanceInt32(int input)
        {
            System.Console.WriteLine("input: {0}", input);
            return input;
        }

        public short InstanceInt16(short input)
        {
            return input;
        }


        public static object StaticObject(object value)
        {
            return value;
        }

        public static double StaticDouble(double input)
        {
            return input;
        }

        public static float StaticFloat(float input)
        {
            return input;
        }

        public static long StaticInt64(long input)
        {
            return input;
        }

        public static int StaticInt32(int input)
        {
            return input;
        }

        public static short StaticInt16(short input)
        {
            return input;
        }
    }
}
