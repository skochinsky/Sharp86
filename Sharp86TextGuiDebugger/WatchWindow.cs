using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConFrames;

namespace Sharp86
{
    public class WatchWindow : Window
    {
        public WatchWindow(TextGuiDebugger owner) : base("Watch", new Rect(80, 18, 40, 22))
        {
            _owner = owner;
            _owner.SettingsChanged += () => Invalidate();
        }

        public void OnBreak()
        {
            Invalidate();
        }

        public override void OnPaint(PaintContext ctx)
        {
            ctx.ForegroundColor = ConsoleColor.Gray;

            int y = 0;
            foreach (var w in _owner.WatchExpressions)
            {
                ctx.Position = new Point(0, y);
                ctx.Write(string.Format("#{0,2}: {1}", w.Number, string.IsNullOrEmpty(w.Name) ? w.ExpressionText : w.Name));

                var val = w.EvalAndFormat(_owner);
                var x = ClientSize.Width - val.Length;
                if (x < 5)
                    x = 5;
                ctx.Position = new Point(x, y);
                ctx.Write(val);
                y++;
            }
        }

        TextGuiDebugger _owner;
    }
}
