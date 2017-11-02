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

using NUnit.Framework;

namespace XLR8.CGLib.Tests
{
    [TestFixture]
    public class TestMethods
    {
        [Test]
        public void CanCallInstanceMethods()
        {
            CallInstanceMethod("InstanceInt16", short.MaxValue >> 2);
            CallInstanceMethod("InstanceInt32", int.MaxValue >> 2);
            CallInstanceMethod("InstanceInt64", long.MaxValue >> 2);
            CallInstanceMethod("InstanceFloat", float.MaxValue / 4.0);
            CallInstanceMethod("InstanceDouble", double.MaxValue / 4.0);
        }

        [Test]
        public void CanCallStaticMethods()
        {
            CallStaticMethod("StaticInt16", short.MaxValue);
            CallStaticMethod("StaticInt32", int.MaxValue);
            CallStaticMethod("StaticInt64", long.MaxValue);
            CallStaticMethod("StaticFloat", float.MaxValue);
            CallStaticMethod("StaticDouble", double.MaxValue);
        }

        [Test]
        public void CanUseMoreThanOneArgument()
        {
            var instanceA = new ClassA();
            var clazzType = FastClass.Create(typeof(ClassA));
            Assert.That(
                clazzType.GetMethod("InstanceMethod2").Invoke(instanceA, 1, 2),
                Is.EqualTo(3));
            Assert.That(
                clazzType.GetMethod("InstanceMethod3").Invoke(instanceA, 3, 4, 5),
                Is.EqualTo(12));
            Assert.That(
                clazzType.GetMethod("InstanceMethod4").Invoke(instanceA, 6, 7, 8, 9),
                Is.EqualTo(30));
        }

        private void CallInstanceMethod<T>(string methodName, T value)
        {
            var instanceA = new ClassA();
            var clazzType = FastClass.Create(typeof(ClassA));
            Assert.That(clazzType.GetMethod(methodName).Invoke(instanceA, value), Is.EqualTo(value));
        }

        private void CallStaticMethod<T>(string methodName, T value)
        {
            var clazzType = FastClass.Create(typeof(ClassA));
            Assert.That(clazzType.GetMethod(methodName).Invoke(null, value), Is.EqualTo(value));
        }
    }
}
