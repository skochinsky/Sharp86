using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public struct FarPointer
    {
        public FarPointer(ushort seg, ushort offset)
        {
            value = (uint)(seg << 16 | offset);
        }

        public FarPointer(uint ptr)
        {
            value = ptr;
        }

        public uint value;

        public ushort Segment { get { return (ushort)(value >> 16); } }
        public ushort Offset { get { return (ushort)(value & 0xFFFF); } }

        public override string ToString()
        {
            return string.Format("0x{0:X4}:0x{1:X4}", value >> 16, value & 0xFFFF);
        }
    }
}
