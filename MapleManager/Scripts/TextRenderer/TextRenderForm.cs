using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MapleManager.Scripts.TextRenderer
{
    public partial class TextRenderForm : Form
    {
        public TextRenderForm()
        {
            InitializeComponent();
        }

        private void TextRenderForm_Load(object sender, EventArgs e)
        {
            txtIn.Text = "#t4031161##b#c4031161##k/5 ";
            ParseText();
        }


        enum Phrase
        {
            None = 0,
            List = 1,
            Func0 = 2,
            Func1 = 3,
            Func2 = 4,
            Func3 = 5,
            Reward = 6,
            ItemIcon = 7,
            ItemIcon_Outline = 8,
            ItemIcon_Secret = 9,
            SkillIcon = 10,
            Canvas = 11,
            Canvas_Outline = 12,
            Canvas_ProgressBar = 13,
            PartyQuestKeyword = 14,
            TimeLimitQuest = 15,
            DailyPlayQuest = 16,
            QuestSummary = 17,
            QuestOrder = 18,
            Text = 19,
            Illustration = 20,
            IllustEmotion = 21,
            ForceNextNpc = 22,
            MirrorDungeon = 23,
            FontName = 24,
            FontSize = 25,
            FontColor = 26,
            IllustAvatarEmotion = 27,
        }


        private void ParseText()
        {

            var lines = AnalyzeText(txtIn.Text, false, true).ToList();
            lbParseResults.Items.Clear();
            txtOut.Clear();

            int curLine = 0;
            foreach (var line in lines)
            {
                lbParseResults.Items.Add(string.Format("{0} {1}, '{2}' {3} {4}", 
                    line.font,
                    line.fontSize, 
                    line.text,
                    line.color, line.bold));

                try
                {
                    txtOut.SelectionFont = new Font(line.font, line.fontSize,
                        line.bold ? FontStyle.Bold : FontStyle.Regular);
                }
                catch
                {
                    txtOut.Clear();
                    txtOut.Text = "Error while parsing font... " + line.font + ", size " + line.fontSize;
                    break;
                }
                switch (line.color)
                {
                    case 0: txtOut.SelectionColor = Color.Black; break;
                    case 1: txtOut.SelectionColor = Color.Red; break;
                    case 2: txtOut.SelectionColor = Color.Green; break;
                    case 3: txtOut.SelectionColor = Color.Blue; break;
                    case 4: txtOut.SelectionColor = Color.White; break;
                    case 5: txtOut.SelectionColor = Color.Violet; break;
                    case 6: txtOut.SelectionColor = Color.Gray; break;
                    case 7: txtOut.SelectionColor = Color.Yellow; break;
                }
                txtOut.SelectedText = line.text;
                if (curLine != line.line)
                {
                    txtOut.SelectedText += "\r\n";
                    curLine = line.line;
                }
            }

        }

        /// <summary>
        /// Returns the next possible character in the String, which
        /// is encoding friendly.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        private static string GetCharacter(ref string input, bool remove)
        {
            if (input.Length == 0) return "";
            var c = input.Substring(0, 1);
            if (remove)
            {
                input = input.Substring(1);
            }
            return c;
        }

        private static void GetPhrase_Sharp(ref string text, ref string phrase)
        {
            var curChar = GetCharacter(ref text, true);
            if (curChar.Length == 0) return;
            switch (curChar[0])
            {
                case '#':
                    curChar = null;
                    return;

                case 'B':
                case 'C':
                case 'F':
                case 'L':
                case 'M':
                case '_':
                case 'a':
                case 'c':
                case 'f':
                case 'h':
                case 'i':
                case 'm':
                case 'o':
                case 'p':
                case 'q':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'x':
                case 'y':
                case 'z':
                    phrase += curChar;
                    break;

                case 'D':
                case 'Q':
                case 'R':
                case 'W':
                case 'j':
                    {
                        phrase += curChar;
                        var p = text.IndexOf("#");
                        if (p >= 0)
                        {
                            // Include '#'
                            phrase += text.Substring(0, p + 1);
                            text = text.Substring(p);
                        }


                        return;
                    }

                case 'n':
                    {
                        phrase += curChar;
                        if (text.IndexOf("pc") != 0)
                        {
                            curChar = null;
                            return;
                        }
                        break;
                    }
                default:
                    phrase += curChar;
                    return;

            }

            // Add leftover stuff
            while (true)
            {
                var tmp = GetCharacter(ref text, true);
                if (tmp == "") break;
                var c = tmp[0];
                if (c == '\r' || c == '#' || c == '\\')
                    break;

                phrase += tmp;
            }
        }

        private static void GetPhrase_Gen(ref string text, ref string phrase)
        {
            // Loop until \r, # or \ is found

            while (true)
            {
                var tmp = GetCharacter(ref text, false);
                if (tmp == "") break;
                var c = tmp[0];
                if (c == '\r' || c == '#' || c == '\\')
                    break;

                phrase += tmp;
                text = text.Substring(1);

            }
        }

        private static void GetPhrase(ref string text, ref string phrase)
        {
            phrase = "";

            if (text.Length == 0)
            {
                return;
            }

            var x = GetCharacter(ref text, false);
            if (x.Length == 0) return;

            if (x[0] == '\r' || x[0] == '\\')
            {
                phrase += x;
                text = text.Substring(1);
                phrase += GetCharacter(ref text, true);
            }
            else if (x[0] == '#')
            {
                phrase += x;
                text = text.Substring(1);
                GetPhrase_Sharp(ref text, ref phrase);
            }
            else
            {
                GetPhrase_Gen(ref text, ref phrase);
            }
        }

        private static int GetParameterNo(string phrase)
        {
            if (phrase.Length < 3) return 0;
            int x;
            if (int.TryParse(phrase.Substring(2), out x)) return x;
            return 0;
        }

        private static Phrase GetPhraseType(string phrase)
        {
            if (phrase.Length < 2 || phrase[0] != '#') return Phrase.None;

            switch (phrase[1])
            {
                case 'E': return Phrase.Func0;
                case 'I': return Phrase.Func1;
                case 'S': return Phrase.Func2;
                case 'K': return Phrase.Func3;
                case 'w': return Phrase.Reward;
            }

            if (phrase.Length <= 2) return Phrase.None;

            var phraseWithoutHash = phrase.Substring(1);
            if (phraseWithoutHash.StartsWith("questorder"))
                return Phrase.QuestOrder;

            if (phrase.Contains("illu") && phrase.Length > 5)
                return Phrase.Illustration;

            if (phrase.Contains("face") && phrase.Length > 5)
                return Phrase.IllustEmotion;

            if (phrase.Contains("avatar") && phrase.Length > 7)
                return Phrase.IllustAvatarEmotion;

            if (phrase.Contains("npc") && phrase.Length > 4)
                return Phrase.ForceNextNpc;

            if (phrase.Contains("fn") && phrase.Length > 2)
                return Phrase.FontName;

            if (phrase.Contains("fs") && phrase.Length > 2)
                return Phrase.FontSize;

            if (phrase.Contains("fc") && phrase.Length > 10)
                return Phrase.FontColor;

            if (phrase.Contains("MD") && phrase.Length > 3)
                return Phrase.MirrorDungeon;


            switch (phrase[1])
            {
                case 'L': return Phrase.List;
                case 'i':
                case 'v': return Phrase.ItemIcon;
                case 'e': return Phrase.ItemIcon_Secret;
                case 's': return Phrase.SkillIcon;
                case 'f':
                case 'F': return Phrase.Canvas;
                case 'B': return Phrase.Canvas_ProgressBar;
                case 'j': return Phrase.PartyQuestKeyword;
                case 'Q': return Phrase.TimeLimitQuest;
                case 'D': return Phrase.DailyPlayQuest;
                case 'W': return Phrase.QuestSummary;

            }

            return Phrase.Text;
        }

        struct Line
        {
            public string font;
            public string text;
            public bool bold;
            public int color;
            public int select;
            public int line;
            public int fontSize;
        }

        private IEnumerable<Line> AnalyzeText(string input, bool isWhiteBased, bool parsePhraseType)
        {
            string phrase = "";


            GetPhrase(ref input, ref phrase);
            int color = isWhiteBased ? 4 : 0;
            bool bold = false;
            int select = -1;
            int phraseType = 0;
            int curLine = 0;
            int fontSize = 8;
            string font = "arial";

            while (phrase != "")
            {
                switch (phrase)
                {
                    case "\r":
                    case "\\":
                        curLine++;
                        continue;

                    case "#k": color = isWhiteBased ? 4 : 0; break;
                    case "#Cred":
                    case "#r": color = 1; break;
                    case "#Cgreen":
                    case "#g": color = 2; break;
                    case "#Cblue":
                    case "#b": color = 3; break;
                    case "#Cviolet":
                    case "#d": color = 5; break;
                    case "#Cgray":
                    case "#y": color = 6; break;
                    case "#Cyellow": color = 7; break;

                    case "#e": bold = true; break;
                    case "#n": bold = false; break;

                    case "#l": select = -1; break;

                    default:
                        {
                            if (parsePhraseType)
                            {
                                // Do more stuff
                                switch (GetPhraseType(phrase))
                                {
                                    case Phrase.FontSize:
                                        {
                                            var x = phrase.Substring(3);
                                            int.TryParse(x, out fontSize);
                                            fontSize = Math.Max(fontSize, 1);
                                            phrase = "";
                                            break;
                                        }
                                    case Phrase.FontName:
                                        {
                                            var x = phrase.Substring(3);
                                            font = x;
                                            phrase = "";
                                            break;
                                        }

                                    case Phrase.ItemIcon:
                                        phrase = "(item icon " + GetParameterNo(phrase) + ")";
                                        break;
                                    case Phrase.ItemIcon_Secret:
                                        phrase = "(secret item icon " + GetParameterNo(phrase) + ")";
                                        break;
                                    case Phrase.SkillIcon:
                                        phrase = "(skill icon " + GetParameterNo(phrase) + ")";
                                        break;
                                    case Phrase.Canvas:
                                        phrase = "(wz reference " + phrase + ")";
                                        break;
                                    case Phrase.Canvas_ProgressBar:
                                        phrase = "(progress bar " + GetParameterNo(phrase) + ")";
                                        break;





                                    case Phrase.Text:
                                        {
                                            var no = GetParameterNo(phrase);
                                            switch (phrase[1])
                                            {
                                                case 'e': phrase = "(secret itemname " + no + ")"; break;
                                                case 't': phrase = "(itemname " + no + ")"; break;
                                                case 'z': phrase = "((old)itemname " + no + ")"; break;
                                                case 'o': phrase = "(mobname " + no + ")"; break;
                                                case 'h': phrase = "(charname)"; break;
                                                case 'p': phrase = "(npcname " + no + ")"; break;
                                                case 'm': phrase = "(mapname " + no + ")"; break;
                                                case 'q': phrase = "(skillname " + no + ")"; break;
                                                case 'c': phrase = "(itemcount " + no + ")"; break;
                                                case 'a': phrase = "(QuestMobCount " + no + ")"; break;
                                                case 'M': phrase = "(QuestMobName " + no + ")"; break;
                                                case 'x': phrase = "(QuestBonusEXP " + no + ")"; break;
                                                // todo: y
                                                case 'u': phrase = "(quest status " + no + ")"; break;
                                                case 'R': phrase = "(quest data " + no + ")"; break;
                                            }
                                            break;
                                        }
                                }

                            }

                            var line = new Line();
                            line.color = color;
                            line.bold = bold;
                            line.select = select;
                            line.text = phrase;
                            line.font = font;
                            line.line = curLine; // line
                            line.fontSize = fontSize;

                            yield return line;

                            break;
                        }
                }


                // Next phrase
                GetPhrase(ref input, ref phrase);
            }
        }

        private void txtIn_TextChanged(object sender, EventArgs e)
        {
            ParseText();
        }
    }
}
