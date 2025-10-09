using System;
using System.Collections.Generic;
using System.Text;

namespace unitechRFIDSample.Model
{
    public class ItemCollection
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ItemCollection(string name) : this(name, name)
        { }

        public ItemCollection(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
