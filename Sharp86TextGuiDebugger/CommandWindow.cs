using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConFrames;

namespace Sharp86
{
    public class CommandWindow : ConsoleWindow
    {
        public CommandWindow(TextGuiDebugger owner) : base("Console", new Rect(0, 30, 80, 10))
        {
            _owner = owner;
            Prompt = ">";
            WriteLine("Win3mu Debugger!\n");
        }

        TextGuiDebugger _owner;

        protected override void OnCommand(string commandLine)
        {
            _owner.ExecuteCommand(commandLine);
            base.OnCommand(commandLine);
        }

    }
}
