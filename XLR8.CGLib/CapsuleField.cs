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

namespace XLR8.CGLib
{
    public class CapsuleField
    {
        private string name;
        private Type type;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Type Type
        {
            get { return type; }
            set { type = value; }
        }

        public CapsuleField()
        {
        }

        public CapsuleField(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }
    }
}
