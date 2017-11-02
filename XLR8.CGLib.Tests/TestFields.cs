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
    public class TestFields
    {
        [Test]
        public void CanGetPublicFields()
        {
            var constValue = 2000;
            var instance = new ClassA();
            instance.publicIntValue = constValue;

            var clazzType = FastClass.Create(typeof(ClassA));
            var clazzField = clazzType.GetField("publicIntValue");

            Assert.That(clazzField, Is.Not.Null);
            Assert.That(clazzField.Get(instance), Is.EqualTo(constValue));
            Assert.That(instance.publicIntValue, Is.EqualTo(constValue));
        }

        [Test]
        public void CanGetPrivateFields()
        {
            var constValue = 3000;
            var instance = new ClassA();
            instance.PrivateIntValue = constValue;

            var clazzType = FastClass.Create(typeof(ClassA));
            var clazzField = clazzType.GetField("privateIntValue");

            Assert.That(clazzField, Is.Not.Null);
            Assert.That(clazzField.Get(instance), Is.EqualTo(constValue));
            Assert.That(instance.PrivateIntValue, Is.EqualTo(constValue));
        }

        [Test]
        public void CanSetPublicFields()
        {
            var constValue = 5000;
            var instance = new ClassA();

            var clazzType = FastClass.Create(typeof(ClassA));
            var clazzField = clazzType.GetField("publicIntValue");

            Assert.That(clazzField, Is.Not.Null);
            clazzField.Set(instance, constValue);
            Assert.That(instance.publicIntValue, Is.EqualTo(constValue));
        }

        [Test]
        public void CanSetPrivateFields()
        {
            var constValue = 10000;
            var instance = new ClassA();

            var clazzType = FastClass.Create(typeof(ClassA));
            var clazzField = clazzType.GetField("privateIntValue");

            Assert.That(clazzField, Is.Not.Null);
            clazzField.Set(instance, constValue);
            Assert.That(instance.PrivateIntValue, Is.EqualTo(constValue));
        }
    }
}
