// Generated by TinyPG v1.1 available at www.codeproject.com

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TinyPG
{
    #region Scanner

    public partial class Scanner
    {
        public string Input;
        public int StartPos = 0;
        public int EndPos = 0;
        public int CurrentLine;
        public int CurrentColumn;
        public int CurrentPosition;
        public List<Token> Skipped; // tokens that were skipped

        private Token LookAheadToken;
        private Dictionary<TokenType, Regex> Patterns;
        private List<TokenType> Tokens;
        private List<TokenType> SkipList; // tokens to be skipped

        public Scanner()
        {
            Regex regex;
            Patterns = new Dictionary<TokenType, Regex>();
            Tokens = new List<TokenType>();
            LookAheadToken = null;

            SkipList = new List<TokenType>();
            SkipList.Add(TokenType.WHITESPACE);

            regex = new Regex(@"[0-9]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.NUMBER, regex);
            Tokens.Add(TokenType.NUMBER);

            regex = new Regex(@"(\+|-)", RegexOptions.Compiled);
            Patterns.Add(TokenType.PLUSMINUS, regex);
            Tokens.Add(TokenType.PLUSMINUS);

            regex = new Regex(@"\*|/", RegexOptions.Compiled);
            Patterns.Add(TokenType.MULTDIV, regex);
            Tokens.Add(TokenType.MULTDIV);

            regex = new Regex(@"\(", RegexOptions.Compiled);
            Patterns.Add(TokenType.BROPEN, regex);
            Tokens.Add(TokenType.BROPEN);

            regex = new Regex(@"\)", RegexOptions.Compiled);
            Patterns.Add(TokenType.BRCLOSE, regex);
            Tokens.Add(TokenType.BRCLOSE);

            regex = new Regex(@"^$", RegexOptions.Compiled);
            Patterns.Add(TokenType.EOF, regex);
            Tokens.Add(TokenType.EOF);

            regex = new Regex(@"\s+", RegexOptions.Compiled);
            Patterns.Add(TokenType.WHITESPACE, regex);
            Tokens.Add(TokenType.WHITESPACE);


        }

        public void Init(string input)
        {
            this.Input = input;
            StartPos = 0;
            EndPos = 0;
            CurrentLine = 0;
            CurrentColumn = 0;
            CurrentPosition = 0;
            Skipped = new List<Token>();
            LookAheadToken = null;
        }

        public Token GetToken(TokenType type)
        {
            Token t = new Token(this.StartPos, this.EndPos);
            t.Type = type;
            return t;
        }

         /// <summary>
        /// executes a lookahead of the next token
        /// and will advance the scan on the input string
        /// </summary>
        /// <returns></returns>
        public Token Scan(params TokenType[] scantokens)
        {
            Token tok = LookAhead(scantokens); // temporarely retrieve the lookahead
            LookAheadToken = null; // reset lookahead token, so scanning will continue
            StartPos = tok.EndPos;
            EndPos = tok.EndPos; // set the tokenizer to the new scan position
            return tok;
        }

        /// <summary>
        /// returns token with longest best match
        /// </summary>
        /// <returns></returns>
        public Token LookAhead(params TokenType[] scantokens)
        {
            int len;
            int index = -1;
            int i;
            int startpos = StartPos;
            Token tok = null;


            // this prevents double scanning and matching
            // increased performance
            if (LookAheadToken != null 
                && LookAheadToken.Type != TokenType._UNDETERMINED_ 
                && LookAheadToken.Type != TokenType._NONE_) return LookAheadToken;

            // if no scantokens specified, then scan for all of them (= backward compatible)
            if (scantokens.Length == 0)
                scantokens = Tokens.ToArray();

            do
            {

                len = -1;
                string input = Input.Substring(startpos);

                tok = new Token(startpos, EndPos);

                for (i = 0; i < scantokens.Length; i++)
                {
                    Regex r = Patterns[scantokens[i]];
                    Match m = r.Match(input);
                    if (m.Success && m.Index == 0 && m.Length > len)
                    {
                        len = m.Length;
                        index = i;  
                    }
                }

                if (index >= 0 && len >= 0)
                {
                    tok.EndPos = startpos + len;
                    tok.Text = Input.Substring(tok.StartPos, len);
                    tok.Type = scantokens[index];
                }
                else
                {
                    if (tok.EndPos < Input.Length)
                        tok.Text = Input.Substring(tok.StartPos, 1);
                }

                if (SkipList.Contains(tok.Type))
                {
                    startpos = tok.EndPos;
                    Skipped.Add(tok);
                }
            }
            while (SkipList.Contains(tok.Type));

            LookAheadToken = tok;
            return tok;
        }
    }

    #endregion

    #region Token

    public enum TokenType
    {

            //Non terminal tokens:
            _NONE_  = 0,
            _UNDETERMINED_= 1,

            //Non terminal tokens:
            Start   = 2,
            AddExpr = 3,
            MultExpr= 4,
            Atom    = 5,

            //Terminal tokens:
            NUMBER  = 6,
            PLUSMINUS= 7,
            MULTDIV = 8,
            BROPEN  = 9,
            BRCLOSE = 10,
            EOF     = 11,
            WHITESPACE= 12
    }

    public class Token
    {
        private int startpos;
        private int endpos;
        private string text;
        private object value;

        public int StartPos { 
            get { return startpos;} 
            set { startpos = value; }
        }

        public int Length { 
            get { return endpos - startpos;} 
        }

        public int EndPos { 
            get { return endpos;} 
            set { endpos = value; }
        }

        public string Text { 
            get { return text;} 
            set { text = value; }
        }

        public object Value { 
            get { return value;} 
            set { this.value = value; }
        }

        public TokenType Type;

        public Token()
            : this(0, 0)
        {
        }

        public Token(int start, int end)
        {
            Type = TokenType._UNDETERMINED_;
            startpos = start;
            endpos = end;
            Text = ""; // must initialize with empty string, may cause null reference exceptions otherwise
            Value = null;
        }

        public void UpdateRange(Token token)
        {
            if (token.StartPos < startpos) startpos = token.StartPos;
            if (token.EndPos > endpos) endpos = token.EndPos;
        }

        public override string ToString()
        {
            if (Text != null)
                return Type.ToString() + " '" + Text + "'";
            else
                return Type.ToString();
        }
    }

    #endregion
}
