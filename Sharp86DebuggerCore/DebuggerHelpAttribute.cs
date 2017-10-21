using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public class DebuggerHelpAttribute : Attribute
    {
        public DebuggerHelpAttribute(string help)
        {
            Help = help;
        }

        public string Help;
    }

}
