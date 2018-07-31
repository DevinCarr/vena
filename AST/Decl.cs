using System;
using System.Collections.Generic;
using System.Text;

namespace Vena.AST
{
    public class Decl : Node
    {
        protected string Identifier { get; set; }

        public Decl(string identifier) { Identifier = identifier; }
    }

    public class VarDecl : Decl
    {
        protected VType Type { get; set; }

        public VarDecl(string identifier, VType type) : base(identifier)
        {
            Type = type;
        }

        public override string ToString()
        {
            return $"[VarDecl {Identifier} {Type}]";
        }
    }
}
