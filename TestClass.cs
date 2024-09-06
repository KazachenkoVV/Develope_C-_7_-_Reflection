using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflexia1
{
    internal class TestClass
    {
        [CastomName("IntegerProp")]
        public int I { get; set; }
        [CastomName("StringProp")]
        public string? S { get; set; }
        [CastomName("DecimalProp")]
        public decimal D { get; set; }
        [CastomName("CharArrayProp")]

        public char[]? C { get; set; }

        public TestClass()
        { }
        private TestClass(int i)
        {
            this.I = i;
        }
        public TestClass(int i, string s, decimal d, char[] c) : this(i)
        {
            this.S = s;
            this.D = d;
            this.C = c;
        }
        // "Этот метод переписал сам для проверки работы программы
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString + "\n" +
            $"I = {I}, " +
            $"S = {S}, " +
            $"D = {D}, " +
            "C[] = {");
            if (C != null)
            {
                foreach (var ch in C)
                {
                    sb.Append($"'{ch}', ");
                }
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
