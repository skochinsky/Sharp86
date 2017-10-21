using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaJson;

namespace Sharp86
{
    class ExpressionBreakPoint : BreakPoint
    {
        public ExpressionBreakPoint()
        {
        }

        public ExpressionBreakPoint(Expression expression)
        {
            _expression = expression;
        }

        [Json("expression")]
        public string ExpressionText
        {
            get { return _expression == null ? null : _expression.OriginalExpression; }
            set { _expression = value == null ? null : new Expression(value); }
        }

        Expression _expression;
        object _prevValue;

        public override string ToString()
        {
            return base.ToString(string.Format("expr {0}", _expression == null ? "null" : _expression.OriginalExpression));
        }

        public override string EditString
        {
            get
            {
                return string.Format("expr {0}", _expression.OriginalExpression);
            }
        }

        public override bool ShouldBreak(DebuggerCore debugger)
        {
            var newValue = _expression.Eval(debugger.ExpressionContext);

            if (_prevValue == null)
            {
                _prevValue = newValue;
                return false;
            }

            bool changed = (bool)Operators.compare_ne(newValue, _prevValue);
            _prevValue = newValue;
            return changed;
        }
    }
}
