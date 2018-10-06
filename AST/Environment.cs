using System;
using System.Collections.Generic;
using System.Text;
using Vena.Lexer;

namespace Vena.AST
{
    public class Environment
    {
        private Dictionary<string, VType> Values = new Dictionary<string, VType>();

        public bool Define(Token name, VType type)
        {
            if (!Values.TryAdd(name.Lexeme, type))
            {
                string definedType = Enum.GetName(typeof(VType), Values[name.Lexeme]);
                VenaError.ParseError(name, $"Variable '{name.Lexeme}' already defined with type '{definedType}'.");
                return false;
            }
            return true;
        }

        public bool ValidAssignment(Token name, VType assignType)
        {
            // Check to see if the token has already been defined.
            if (!Values.ContainsKey(name.Lexeme))
            {
                VenaError.ParseError(name, $"Undefined variable '{name.Lexeme}'.");
                return false;
            }

            // Check to see if the defined token matches the assignee type.
            if (Values[name.Lexeme] == assignType)
            {
                return true;
            }

            VenaError.ParseError(name,
                $"Assigning type '{Enum.GetName(typeof(VType), assignType)}' to already defined type '{Enum.GetName(typeof(VType), Values[name.Lexeme])}'.");
            return false;
        }
    }
}
