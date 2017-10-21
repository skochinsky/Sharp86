using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public class StepOutBreakPoint : BreakPoint
    {
        public StepOutBreakPoint(CPU cpu)
        {
            // Store the current stack pointer
            _ssBreakOnReturn = cpu.ss;
            _spBreakOnReturn = cpu.sp;
        }

        ushort _ssBreakOnReturn = 0;
        ushort _spBreakOnReturn = 0;

        public override string EditString
        {
            get
            {
                return "";
            }
        }


        public override bool ShouldBreak(DebuggerCore debugger)
        {
            // Break after executing a return instruction when the stack
            // pointer is higher than it currently is.

            var cpu = debugger.CPU;
            return cpu.DidReturn && cpu.ss == _ssBreakOnReturn && cpu.sp > _spBreakOnReturn;
        }
    }
}
