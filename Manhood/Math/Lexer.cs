﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Manhood
{
    internal sealed partial class Lexer : IEnumerable<Token>
    {
        private static readonly Regex NumberRegex = new Regex(@"-?(\d+(\.\d+)?|\.\d+)");
        private readonly string _string;
        private int _pos;
        
        public Lexer(string input)
        {
            _pos = 0;
            _string = input;
        }

        public static bool IsValueChar(char c)
        {
            return Char.IsLetterOrDigit(c) || "_".Contains(c);
        }

        public IEnumerator<Token> GetEnumerator()
        {
            while (_pos < _string.Length)
            {
                int start = _pos;
                char c = _string[_pos++];
                Match match = null;
                
                if ((match = NumberRegex.Match(_string, start)).Success && match.Index == start)
                {
                    yield return new Token(_string.Substring(start, match.Length), TokenType.Number);
                    _pos = start + match.Length;
                }
                else if (IsValueChar(c)) // Variable
                {
                    while (_pos < _string.Length)
                    {
                        if (IsValueChar(_string[_pos]))
                        {
                            _pos++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    yield return new Token(_string.Substring(start, _pos - start), TokenType.Name);
                }
                else if (Punctuation.Contains(c)) // Operator
                {
                    bool found = false;
                    foreach (var o in Operators)
                    {
                        if (_string.IndexOf(o.Item1, start, StringComparison.Ordinal) != start) continue;
                        _pos = start + o.Item1.Length;
                        yield return new Token(o.Item1, o.Item2);
                        found = true;
                        break;
                    }
                    if (!found) throw new ManhoodException("Invalid token '" + c + "' in expression \"" + _string + "\".");
                }
                else if (!Char.IsWhiteSpace(c) && !Char.IsControl(c)) // No strange symbols allowed
                {
                    throw new ManhoodException("Invalid token '" + c + "' in expression \"" + _string + "\".");
                }

            }
            yield return new Token("", TokenType.End);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}