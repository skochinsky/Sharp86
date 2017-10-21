using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public class TextGuiDebuggerCommands
    {
        public TextGuiDebuggerCommands(TextGuiDebugger debugger)
        {
            _debugger = debugger;
        }

        TextGuiDebugger _debugger;

        [DebuggerHelp("View address in the code window")]
        public void view_code(FarPointer addr)
        {
            _debugger.CodeWindow.MoveTo(addr.Segment, addr.Offset, 0);
            _debugger.CodeWindow.Activate();
        }

        [DebuggerHelp("View address in the memory window")]
        public void view_mem(FarPointer addr)
        {
            _debugger.MemoryWindow.MoveTo(addr.Segment, addr.Offset);
            _debugger.MemoryWindow.Activate();
        }
    }
}
