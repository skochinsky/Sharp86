using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaJson;

namespace Sharp86
{
    public class CodeBreakPoint : BreakPoint
    {
        public CodeBreakPoint()
        {
        }

        public CodeBreakPoint(ushort segment, ushort offset)
        {
            Segment = segment;
            Offset = offset;
        }

        [Json("address")]
        public uint Address
        {
            get
            {
                return (uint)(Segment << 16 | Offset);
            }
            set
            {
                Segment = (ushort)(value >> 16);
                Offset = (ushort)(value & 0xFFFF);
            }
        }

        public ushort Segment;
        public ushort Offset;

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            var cpu = debugger.CPU;
            return Segment == cpu.cs && Offset == cpu.ip;
        }

        public override string ToString()
        {
            return base.ToString(string.Format("at {0:X4}:{1:X4}", Segment, Offset));
        }

        public override string EditString
        {
            get
            {
                return string.Format("0x{0:X4}:0x{1:X4}", Segment, Offset);
            }
        }
    }

}
