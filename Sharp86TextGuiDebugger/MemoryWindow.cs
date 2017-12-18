/*
Sharp86 - 8086 Emulator
Copyright (C) 2017 Topten Software.

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
*/

using System;
using System.Reflection;
using ConFrames;
using PetaJson;

namespace Sharp86
{
    public class MemoryWindow : Window
    {
        public MemoryWindow(TextGuiDebugger debugger) 
            : base("Memory", new Rect(0, 18, 80, 12))
        {
            _debugger = debugger;
            _segSet = false;
            _seg = 0;
            _offset = 0;
            CursorVisible = true;
            ClearAttributes = ConFrames.Attribute.Make(ConsoleColor.Gray, ConsoleColor.Blue);
        }

        bool _segSet = false;
        ushort _seg;
        ushort _offset;
        TextGuiDebugger _debugger;

        [Json("address")]
        public uint Address
        {
            get
            {
                return (uint)(_seg << 16 | _offset);
            }
            set
            {
                _seg = (ushort)(value >> 16);
                _offset = (ushort)(value & 0xFFFF);
                _segSet = true;
            }
        }


        int BytesPerRow
        {
            get
            {
                return 16;
            }

        }

        public void MoveTo(ushort seg, ushort offset)
        {
            _seg = seg;
            _offset = offset;
            CursorX = 0;
            CursorY = 0;
            Invalidate();
        }
       
        public override void OnPaint(PaintContext ctx)
        {
            var bus = _debugger.CPU.MemoryBus;

            ctx.ForegroundColor = ConsoleColor.Gray;

            ctx.ClearLineOnReturn = false;

            // Get memory state
            var displayedBytes = (ushort)(BytesPerRow * ClientSize.Height);
            var memoryState = new MemoryState[displayedBytes];
            IMemoryBusDebug debugBus = _debugger.MemoryBus as IMemoryBusDebug;
            if (debugBus != null)
            {
                debugBus.GetMemoryState(_seg, _offset, memoryState);
            }
            else
            {
                for (int i = 0; i < displayedBytes; i++)
                {
                    memoryState[i] = MemoryState.Valid;
                }
            }

            ushort modAttributes = ConFrames.Attribute.Make(ConsoleColor.White, ConsoleColor.Blue);

            int bytesPerFormat = 1;
            bool showChars = true;
            switch (_viewFormat)
            {
                case ViewFormat.DWord:
                    bytesPerFormat = 4;
                    break; 

                case ViewFormat.Long:
                    bytesPerFormat = 4;
                    showChars = false;
                    break;

                case ViewFormat.Word:
                    bytesPerFormat = 2;
                    break;

                case ViewFormat.Int:
                    bytesPerFormat = 2;
                    showChars = false;
                    break;
            }

            ushort offset = _offset;
            for (int i=0; i<ClientSize.Height; i++)
            {
                ctx.ForegroundColor = ConsoleColor.Gray;
                ctx.Write("{0:X4}:{1:X4} ", _seg, offset);

                ctx.SetChar(58, ctx.Position.Y, (char)'|', ctx.Attributes);
                ctx.SetChar(58+17, ctx.Position.Y, (char)'|', ctx.Attributes);

                for (int x = 0; x<BytesPerRow; x += bytesPerFormat)
                {
                    MemoryState dataState = MemoryState.ValidUnchanged;


                    uint data = 0;
                    for (int bi=0; bi<bytesPerFormat; bi++)
                    {
                        // Work out if modified
                        var byteState = memoryState[offset - _offset + bi];
                        dataState |= (byteState & MemoryState.Changed);

                        if ((byteState & MemoryState.Valid)!=0)
                        {
                            try
                            {
                                var b = bus.ReadByte(_seg, (ushort)(offset + bi));
                                data = (uint)(data | (uint)(b << (8 * bi)));

                                if (b >= 32 && showChars)
                                {
                                    ctx.ForegroundColor = (byteState & MemoryState.Changed)!=0 ? ConsoleColor.White : ConsoleColor.Gray;
                                    ctx.SetChar(x + 59 + bi, ctx.Position.Y, (char)b, ctx.Attributes);
                                }
                            }
                            catch (CPUException)
                            {
                                byteState &= ~MemoryState.Valid;
                            }
                        }
                        else
                        {
                            dataState &= ~MemoryState.Valid;
                        }
                    }

                    ctx.ForegroundColor = (dataState & MemoryState.Changed)!=0 ? ConsoleColor.White : ConsoleColor.Gray;

                    switch (_viewFormat)
                    {
                        case ViewFormat.Byte:
                            if ((dataState & MemoryState.Valid)!=0)
                                ctx.Write("{0:X2} ", (byte)data);
                            else
                                ctx.Write("?? ");
                            break;

                        case ViewFormat.Word:
                            if ((dataState & MemoryState.Valid) != 0)
                                ctx.Write("{0:X4} ", (ushort)data);
                            else
                                ctx.Write("???? ");
                            break;

                        case ViewFormat.DWord:
                            if ((dataState & MemoryState.Valid) != 0)
                                ctx.Write("{0:X8} ", (uint)data);
                            else
                                ctx.Write("???????? ");
                            break;

                        case ViewFormat.Int:
                            if ((dataState & MemoryState.Valid) != 0)
                                ctx.Write("{0,6} ", (short)data);
                            else
                                ctx.Write("     ? ");
                            break;

                        case ViewFormat.Long:
                            if ((dataState & MemoryState.Valid) != 0)
                                ctx.Write("{0,11} ", (int)data);
                            else
                                ctx.Write("          ? ");
                            break;

                    }

                    offset = (ushort)(offset + bytesPerFormat);
                }

                ctx.WriteLine("");
            }
        }

        [Obfuscation(Exclude = true, ApplyToMembers = true)]
        enum ViewFormat
        {
            Byte,
            Word,
            DWord,
            Int,
            Long,
        }

        public override bool OnKey(ConsoleKeyInfo key)
        {
            // Hex digit?
            int hexDigit = -1;
            if (key.KeyChar>='0' && key.KeyChar<='9')
            {
                hexDigit = key.KeyChar - '0';
            }
            else if (key.KeyChar >= 'A' && key.KeyChar <='F')
            {
                hexDigit = key.KeyChar - 'A' + 10;
            }
            else if (key.KeyChar >= 'a' && key.KeyChar <='f')
            {
                hexDigit = key.KeyChar - 'a' + 10;
            }

            if (hexDigit>=0)
            {
                if (CursorX == 4)
                    CursorX = 5;

                if (CursorPosition.X >=0 && CursorPosition.X <= 3)
                {
                    var shift = (3 - CursorPosition.X) * 4;
                    var mask = 0x0F << shift;
                    _seg = (ushort)((_seg & ~mask) | (hexDigit << shift));

                    CursorX++;
                    Invalidate();
                    return true;
                }

                if (CursorPosition.X >= 5 && CursorPosition.X <= 8)
                {
                    var shift = (3 - (CursorPosition.X - 5)) * 4;
                    var mask = 0x0F << shift;
                    var lineOffset = _offset + CursorPosition.Y * BytesPerRow;
                    var offset = (ushort)((lineOffset & ~mask) | (hexDigit << shift));
                    _offset = (ushort)(offset - CursorPosition.Y * BytesPerRow);

                    CursorX++;
                    Invalidate();
                    return true;
                }
            }

            if (key.KeyChar ==' ' || key.KeyChar ==':')
            {
                if (CursorX < 74)
                    CursorX++;
            }

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.Backspace:
                    if (CursorPosition.X > 0)
                        CursorX--;
                    break;

                case ConsoleKey.RightArrow:
                    if (CursorPosition.X < 74)
                        CursorX++;
                    break;

                case ConsoleKey.UpArrow:
                    if (CursorY > 0)
                    {
                        CursorY--;
                    }
                    else
                    {
                        if (_offset > BytesPerRow)
                            _offset = (ushort)(_offset - BytesPerRow);
                        else
                            _offset = 0;
                        Invalidate();
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (CursorY + 1 < ClientSize.Height)
                        CursorY++;
                    else
                    {
                        if (_offset + BytesPerRow < 0xFFF0)
                            _offset = (ushort)(_offset + BytesPerRow);
                        else
                            _offset = 0xFFF0;
                        Invalidate();
                    }
                    break;

            }

            switch (key.KeyChar)
            {
                case 'b':
                case 'B':
                    _viewFormat = ViewFormat.Byte;
                    Invalidate();
                    break;

                case 'w':
                case 'W':
                    _viewFormat = ViewFormat.Word;
                    Invalidate();
                    break;

                case 'd':
                case 'D':
                    _viewFormat = ViewFormat.DWord;
                    Invalidate();
                    break;

                case 'i':
                case 'I':
                    _viewFormat = ViewFormat.Int;
                    Invalidate();
                    break;

                case 'l':
                case 'L':
                    _viewFormat = ViewFormat.Long;
                    Invalidate();
                    break;
            }

            return base.OnKey(key);
        }

        [Json("viewFormat")]
        ViewFormat _viewFormat = ViewFormat.Byte;

        public void OnBreak()
        {
            if (!_segSet && _seg == 0)
            {
                _seg = _debugger.CPU.ds;
                _segSet = true;
            }

            Invalidate();
        }
    }
}
