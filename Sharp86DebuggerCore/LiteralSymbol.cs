using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public class LiteralSymbol : Symbol
    {
        public LiteralSymbol(object value)
        {
            _value = value;
        }

        object _value;

        public override object GetValue()
        {
            return _value;
        }
    }

}
