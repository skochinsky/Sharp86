using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaJson;

namespace Sharp86
{
    public class InterruptBreakPoint : BreakPoint
    {
        public InterruptBreakPoint()
        {
        }

        public InterruptBreakPoint(byte interruptNumber)
        {
            _interruptNumber = interruptNumber;
        }

        [Json("interrupteNumber")]
        byte _interruptNumber;
        public byte InterruptNumber
        {
            get { return _interruptNumber; }
        }

        public override string EditString
        {
            get
            {
                return string.Format("int 0x{0:X2}", _interruptNumber);
            }
        }

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            return false;
        }

        public override string ToString()
        {
            return base.ToString(string.Format("int 0x{0:X2}", _interruptNumber));
        }
    }
}
