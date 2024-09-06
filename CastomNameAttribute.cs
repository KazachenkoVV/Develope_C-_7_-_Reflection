using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflexia1
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class CastomNameAttribute:Attribute
    {
        public string Name { get; }
        internal CastomNameAttribute(string name)
        {
            Name = name;
        }
    }
}
