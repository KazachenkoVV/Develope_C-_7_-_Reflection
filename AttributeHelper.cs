using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reflexia1
{
    public static class AttributeHelper
    {
        public static string GetPropertyByAttributeName<T>(string nameOfAttribute) where T : class
        {
            Type type = typeof(T);
            foreach (var property in type.GetProperties())
            {
                var attribute = (CastomNameAttribute)property.GetCustomAttribute(typeof(CastomNameAttribute), false);
                if(attribute != null && attribute.Name == nameOfAttribute)
                {
                    return property.Name;
                    break;
                }
            }
            return null;
        }
    }
}
