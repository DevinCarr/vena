using System;
using System.Collections.Generic;
using System.Text;

namespace Vena.AST
{
    public abstract class Node
    {
    }

    public class VType
    {
        public static readonly VType VVoid = new VType(String.Intern("void"));
        public static readonly VType VInt = new VType(String.Intern("int"));
        public static readonly VType VDouble = new VType(String.Intern("double"));
        public static readonly VType VString = new VType(String.Intern("string"));
        public static readonly Dictionary<string,VType> VenaTypes = new Dictionary<string, VType>()
        {
            { VVoid.ToString(), VVoid },
            { VInt.ToString(), VInt },
            { VDouble.ToString(), VDouble },
            { VString.ToString(), VString }
        };

        private string TypeName { get; set; }
        private VType(string typename) { TypeName = typename; }

        public override string ToString()
        {
            return TypeName;
        }
    }
}
