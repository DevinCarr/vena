using System;
using System.Collections.Generic;
using System.Text;

namespace Vena.AST
{
    public class Expr : Node
    {
        public virtual VType GetVType() { return VType.VVoid; }
    }

    public class ValueInt : Expr
    {
        protected long Value { get; set; }

        public ValueInt(long value) { Value = value; }
        public ValueInt(string value)
        {
            Value = long.Parse(value, System.Globalization.NumberStyles.Integer);
        }

        public override VType GetVType()
        {
            return VType.VInt;
        }

        public override string ToString()
        {
            return $"[ValueInt {Value}]";
        }
    }
}
