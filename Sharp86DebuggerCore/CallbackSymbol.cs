using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    class CallbackSymbol : Symbol
    {
        public CallbackSymbol(Func<object> callback)
        {
            _callback = callback;
        }

        Func<object> _callback;

        public override object GetValue()
        {
            return _callback();
        }
    }

}
