using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaJson;

namespace Sharp86
{
    class CpuTimeBreakPoint : BreakPoint
    {
        public CpuTimeBreakPoint()
        {
        }

        public CpuTimeBreakPoint(ulong cputime)
        {
            CpuTime = cputime;
        }

        [Json("cpuTime")]
        public ulong CpuTime;

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            return debugger.CPU.CpuTime == CpuTime;
        }

        public override string ToString()
        {
            return base.ToString(string.Format("cputime {0}", CpuTime));
        }

        public override string EditString
        {
            get
            {
                return string.Format("cputime {0}", CpuTime);
            }
        }
    }
}
