using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    class MemoryChangeBreakPoint : BaseMemoryBreakPoint
    {
        public MemoryChangeBreakPoint()
        {
        }

        public MemoryChangeBreakPoint(ushort segment, ushort offset, ushort length) : 
            base(segment, offset, length)
        {
        }

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            var cpu = debugger.CPU;

            var bus = cpu.MemoryBus as IMemoryBusDebug;
            if (bus == null)
                return false;

            return bus.DidMemoryChange(Segment, Offset, Length);
        }

        public override string ToString()
        {
            return base.ToString(string.Format("mem 0x{0:X4}:{1:X4} - 0x{2:X4}:{3:X4} ({4} bytes)",
                Segment, Offset,
                Segment, Offset + Length,
                Length
                ));

        }

        public override string EditString
        {
            get
            {
                return string.Format("mem 0x{0:X4}:0x{1:X4},{2}", Segment, Offset, Length);
            }
        }
    }
}
