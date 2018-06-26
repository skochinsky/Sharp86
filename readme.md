## Welcome to Sharp86

Sharp86 is an Intel 8086 emulator for .NET that is simple to host and includes a built-in "text based GUI" debugger.

## Background

Sharp86 is the CPU emulation used by Win3mu - a 16-bit Windows 3 emulator.

It supports the 8086 instruction but can also be run in a pseudo-protected mode where the host emulator provides selector support.  This is possible because Sharp86's bus interface provides both segment/selector and offset for memory operations (rather than exposing a flat memory model like a real processor would at the bus).

In other words, even though a host can emulate protected mode for user mode software, you can't run operating system level software that expects protected mode instructions to be available.

In the context of Win3mu this means the running software thinks it's running in protected mode, but Win3mu itself isn't running the real Windows 3.1 operating system.

## Hosting Sharp86

To host Sharp86, first create an instance of it, or derive a class from it and provide an implementation of a memory bus and a port bus:

```cs
class MyMachine : Sharp86.CPU, IMemoryBus, IPortBus
{
    public MyMachine()
    {
        base.MemoryBus = this;
        base.PortBus = this;
    }
}
```

The memory bus is a simple 8-bit bus used everytime the processor needs to read or write memory:

```cs
public interface IMemoryBus
{
    byte ReadByte(ushort seg, ushort offset);
    void WriteByte(ushort seg, ushort offset, byte value);
    bool IsExecutableSelector(ushort seg);
}
```

A minimal implementation for a flat memory model might look something like this:

```cs
    // A chunk of memory
    byte[] _memory = new byte[1024 * 1024];

    // Read a byte from memory
    byte IMemoryBus.ReadByte(ushort seg, ushort offset)
    {
        return _mem[seg << 4 + offset]
    }

    // Write a byte to memory
    void IMemoryBus.WriteByte(ushort seg, ushort offset, byte value)
    {
        _mem[seg << 4 + offset] = value;
    }

    // Check if a selector is executable.  For non-protected mode
    // just return true.  Called to validate selectors before 
    // being loaded into the cs register
    bool IMemoryBus.IsExecutableSelector(ushort seg)
    {
        return true;
    }
```

The port bus is used for port I/O operations:

```cs
public interface IPortBus
{
    byte ReadPortByte(ushort port);
    void WritePortByte(ushort port, byte value);
}
```

A minimal implementation of this could simply return 0 and ignore port writes.

## Running the Processor

Once the processor's memory and port busses have been configured you can set the start address by directly accessing the IP register:

```cs
// Start execution after the interrupt vector table
cpu.IP = 0x0x0400;  
```

To make the processor run, call the Run() method indicating how many instructions to execute:

```cs
// Run the next 10,000 instructions
cpu.Run(10000);
```

The processor will now run, reading instructions from the memory bus and executing them.

## About Run Frames

The above call to `Run()` executes what's called a "run frame".  A run frame is set of instructions executed as a group.  Executing multiple instructions in run frames is considerably faster than calling a Step() method (if it existed) many times.

Normally a run frame will execute until the specified number of instructions have been executed however you can cancel the current run frame by calling the `CPU.AbortRunFrame()` method while in a callback from a bus operation, or an interrupt handler (see below)

You might want to do this if the processor executes an instruction that should cause the machine to shut down (eg: calling a DOS exit process interupt) and no more instructions should be executed.

## Implementing Interrupt Handlers

Aside from the memory bus and port bus, the other main way the CPU can interact with the hosting environment is via interrupts.

To install an interrupt handler, override the `CPU` class's `RaiseInterrupt` method:

```cs
public override void RaiseInterrupt(byte interruptNumber)
{
    // DOS interrupt?
    if (interruptNumber == 0x21)
    {
        // Handle the interrupt by directly interacting
        // with the CPU registers and machine memory
        HandleDosInterrupt(interruptNumber);

        // Don't pass to the default handler
        return;
    }

    // Do default (ie: invoke interrupt vector table)
    base.RaiseInterrupt(interruptNumber);
}
```

By returning from `RaiseInterrupt` without calling the base method implementation the CPU will continue execution at the instruction immediately after the interrupt.

Normally an interrupt handler will handle the interrupt by directly interacting with the machine's memory and/or registers:

```cs
void HandleDosInterrupt(interruptNumber)
{
    switch (_cpu.ah)
    {
        // handle DOS interrupt sub-op
    }

    // Clear carry flag to indicate no error
    _cpu.FlagC = false;
}
```

## Built-in Debugger

Sharp86 includes a built in debugger.  To enable the debugger, create an instance of it and set its CPU property:

```cs
// Create a debugger and attach it to the CPU
_debugger = new Sharp86.TextGuiDebugger();
_debugger.CPU = _cpu;
```

To break the debugger at the on the next executed instruction, call it's `Break()` method:

```cs
if (IsKeyPress && KeyCode == KeyCode.F9)
{
    debugger.Break();
}
```

Calling `Break()` will cause execution to stop just before the next instruction is executed.  (ie: the processor needs to be running for the break to occur - either by calling `Run()` again, or by calling `Break()` while a call to `Run()` is in progress).

## Using the Debugger

The built in debugger is displayed as a Windows console mode window with a GUIish retro 90's style text mode user-interface.  The GUI is somewhat limited (ie: unfortunately no mouse support) but considerably better than a scrolling console mode debugger.

The basics of using the debugger are as follows:

* F5 = Continue
* F8 = Step
* Shift+F8 = Step Out
* F9 = Set breakpoint (in code window)
* F10 = Step Over
* Ctrl+Tab and Ctrl+Shift+Tab move focus between windows (aka: panels)
* Type over an address to change position (code and data windows)
* Shift + Up/Down in console window to scroll (but, see notes below)
* Type "help" in the console window for list of other commands.

By default, some of the above keys won't work in newer versions of Windows. To fix this, click the debugger window's system menu, choose Properties and turn off: "Quick Edit Mode", "Insert Mode", "Enable Ctrl Key Shortcuts", "Filter clipboard contents on paste", "Enable line wrapping selection" and "Extended text selection keys".

## Debugger Commands

The debugger supports the following set of commands that can be typed into the console window (from the `help` command):

```
                  bp - Set a break point at a code location
            bp break - Attach a break condition to a break point
            bp clear - Remove all break points
          bp cputime - Set a CPU time break point
              bp del - Delete break point
             bp edit - Edit a break point
             bp expr - Set a expression change break point
              bp int - Set a interrupt break point
             bp list - List break points
            bp match - Attach a match condition to a break point
              bp mem - Set a memory change break point
             bp memr - Set a memory read break point
             bp memw - Set a memory write break point
              bp off - Disable break point
               bp on - Enable break point
            bp reset - Reset break point trip counts
           close log - Close log file
              disasm - Disassemble (format=b,w,dw,i,l)
            dump mem - Dump memory (format=b,w,dw,i,l)
                   e - Evaluate an expression
                help - List all available commands
                 log - Log output to a file
                   o - Step Over
                   r - Run
               r for - Run for CPU cycles
              r time - Run to CPU time
                r to - Run to address
                   s - Step
                   t - Step Out
               trace - Dump execution trace
         trace clear - Clear execution trace
           trace off - Disable execution tracing
            trace on - Enable execution tracing
           trace set - Set the execution trace buffer size
           view code - View address in the code window
            view mem - View address in the memory window
                   w - Add a watch expression
             w clear - Remove all watch expression
               w del - Remove a watch expression
              w edit - Edit a watch point
```

Some commands expect arguments in which case you can use expressions.  Most C# style operators are supported.

eg: This would move the memory window to watch 16 bytes before the current stack pointer.

```
view mem ss:sp-0x10
```

Note that commands that expect strings should be quoted:

```
log "dump.txt"
```

eg: watch to the current top of stack:

```
w word ptr [ss:sp]
```
eg: set a break point at code location:

```
bp cs:0x1000
```
eg: set a break point on a write to memory:

```
bp memw ds:0x1234,16
```

Aside from watch expressions, all expressions are evaluted at the time the command is invoked (ie: the above break point wouldn't move if the DS register changed).

Note too that break points are evaluated for exact matches on the segment/offset.  For memory models where multiple addresses refer to the same memory location, break points only work on a matching segment address.

## License

Sharp86 - 8086 Emulator
Copyright &copy; 2017-2018 Topten Software.

Sharp86 is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Sharp86 is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Sharp86.  If not, see <http://www.gnu.org/licenses/>.
