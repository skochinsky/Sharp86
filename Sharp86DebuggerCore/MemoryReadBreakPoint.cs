using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    class MemoryReadBreakPoint : BaseMemoryBreakPoint, IBreakPointMemRead
    {
        public MemoryReadBreakPoint()
        {
        }

        public MemoryReadBreakPoint(ushort segment, ushort offset, ushort length) : 
            base(segment, offset, length)
        {
        }

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            bool retv = _tripped;
            _tripped = false;
            return retv;
        }

        bool _tripped;

        public override string ToString()
        {
            return base.ToString(string.Format("memr 0x{0:X4}:{1:X4} - 0x{2:X4}:{3:X4} ({4} bytes)",
                Segment, Offset,
                Segment, Offset + Length,
                Length
                ));

        }

        void IBreakPointMemRead.ReadByte(ushort seg, ushort offset, byte value)
        {
            if (seg == Segment && offset >= Offset && offset < Offset + Length)
            {
                _tripped = true;
            }
        }

        public override string EditString
        {
            get
            {
                return string.Format("memr 0x{0:X4}:0x{1:X4},{2}", Segment, Offset, Length);
            }
        }
    }
}
