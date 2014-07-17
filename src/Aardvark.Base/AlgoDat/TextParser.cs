﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Aardvark.Base
{
    #region State

    /// <summary>
    /// State of a generic recursive descent parser.
    /// </summary>
    /// <typeparam name="TPar">Parser class containing static state variables.</typeparam>
    /// <typeparam name="TNode">Node class for building parse trees.</typeparam>
    public class State<TPar, TNode>
        where TPar : TextParser<TPar>
    {
        public readonly Action<TPar, TNode, Text> TextAct;
        public readonly Case<TPar, TNode>[] Cases;
        public readonly Func<TPar, State<TPar, TNode>, TNode, int> Match;

        #region Constructors

        public State(Cases<TPar, TNode> cases)
            : this(cases.Array, null)
        { }

        public State(Case<TPar, TNode>[] cases)
            : this(cases, null)
        { }

        public State(Cases<TPar, TNode> cases, Action<TPar, TNode, Text> textAct)
            : this(cases.Array, textAct)
        { }

        public State(Case<TPar, TNode>[] cases, Action<TPar, TNode, Text> textAct)
        {
            Cases = cases;
            TextAct = textAct;

            var pattern = cases.Copy(
                (c, i) => c.Pattern != null
                            ? "(?<" + (i + 1) + ">" + c.Pattern + ")"
                            : "(?<" + (i + 1) + ">.|\r)"
            ).Join("|");

            var regex = new Regex(pattern, RegexOptions.Singleline
                                           | RegexOptions.ExplicitCapture
                                           | RegexOptions.Compiled);
            var caseCount = cases.Length;

            if (textAct != null)
            {
                Match = (par, state, node) =>
                {
                    var m = regex.Match(par.Text.String, par.Pos, par.Text.End - par.Pos);
                    if (m.Success)
                        for (int i = 1; i <= caseCount; i++)
                            if (m.Groups[i].Success)
                            {
                                var index = m.Groups[i].Index;
                                if (par.Pos < index)
                                {
                                    var t = par.PeekToPos(index);
                                    var adj = state.Cases[i - 1].AdjustFun;
                                    if (adj != null) t = adj(par, node, t);
                                    state.TextAct(par, node, t);
                                    par.SetPosAndCountLines(index);
                                }
                                par.LastEnd = index + m.Groups[i].Length;
                                return i - 1;
                            }
                    if (par.Pos < par.Text.End)
                    {
                        state.TextAct(par, node, par.PeekToPos(par.Text.End));
                        par.SetPosAndCountLines(par.Text.End);
                    }
                    return -1;
                };
            }
            else
            {
                if (cases[caseCount - 1].Pattern != null)
                    throw new ArgumentException(
                        "either a text action or a default case needs to be specified");

                Match = (par, state, node) =>
                {
                    var m = regex.Match(par.Text.String, par.Pos, par.Text.End - par.Pos);
                    if (m.Success)
                        for (int i = 1; i <= caseCount; i++)
                            if (m.Groups[i].Success)
                            {
                                par.LastEnd = m.Groups[i].Index + m.Groups[i].Length;
                                return i - 1;
                            }
                    return -1;
                };
            }
        }

        #endregion
    }

    #endregion

    #region Case

    /// <summary>
    /// Case represents a state transition of a generic recursive descent parser.
    /// </summary>
    /// <typeparam name="TPar">Parser class containing static state variables.</typeparam>
    /// <typeparam name="TNode">Node class for building parse trees.</typeparam>
    public struct Case<TPar, TNode>
        where TPar : TextParser<TPar>
    {
        public readonly Func<TPar, TNode, State<TPar, TNode>> MatchedFun;
        public readonly Func<TPar, TNode, Text, Text> AdjustFun;
        public readonly string Pattern;

        #region Constructors

        public Case(Func<TPar, TNode, State<TPar, TNode>> matchedFun)
            : this(null, matchedFun)
        { }

        public Case(Rx rx, Func<TPar, TNode, State<TPar, TNode>> matchedFun)
            : this(rx.Pattern, matchedFun)
        { }

        public Case(string pattern, Func<TPar, TNode, State<TPar, TNode>> matchedFun)
            : this(pattern, matchedFun, null)
        { }

        public Case(string pattern, Func<TPar, TNode, State<TPar, TNode>> matchedFun,
                    Func<TPar, TNode, Text, Text> adjustFun)
        {
            MatchedFun = matchedFun;
            AdjustFun = adjustFun;
            Pattern = pattern;
        }

        #endregion
    }

    #endregion

    #region Cases

    /// <summary>
    /// The Cases class makes it possible to build a single state transition
    /// table by combining already defined case arrays. This makes it possible
    /// to avoid re-specifying cases that appear in multiple states.
    /// </summary>
    public class Cases<TPar, TNode> : IEnumerable<Case<TPar, TNode>>
        where TPar : TextParser<TPar>
    {
        private List<Case<TPar, TNode>> CaseList;

        public Case<TPar, TNode>[] Array
        {
            get { return CaseList.ToArray(); }
        }

        public Cases()
        {
            CaseList = new List<Case<TPar, TNode>>();
        }

        public Cases(Rx[] rxs, Func<TPar, TNode, State<TPar, TNode>> matchedFun)
            : this()
        {
            foreach (var rx in rxs)
                CaseList.Add(new Case<TPar, TNode>(rx, matchedFun));
        }

        public void Add(Case<TPar, TNode> singleCase)
        {
            CaseList.Add(singleCase);
        }

        public void Add(Case<TPar, TNode>[] caseArray)
        {
            foreach (var c in caseArray) CaseList.Add(c);
        }

        public void Add(Cases<TPar, TNode> cases)
        {
            foreach (var c in cases.Array) CaseList.Add(c);
        }

        #region IEnumerable<Case<TPar,TNode>> Members

        public IEnumerator<Case<TPar, TNode>> GetEnumerator()
        {
            return CaseList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return CaseList.GetEnumerator();
        }

        #endregion
    }

    #endregion

    #region TextParser

    /// <summary>
    /// The recursive descent parser class contains parsing state and serves
    /// as a base class for specific parsers implementations. The specific
    /// derived parser class needs to be specified as type parameter. It
    /// should also contain the state transition table as static member
    /// variables.
    /// </summary>
    /// <typeparam name="TPar">A concrete parser class derived from
    /// this generic parser class.</typeparam>
    public class TextParser<TPar>
        where TPar : TextParser<TPar>
    {
        public Text Text;
        public int Pos;
        public int LastEnd;
        public Text.Line Line;
        public int LastWhiteSpace;
        private int m_currentCaseIndex;

        #region Static Parsing Methods

        /// <summary>
        /// Parse the supplied text into the supplied node, starting the
        /// parser in the supplied root state. This function is the entry
        /// call to the parser, it returns when the text has been fully
        /// parsed, or throws a <see cref="ParserException"/>.
        /// </summary>
        public static TPar Parse<TNode>(
                Text text,
                TPar parser, State<TPar, TNode> rootState, TNode rootNode)
        {
            parser.Text = text;
            parser.Pos = text.Start;
            parser.LastEnd = text.Start;
            parser.Line = new Text.Line(0, text.Start);
            parser.LastWhiteSpace = -1;
            parser.m_currentCaseIndex = -1;

            return Parse(parser, rootState, rootNode);
        }

        /// <summary>
        /// Parse a part of the input into the supplied node.
        /// This function can be called to implement recursive descent.
        /// </summary>
        public static TPar Parse<TNode>(
                TPar parser, State<TPar, TNode> state, TNode node)
        {
            int end = parser.Text.End;
            var match = state.Match(parser, state, node);
            while (parser.Pos < end)
            {
                parser.m_currentCaseIndex = match;
                state = state.Cases[match].MatchedFun(parser, node);
                if (state == null) break;
                match = state.Match(parser, state, node);
            }
            return parser;
        }

        #endregion

        #region Properties

        /// <summary>
        /// True if parsing has consumed all input.
        /// </summary>
        public bool EndOfText
        {
            get { return Pos == Text.End; }
        }

        /// <summary>
        /// Users start counting at 1.
        /// </summary>
        public int UserLine { get { return 1 + Line.Index; } }

        /// <summary>
        /// Users start counting at 1.
        /// </summary>
        public int UserColumn { get { return 1 + Pos - Line.Start; } }

        /// <summary>
        /// The index of the current case within the state table.
        /// This is only valid as long as no recursive parse function
        /// has been called.
        /// </summary>
        public int CurrentCaseIndex
        {
            get { return m_currentCaseIndex; }
        }
        
        #endregion

        #region Current State Query Methods

        /// <summary>
        /// The current case within the supplied state table.
        /// This is only valid as long as no recursive parse function
        /// has been called.
        /// </summary>
        public Case<TPar, TNode> CurrentCase<TNode>(State<TPar, TNode> state)
        {
            return state.Cases[m_currentCaseIndex];
        }

        /// <summary>
        /// The current pattern within the supplied state table.
        /// This is only valid as long as no recursive parse function
        /// has been called.
        /// </summary>
        public string CurrentPattern<TNode>(State<TPar, TNode> state)
        {
            return state.Cases[m_currentCaseIndex].Pattern;
        }

        #endregion

        #region Skipping Text

        public void SkipToEnd()
        {
            Pos = Text.End;
        }

        /// <summary>
        /// Skip the last pattern that was matched up to its start. This can
        /// either be the pattern of the case, or the pattern that is left
        /// by one of the GetToStartOf(...) methods.
        /// </summary>
        public void Skip()
        {
            Pos = LastEnd;
        }

        /// <summary>
        /// Skip exactly count characters.
        /// </summary>
        public void Skip(int count)
        {
            Pos += count;
        }

        public void SkipAndCountLines()
        {
            SetPosAndCountLines(LastEnd);
        }

        public void SkipAndCountLines(int count)
        {
            SetPosAndCountLines(Pos + count);
        }

        public bool TrySkip(int count)
        {
            var pos = Pos + count;
            if (pos > Text.End) return false;
            SetPosAndCountLines(pos);
            return true;
        }

        public void Skip(char skipCh)
        {
            if (!TrySkip(skipCh)) ThrowCouldNotSkip("'" + skipCh + "'");
        }

        public void Skip(string skipStr)
        {
            if (!TrySkip(skipStr)) ThrowCouldNotSkip('"' + skipStr + '"');
        }

        public void Skip(Rx skipRx)
        {
            if (!TrySkip(skipRx))
                ThrowCouldNotSkip("pattern \"" + skipRx.Pattern + '"');
        }

        public bool TrySkip(char skipCh)
        {
            var index = Math.Max(Pos, LastEnd);
            var end = index + 1;
            if (end >= Text.End) return false;
            if (Text[index] != skipCh) return false;
            SetPosAndCountLines(end);
            return true;
        }

        public bool TrySkip(string skipStr)
        {
            var index = Math.Max(Pos, LastEnd);
            var end = index + skipStr.Length;
            if (end > Text.End) return false;
            for (int i = 0; index < end; index++, i++)
                if (Text[index] != skipStr[i]) return false;
            SetPosAndCountLines(index);
            return true;
        }

        public bool TrySkip(Rx skipRx)
        {
            var index = Math.Max(Pos, LastEnd);
            var m = skipRx.AnchoredRegex.Match(Text.String, index, Text.End - index);
            if (!m.Success || m.Groups[2].Success) return false;
            SetPosAndCountLines(index); LastEnd = index + m.Length;
            return true;
        }

        /// <summary>
        /// Skip as much white space characters as possible. Returns the
        /// actual number skipped.
        /// </summary>
        public int SkipWhiteSpace()
        {
            var start = Pos;
            while (Pos < Text.End)
            {
                var c = Text.String[Pos];
                switch (c)
                {
                    case ' ':
                    case '\t':
                    case '\r': ++Pos; break;
                    case '\n':
                        ++Pos; Line.Index++; Line.Start = Pos; break;
                    default: return Pos - start;
                }
            }
            return Pos - start;
        }

        /// <summary>
        /// Skip at least minimalCount white space characters.
        /// Throws a ParserException if less white space is available.
        /// </summary>
        public void SkipWhiteSpaceAtLeast(int minimalCount)
        {
            if (SkipWhiteSpace() < minimalCount)
                throw new ParserException<TPar>((TPar)this,
                    "not enough white space");
        }

        public void SkipWhiteSpaceAndCheckProgress()
        {
            if (Pos == LastWhiteSpace) // stall detected
                throw new ParserException<TPar>((TPar)this, "illegal character");
            SkipWhiteSpace(); LastWhiteSpace = Pos;
        }

        public void SkipToStartOf(char searchChar)
        {
            if (!TrySkipToStartOf(searchChar))
                ThrowCouldNotFind("'" + searchChar + "'");
        }

        public void SkipToStartOf(string searchString)
        {
            if (!TrySkipToStartOf(searchString))
                ThrowCouldNotFind('"' + searchString + '"');
        }

        public void SkipToStartOf(Rx searchRx)
        {
            if (!TrySkipToStartOf(searchRx))
                ThrowCouldNotFind("pattern \"" + searchRx.Pattern + '"');
        }

        public void SkipToEndOf(char searchChar)
        {
            if (!TrySkipToEndOf(searchChar))
                ThrowCouldNotFind("'" + searchChar + "'");
        }

        public void SkipToEndOf(string searchString)
        {
            if (!TrySkipToEndOf(searchString))
                ThrowCouldNotFind('"' + searchString + '"');
        }

        public void SkipToEndOf(Rx searchRx)
        {
            if (!TrySkipToEndOf(searchRx))
                ThrowCouldNotFind("pattern \"" + searchRx.Pattern + '"');
        }

        public void SkipToEndOfOrEnd(char searchChar)
        {
            if (!TrySkipToEndOf(searchChar)) SkipToEnd();
        }

        public void SkipToEndOfOrEnd(string searchString)
        {
            if (!TrySkipToEndOf(searchString)) SkipToEnd();
        }

        public void SkipToEndOfOrEnd(Rx searchRx)
        {
            if (!TrySkipToEndOf(searchRx)) SkipToEnd();
        }

        public bool TrySkipToStartOf(char searchChar)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchChar, index, Text.End - index);
            if (index < 0) return false;
            SetPosAndCountLines(index); LastEnd = index + 1;
            return true;
        }

        public bool TrySkipToStartOf(string searchString)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchString, index, Text.End - index);
            if (index < 0) return false;
            SetPosAndCountLines(index); LastEnd = index + searchString.Length;
            return true;
        }

        public bool TrySkipToStartOf(Rx searchRx)
        {
            var index = Math.Max(Pos, LastEnd);
            var m = searchRx.Regex.Match(Text.String, index, Text.End - index);
            if (!m.Success) return false;
            index = m.Index;
            SetPosAndCountLines(index); LastEnd = index + m.Length;
            return true;
        }

        public bool TrySkipToEndOf(char searchChar)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchChar, index, Text.End - index);
            if (index < 0) return false;
            SetPosAndCountLines(++index);
            return true;
        }

        public bool TrySkipToEndOf(string searchString)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchString, index, Text.End - index);
            if (index < 0) return false;
            index += searchString.Length;
            SetPosAndCountLines(index);
            return true;
        }

        public bool TrySkipToEndOf(Rx searchRx)
        {
            var index = Math.Max(Pos, LastEnd);
            var m = searchRx.Regex.Match(Text.String, index, Text.End - index);
            if (!m.Success) return false;
            index = m.Index + m.Length;
            SetPosAndCountLines(index);
            return true;
        }

        #endregion

        #region Getting Text

        /// <summary>
        /// Get one character of input.
        /// </summary>
        public char GetChar()
        {
            if (Pos >= Text.End)
                throw new ParserException<TPar>((TPar)this, "end of text");
            return Text.String[Pos++];
        }

        public bool TryGet(char ch)
        {
            if (Pos >= Text.End) return false;
            if (Text.String[Pos] != ch) return false;
            ++Pos;
            return true;
        }

        public bool TryGet(string str)
        {
            var len = str.Length;
            if (Pos + len > Text.End) return false;
            for (int i = 0, p = Pos; i < len; i++, p++)
                if (str[i] != Text.String[p]) return false;
            Pos += len;
            return true;
        }

        /// <summary>
        /// Get the text up to the end of the last pattern that was matched
        /// up to its start. This can either be the pattern of the case,
        /// the pattern that is left by one of the GetToStartOf(...), or
        /// the pattern that was left by one of the SkipToStartOf(...)
        /// methods.
        /// </summary>
        public Text Get()
        {
            var text = new Text(Pos, LastEnd, Text.String);
            Pos = LastEnd;
            return text;
        }

        /// <summary>
        /// Get the text from the supplied start up to the current parsing
        /// position. This can be used, if a number of patterns have been
        /// skipped before and need to be combined into a single text.
        /// </summary>
        public Text GetFrom(int start)
        {
            var text = new Text(start, Pos, Text.String);
            return text;
        }

        /// <summary>
        /// Get text that matches the supplied regular expression (it must
        /// start at the current position). If the text does not match, an
        /// exception is thrown.
        /// </summary>
        public Text Get(Rx rx)
        {
            return Get(rx, p => p.ThrowCouldNotFind(
                                    "pattern \"" + rx.Pattern + '"'));
        }

        /// <summary>
        /// Get text that matches the supplied regular expression (it must
        /// start at the current position). If the text does not match, the
        /// supplied function is executed and its result is returned.
        /// </summary>
        public Text Get(Rx rx, Func<TPar, Text> notMatchedFun)
        {
            var m = rx.AnchoredRegex.Match(Text.String, Pos);
            if (!m.Success || m.Groups[2].Success) return notMatchedFun((TPar)this);
            Pos += m.Length;
            return new Text(Text.String, m.Index, m.Length);
        }

        /// <summary>
        /// Get text [pos, pos + offset) if offset is greater than 0, and
        /// text [pos, end + offset) if offset is lower or equal to 0.
        /// </summary>
        public Text GetTo(int offset)
        {
            return GetToPos(offset <= 0
                            ? Math.Max(Pos, Text.End + offset)
                            : Math.Min(Pos + offset, Text.End));
        }

        public Text GetToPos(int pos)
        {
            var text = new Text(Pos, pos, Text.String);
            SetPosAndCountLines(pos);
            return text;
        }

        public Text GetToEnd()
        {
            return GetToPos(Text.End);
        }

        public List<Text> GetList(Rx item, Rx sep)
        {
            var idx = Math.Max(Pos, LastEnd);
            var list = new List<Text>();
            var m = item.AnchoredRegex.Match(Text.String, idx);
            if (!m.Success || m.Groups[2].Success
                || idx + m.Length > Text.End)
                throw new ParserException<TPar>((TPar)this, idx,
                    "could not get list item with pattern \""
                    + item.Pattern + '"');
            list.Add(new Text(Text.String, m.Index, m.Length));
            idx += m.Length;
            while ((m = sep.AnchoredRegex.Match(Text.String, idx)).Success
                   && m.Groups[1].Success && idx + m.Length < Text.End)
            {
                idx += m.Length;
                m = item.Regex.Match(Text.String, idx);
                if (!m.Success || m.Groups[2].Success
                    || idx + m.Length > Text.End)
                    throw new ParserException<TPar>((TPar)this, idx,
                        "could not get list item with pattern \""
                        + item.Pattern + '"');
                list.Add(new Text(Text.String, m.Index, m.Length));
                idx = m.Index + m.Length;
            }
            SetPosAndCountLines(idx);
            return list;
        }

        public Text GetToStartOf(char searchChar)
        {
            return GetToStartOf(searchChar,
                        p => p.ThrowCouldNotFind(
                                "'" + searchChar + "'"));
        }

        public Text GetToStartOf(char searchChar, Func<TPar, Text> notFoundAction)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchChar, index, Text.End - index);
            if (index < 0) return notFoundAction((TPar)this);
            var text = new Text(Pos, index, Text.String);
            SetPosAndCountLines(index); LastEnd = index + 1;
            return text;
        }

        public Text GetToStartOf(string searchString)
        {
            return GetToStartOf(searchString,
                        p => p.ThrowCouldNotFind(
                                '"' + searchString + '"'));
        }

        public Text GetToStartOf(string searchString, Func<TPar, Text> notFoundAction)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchString, index, Text.End - index);
            if (index < 0) return notFoundAction((TPar)this);
            var text = new Text(Pos, index, Text.String);
            SetPosAndCountLines(index); LastEnd = index + searchString.Length;
            return text;
        }

        public Text GetToStartOf(Rx searchRx)
        {
            return GetToStartOf(searchRx,
                    p => p.ThrowCouldNotFind("pattern \""
                            + searchRx.Pattern + '"'));
        }

        public Text GetToStartOf(Rx searchRx, Func<TPar, Text> notFoundAction)
        {
            var index = Math.Max(Pos, LastEnd);
            var m = searchRx.Regex.Match(Text.String, index, Text.End - index);
            if (!m.Success) return notFoundAction((TPar)this);
            index = m.Index;
            var text = new Text(Pos, index, Text.String);
            SetPosAndCountLines(index); LastEnd = index + m.Length;
            return text;
        }

        public Text GetToEndOf(char searchChar)
        {
            return GetToEndOf(searchChar,
                        p => p.ThrowCouldNotFind(
                                "'" + searchChar + "'"));
        }

        public Text GetToEndOf(char searchChar, Func<TPar, Text> notFoundFun)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchChar, index, Text.End - index);
            if (index < 0) return notFoundFun((TPar)this);
            index++;
            var text = new Text(Pos, index, Text.String);
            SetPosAndCountLines(index);
            return text;
        }

        public Text GetToEndOf(string searchString)
        {
            return GetToEndOf(searchString,
                        p => p.ThrowCouldNotFind(
                                '"' + searchString + '"'));
        }

        public Text GetToEndOf(string searchString, Func<TPar, Text> notFoundFun)
        {
            var index = Math.Max(Pos, LastEnd);
            index = Text.String.IndexOf(searchString, index, Text.End - index);
            if (index < 0) return notFoundFun((TPar)this);
            index += searchString.Length;
            var text = new Text(Pos, index, Text.String);
            SetPosAndCountLines(index);
            return text;
        }

        public Text GetToEndOf(Rx searchRx)
        {
            return GetToEndOf(searchRx,
                        p => p.ThrowCouldNotFind("pattern \""
                                + searchRx.Pattern + '"'));
        }

        public Text GetToEndOf(Rx searchRx, Func<TPar, Text> notFoundFun)
        {
            var index = Math.Max(Pos, LastEnd);
            var m = searchRx.Regex.Match(Text.String, index, Text.End - index);
            if (!m.Success) return notFoundFun((TPar)this);
            index = m.Index + m.Length;
            var text = new Text(Pos, index, Text.String);
            SetPosAndCountLines(index);
            return text;
        }

        public Text GetToStartOfOrEnd(char searchChar)
        {
            return GetToStartOf(searchChar, p => p.GetToPos(Text.End));
        }

        public Text GetToStartOfOrEnd(string searchString)
        {
            return GetToStartOf(searchString, p => p.GetToPos(Text.End));
        }

        public Text GetToStartOfOrEnd(Rx searchRx)
        {
            return GetToStartOf(searchRx, p => p.GetToPos(Text.End));
        }

        public Text GetToEndOfOrEnd(char searchChar)
        {
            return GetToEndOf(searchChar, p => p.GetToPos(Text.End));
        }

        public Text GetToEndOfOrEnd(string searchString)
        {
            return GetToEndOf(searchString, p => p.GetToPos(Text.End));
        }

        public Text GetToEndOfOrEnd(Rx searchRx)
        {
            return GetToEndOf(searchRx, p => p.GetToPos(Text.End));
        }

        public Text GetToWhiteSpace()
        {
            var start = Pos;
            while (Pos < Text.End && !Text.String[Pos].IsWhiteSpace())
                Pos++;
            return new Text(start, Pos, Text.String);
        }

        public bool TryGetByte(ref ParsedValue<byte> parsedValue)
        {
            parsedValue = Text.ParsedValueOfByteAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetSByte(ref ParsedValue<sbyte> parsedValue)
        {
            parsedValue = Text.ParsedValueOfSByteAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetShort(ref ParsedValue<short> parsedValue)
        {
            parsedValue = Text.ParsedValueOfShortAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetUShort(ref ParsedValue<ushort> parsedValue)
        {
            parsedValue = Text.ParsedValueOfUShortAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetInt(ref ParsedValue<int> parsedValue)
        {
            parsedValue = Text.ParsedValueOfIntAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetUInt(ref ParsedValue<uint> parsedValue)
        {
            parsedValue = Text.ParsedValueOfUIntAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetLong(ref ParsedValue<long> parsedValue)
        {
            parsedValue = Text.ParsedValueOfLongAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetULong(ref ParsedValue<ulong> parsedValue)
        {
            parsedValue = Text.ParsedValueOfULongAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetFloat(ref ParsedValue<float> parsedValue)
        {
            parsedValue = Text.ParsedValueOfFloatAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        public bool TryGetDouble(ref ParsedValue<double> parsedValue)
        {
            parsedValue = Text.ParsedValueOfDoubleAt(Pos - Text.Start);
            if (parsedValue.Error != ParseError.None) return false;
            Pos += parsedValue.Length;
            return true;
        }

        #endregion

        #region Peeking

        public char Peek
        {
            get { return Text.String[Pos]; }
        }

        public Text PeekFromStart
        {
            get { return PeekFromPos(Text.Start); }
        }

        public Text PeekToEnd
        {
            get { return PeekToPos(Text.End); }
        }

        /// <summary>
        /// Returns the text [pos + offset, pos) if offset is lower than 0,
        /// and the text [start + offset, pos) if offset is greater or equal
        /// to 0.
        public Text PeekFrom(int offset)
        {
            return PeekFromPos(offset < 0
                                ? Math.Max(Text.Start, Pos + offset)
                                : Math.Min(Text.Start + offset, Pos));
        }

        /// <summary>
        /// Returns the text [pos, pos + offset) if offset is greater than 0,
        /// and the text [pos, end + offset) if offset is lower or equal to 0.
        /// </summary>
        public Text PeekTo(int offset)
        {
            return PeekToPos(offset <= 0
                                ? Math.Max(Pos, Text.End + offset)
                                : Math.Min(Pos + offset, Text.End));
        }

        public Text PeekFromPos(int pos)
        {
            return new Text(pos, Pos, Text.String);
        }

        public Text PeekToPos(int pos)
        {
            return new Text(Pos, pos, Text.String);
        }

        #endregion

        #region Mostly Internal Methods

        public void SetPosAndCountLines(int pos)
        {
            int i = Pos;
            while (i < pos)
            {
                if (Text.String[i] == '\n')
                { ++i; Line.Index++; Line.Start = i; }
                else
                    ++i;
            }
            Pos = pos;
        }

        #endregion

        #region Throwing Exceptions

        public void ThrowEndOfText()
        {
            throw new ParserException<TPar>((TPar)this, "end of text");
        }

        public void ThrowOutOfRange(string typeName)
        {
            throw new ParserException<TPar>((TPar)this,
                "value out of " + typeName + " range");
        }

        public Text ThrowCouldNotFind(string str)
        {
            throw new ParserException<TPar>((TPar)this,
                "could not find " + str);
        }

        public Text ThrowCouldNotSkip(string str)
        {
            throw new ParserException<TPar>((TPar)this,
                "could not skip " + str);
        }

        #endregion
    }

    #endregion

    #region StackTextParser

    public class StackTextParser<TPar, TNode> : TextParser<TPar>
        where TPar : StackTextParser<TPar, TNode>
    {
        public List<TNode> NodeStack;

        public static TPar StackParse(
                Text text,
                TPar parser, State<TPar, TNode> rootState, TNode rootNode)
        {
            parser.NodeStack = rootNode.IntoList();
            var p = TextParser<TPar>.Parse(text, parser, rootState, rootNode);
            parser.NodeStack = null;
            return p;
        }

        /// <summary>
        /// Parse a part of the input into the supplied node.
        /// This function can be called to implement recursive descent.
        /// </summary>
        public static TPar StackParse(
                TPar parser, State<TPar, TNode> state, TNode node)
        {
            parser.NodeStack.Add(node);
            var p = TextParser<TPar>.Parse(parser, state, node);
            parser.NodeStack.RemoveAt(parser.NodeStack.Count - 1);
            return p;
        }
    }

    #endregion

    #region ParserException

    /// <summary>
    /// The generic recursive descent parser throws this type of exception.
    /// </summary>
    public class ParserException<TPar> : ApplicationException
        where TPar : TextParser<TPar>
    {
        public readonly TPar Parser;

        #region Constructors

        public ParserException(TPar parser, string message)
            : base(message)
        {
            Parser = parser;
        }

        public ParserException(TPar parser, int pos, string message)
            : base(message)
        {
            parser.SetPosAndCountLines(pos);
            Parser = parser;
        }

        public ParserException(TPar parser, string message, Exception inner)
            : base(message, inner)
        {
            Parser = parser;
        }

        #endregion
    }

    #endregion
}