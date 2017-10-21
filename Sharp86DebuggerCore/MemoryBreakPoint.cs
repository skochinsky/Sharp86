using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaJson;

namespace Sharp86
{
    public abstract class BaseMemoryBreakPoint : BreakPoint
    {
        public BaseMemoryBreakPoint()
        {
        }
        public BaseMemoryBreakPoint(ushort segment, ushort offset, ushort length)
        {
            Segment = segment;
            Offset = offset;
            Length = length;
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

        [Json("length")]
        public uint Length;

    }
}
