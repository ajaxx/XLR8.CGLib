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
    public class TestConstructor
    {
        [Test]
        public void CanCallDefaultConstructor()
        {
            var clazzType = FastClass.Create(typeof(ClassA));
            var constructor = clazzType.GetDefaultConstructor();
            Assert.That(constructor, Is.Not.Null);

            var instance = constructor.New();
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<ClassA>());
        }

        [Test]
        public void CanCallConstructorWithArgs()
        {
            var clazzType = FastClass.Create(typeof(ClassA));

            var constructor1 = clazzType.GetConstructor(typeof(int));
            Assert.That(constructor1, Is.Not.Null);
            var instance1 = constructor1.New(100);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(instance1, Is.InstanceOf<ClassA>());
            Assert.That(((ClassA)instance1).publicIntValue, Is.EqualTo(100));

            var constructor2 = clazzType.GetConstructor(typeof(int), typeof(int));
            Assert.That(constructor2, Is.Not.Null);
            var instance2 = constructor2.New(200, 300);
            Assert.That(instance2, Is.Not.Null);
            Assert.That(instance2, Is.InstanceOf<ClassA>());
            Assert.That(((ClassA)instance2).publicIntValue, Is.EqualTo(200));
            Assert.That(((ClassA)instance2).PrivateIntValue, Is.EqualTo(300));
        }

        [Test]
        public void MissingConstructorShouldReturnNull()
        {
            var clazzType = FastClass.Create(typeof(ClassA));

            var constructor = clazzType.GetConstructor(typeof(double));
            Assert.That(constructor, Is.Null);
        }
    }
}
