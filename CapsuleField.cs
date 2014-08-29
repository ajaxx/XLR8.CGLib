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
