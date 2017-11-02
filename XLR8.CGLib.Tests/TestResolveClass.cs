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
    public class TestResolveClass
    {
        [Test]
        public void ResolvePublicClass()
        {
            var sampleClassA = FastClass.Create(typeof(ClassA));
            Assert.That(sampleClassA, Is.Not.Null);
            Assert.That(sampleClassA.TargetType, Is.EqualTo(typeof(ClassA)));
        }

        [Test]
        public void ResolveNamespaceClass()
        {
            var sampleClassB = FastClass.Create(typeof(ClassB));
            Assert.That(sampleClassB, Is.Not.Null);
            Assert.That(sampleClassB.TargetType, Is.EqualTo(typeof(ClassB)));
        }

        [Test]
        public void CacheShouldReturnSameClass()
        {
            var sampleClassA = FastClass.Create(typeof(ClassA));
            var sampleClassB = FastClass.Create(typeof(ClassA));
            Assert.That(sampleClassA, Is.Not.Null);
            Assert.That(sampleClassB, Is.Not.Null);
            Assert.That(sampleClassA, Is.SameAs(sampleClassB));
        }
    }
}
