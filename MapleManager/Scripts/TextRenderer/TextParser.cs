using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapleManager.Scripts.TextRenderer
{
    class TextParser
    {
        public string Text { get; set; }
        private ScriptNode MainScriptNode;
        private Dictionary<long, string> QuestRecord;

        public TextParser(string text, ScriptNode mainScriptNode, Dictionary<long, string> questRecord)
        {
            this.Text = text;
            MainScriptNode = mainScriptNode;
            QuestRecord = questRecord;
            if (QuestRecord == null)
                QuestRecord = new Dictionary<long, string>();
        }


        private void TempCopyPasteImage(Line line, RichTextBox outputBox)
        {
            if (line.image == null) return;
            var tmp = Clipboard.GetDataObject();

            //Clipboard.SetImage(line.image);
            // Black BG
            // line.image.CopyMultiFormatBitmapToClipboard();

            using (var bm = new Bitmap(line.image.Width, line.image.Height))
            using (var g = Graphics.FromImage(bm))
            {
                g.Clear(outputBox.BackColor);
                g.DrawImageUnscaled(line.image, 0, 0);
                Clipboard.SetImage(bm);

                outputBox.ReadOnly = false;
                outputBox.Paste();
                outputBox.ReadOnly = true;
            }


            if (tmp != null)
            {
                Clipboard.SetDataObject(tmp);
            }
        }


        public List<string> RenderText(RichTextBox outputBox)
        {
            outputBox.Clear();
            List<string> ret = new List<string>();

            try
            {
                var lines = AnalyzeText(Text, false, true).ToList();

                int curLine = 0;
                foreach (var line in lines)
                {
                    var text = line.text.Replace("\t", "    ");

                    ret.Add(string.Format("{0} {1}, '{2}' {3} {4}",
                        line.font,
                        line.fontSize,
                        text,
                        line.color, line.bold
                    ));

                    try
                    {
                        outputBox.SelectionFont = new Font(line.font, line.fontSize,
                            line.bold ? FontStyle.Bold : FontStyle.Regular);
                    }
                    catch
                    {
                        outputBox.Clear();
                        outputBox.Text = "Error while parsing font... " + line.font + ", size " + line.fontSize;
                        break;
                    }
                    switch (line.color)
                    {
                        case 0:
                            outputBox.SelectionColor = Color.Black;
                            break;
                        case 1:
                            outputBox.SelectionColor = Color.Red;
                            break;
                        case 2:
                            outputBox.SelectionColor = Color.Green;
                            break;
                        case 3:
                            outputBox.SelectionColor = Color.Blue;
                            break;
                        case 4:
                            outputBox.SelectionColor = Color.White;
                            break;
                        case 5:
                            outputBox.SelectionColor = Color.Violet;
                            break;
                        case 6:
                            outputBox.SelectionColor = Color.Gray;
                            break;
                        case 7:
                            outputBox.SelectionColor = Color.Yellow;
                            break;
                    }
                    outputBox.SelectedText = text;

                    TempCopyPasteImage(line, outputBox);

                    if (curLine != line.line)
                    {
                        outputBox.SelectedText += "\r\n";
                        curLine = line.line;
                    }
                }

            }
            catch (Exception ex)
            {
                outputBox.Clear();
                outputBox.Text = ex.ToString();
            }

            return ret;
        }

        class SelectedMob
        {
            public int MobId;
            public bool Abs;
            public int BonusEXP;
        }

        private static SelectedMob DecodeSelectedMob(string questRecordValue)
        {
            var elems = questRecordValue.Split('/');
            if (elems.Length >= 3)
            {
                return new SelectedMob
                {
                    MobId = int.Parse(elems[0]),
                    Abs = int.Parse(elems[1]) != 0,
                    BonusEXP = int.Parse(elems[2])
                };
            }
            return null;
        }

        enum StringType
        {
            Item,
            Npc,
            Mob,
            Skill,
            Map
        }

        private string GetString(StringType type, int id, string field = "name")
        {
            int inv = id / 1000000;
            if (type == StringType.Item && inv == 1)
            {
                // Oh boy, this one is a bit harder.
                // Eqp.img/Eqp/categoryname/id ...
                // Lets just iterate over each category and try to find it

                foreach (var eqpCategory in MainScriptNode.GetNode("String/Eqp.img/Eqp"))
                {
                    var eqpNode = eqpCategory.GetNode(id.ToString());
                    if (eqpNode != null)
                    {
                        return eqpNode.GetString(field);
                    }
                }

                return "??? Unknown equip " + id + " ???";
            }

            if (type == StringType.Map)
            {
                // Maps have about the same as equips. There's a category....
                foreach (var mapCategory in MainScriptNode.GetNode("String/Map.img"))
                {
                    var mapNode = mapCategory.GetNode(id.ToString());
                    if (mapNode != null)
                    {
                        return mapNode.GetString(field);
                    }
                }
                return "??? Unknown map " + id + " ???";
            }


            string mainNode = ".";
            string idStr = null;
            switch (type)
            {
                case StringType.Mob: mainNode = "Mob.img"; break;
                case StringType.Skill:
                    mainNode = "Skill.img";
                    idStr = id.ToString("D7");
                    break;
                case StringType.Npc: mainNode = "Npc.img"; break;
                case StringType.Item:
                    {
                        switch (inv)
                        {
                            case 2: mainNode = "Consume.img"; break;
                            case 3: mainNode = "Ins.img"; break;
                            case 4: mainNode = "Etc.img/Etc"; break;
                            case 5: mainNode = "Cash.img"; break;
                        }
                        break;
                    }
            }

            return GetString(string.Format("String/{0}/{1}/{2}", mainNode, idStr ?? id.ToString(), field));
        }

        private string GetString(string path)
        {
            var node = MainScriptNode.GetNode(path);
            if (node == null) return "??? Unknown node " + path + " ???";
            var str = node.GetString();
            return str ?? "??? Not a string " + path + " ???";
        }

        private Image GetEquipmentImage(int id, string iconName = "iconRaw")
        {
            var str = id.ToString("D8") + ".img";
            foreach (var node in MainScriptNode["Character"])
            {
                if (node.Name.EndsWith(".img")) continue;
                Console.WriteLine("{0}", node.Name);
                var x = node[str];
                if (x == null) continue;
                var subNodePath = "info/" + iconName;
                var img = x.GetImage(subNodePath);

                if (img == null)
                {
                    Console.WriteLine("Found the correct node {0}, but not {1}", x.GetFullPath(), subNodePath);
                    continue;
                }

                return img;
            }

            return null;
        }

        private Image GetItemImage(int id, string iconName = "iconRaw")
        {
            int inv = id / 1000000;
            if (inv == 1)
            {
                // Eh... I'm not gonna fetch this right now.
                return GetEquipmentImage(id, iconName);
            }

            int category = id / 10000;

            string name;
            switch (inv)
            {
                case 2: name = "Consume"; break;
                case 3: name = "Install"; break;
                case 4: name = "Etc"; break;
                default:
                    if (category == 500) name = "Pet";
                    else name = "Cash";
                    break;
            }

            var path = string.Format("Item/{0}/{1:D4}.img/{2:D8}/info/{3}", name, category, id, iconName);
            return GetImage(path);
        }

        private Image GetSkillImage(int id, string iconName = "icon")
        {
            int job = id / 10000;
            var path = string.Format("Skill/{0:D3}.img/skill/{1}/{2}", job, id, iconName);
            return GetImage(path);
        }

        private Image GetImage(string path)
        {
            var node = MainScriptNode.GetNode(path);
            if (node == null)
            {
                Console.WriteLine("Unknown node {0}", path);
                return null;
            }

            var actualImage = node.GetImage();
            if (actualImage != null) return actualImage;
            Console.WriteLine("Image not found?");
            return null;
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

        /// <summary>
        /// This function always starts from the 3rd character.
        /// #f12345# will result into 12345
        /// Returns 0 if not found
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        private static int GetParameterNo(string phrase)
        {
            if (phrase.Length < 3) return 0;
            int x;

            const int skip = 2;
            for (int i = skip; i < phrase.Length; i++)
            {
                if (!char.IsDigit(phrase[i]))
                {
                    if (int.TryParse(phrase.Substring(skip, i - skip), out x)) return x;
                    return 0;
                }
            }


            if (int.TryParse(phrase.Substring(skip), out x)) return x;
            return 0;
        }

        private static Phrase GetPhraseType(string phrase)
        {
            if (phrase.Length < 2 || phrase[0] != '#')
            {
#if TRACE_PHRASE
                Console.WriteLine("Phrase None: {0}", phrase);
#endif
                return Phrase.None;
            }

            switch (phrase[1])
            {
                case 'E': return Phrase.Func0;
                case 'I': return Phrase.Func1;
                case 'S': return Phrase.Func2;
                case 'K': return Phrase.Func3;
                case 'w': return Phrase.Reward;
            }

            if (phrase.Length <= 2)
            {
#if TRACE_PHRASE
                Console.WriteLine("Phrase None (2): {0}", phrase);
#endif
                return Phrase.None;
            }

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

            var phraseType = Phrase.Text;
            switch (phrase[1])
            {
                case 'L': phraseType = Phrase.List; break;
                case 'i':
                case 'v': phraseType = Phrase.ItemIcon; break;
                case 'e': phraseType = Phrase.ItemIcon_Secret; break;
                case 's': phraseType = Phrase.SkillIcon; break;
                case 'f':
                case 'F': phraseType = Phrase.Canvas; break;
                case 'B': phraseType = Phrase.Canvas_ProgressBar; break;
                case 'j': phraseType = Phrase.PartyQuestKeyword; break;
                case 'Q': phraseType = Phrase.TimeLimitQuest; break;
                case 'D': phraseType = Phrase.DailyPlayQuest; break;
                case 'W': phraseType = Phrase.QuestSummary; break;
            }

#if TRACE_PHRASE
            Console.WriteLine("Phrase {1}: {0}", phrase, phraseType);
#endif
            return phraseType;
        }

        public struct Line
        {
            public string font;
            public string text;
            public bool bold;
            public int color;
            public int select;
            public int line;
            public int fontSize;
            public Image image;
        }

        public IEnumerable<Line> AnalyzeText(string input, bool isWhiteBased, bool parsePhraseType)
        {
            string phrase = "";

            int color = isWhiteBased ? 4 : 0;
            bool bold = false;
            int select = -1;
            int curLine = 0;
            int fontSize = 8;
            string font = "arial";
            string extraInfo = null;
            Image img = null;

            while (true)
            {
                GetPhrase(ref input, ref phrase);
#if TRACE_PHRASE
                Console.WriteLine("Phrase: {0} from {1}", phrase, input);
#endif
                if (phrase == "") break;

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
                    // #y is conflicting with Quest status, this one is ignored.
                    case "#y": color = 6; break;
                    case "#Cyellow": color = 7; break;

                    case "#e": bold = true; break;
                    case "#n": bold = false; break;

                    case "#l": select = -1; break;

                    default:
                        {
                            if (parsePhraseType)
                            {
                                var phraseType = GetPhraseType(phrase);
#if TRACE_PHRASE
                                Console.WriteLine("Parsing phraseType {0} from {1}", phraseType, phrase);
#endif
                                // Do more stuff
                                switch (phraseType)
                                {
                                    case Phrase.FontSize:
                                        {
                                            var x = phrase.Substring(3);
                                            extraInfo = "Font size " + x;
                                            int.TryParse(x, out fontSize);
                                            fontSize = Math.Max(fontSize, 1);
                                            phrase = "";
                                            break;
                                        }
                                    case Phrase.FontName:
                                        {
                                            var x = phrase.Substring(3);
                                            extraInfo = "Font name " + x;
                                            font = x;
                                            phrase = "";
                                            break;
                                        }

                                    case Phrase.ItemIcon:
                                        extraInfo = "Item icon";
                                        img = GetItemImage(GetParameterNo(phrase));
                                        phrase = "";
                                        break;


                                    case Phrase.ItemIcon_Secret:
                                        extraInfo = "Secret item " + GetParameterNo(phrase);
                                        // Conflicting with the bold toggle, this one is ignored
                                        phrase = "";
                                        img = GetItemImage(3800088);
                                        break;

                                    case Phrase.SkillIcon:
                                        extraInfo = "Skill icon " + GetParameterNo(phrase);
                                        img = GetSkillImage(GetParameterNo(phrase));
                                        phrase = "";
                                        break;

                                    case Phrase.Canvas:
                                    case Phrase.Canvas_Outline:
                                        // #FUI/MapleTV.img/TVon/0#
                                        phrase = phrase.Substring(2);
                                        extraInfo = "Canvas " + phrase;
                                        img = GetImage(phrase);
                                        if (img == null)
                                        {
                                            phrase = "!!! image not found! " + phrase + " !!!";
                                        }
                                        else
                                        {
                                            phrase = "";
                                        }
                                        break;

                                    case Phrase.Canvas_ProgressBar:
                                        {
                                            extraInfo = "(progress bar " + GetParameterNo(phrase) + ")";
                                            // this one requires explicit rendering of the image

                                            var basePath = "UI/Login.img/Notice/Loading/bar/";
                                            var baseImage = GetImage(basePath + "10");

                                            img = new Bitmap(baseImage);
                                            var g = Graphics.FromImage(img);

                                            var percentage = GetParameterNo(phrase);
                                            percentage = Math.Max(0, Math.Min(percentage, 100));

                                            g.DrawImageUnscaled(GetImage(basePath + (percentage / 10)), 0, 0);
                                            phrase = "";

                                            break;
                                        }

                                    case Phrase.QuestSummary:
                                        {
                                            var str = phrase.Substring(2);
                                            if (str.Length != 0)
                                            {
                                                img = GetImage("UI/UIWindow2.img/Quest/quest_info/summary_icon/" + str);
                                            }

                                            phrase = "";
                                            break;
                                        }


                                    case Phrase.PartyQuestKeyword:
                                        {
                                            // This thing is huge.
                                            phrase = "(party quest keyword...)";
                                            break;
                                        }

                                    case Phrase.Text:
                                        {
                                            var no = GetParameterNo(phrase);
                                            switch (phrase[1])
                                            {

                                                case 'e':
                                                    // Conflicts with bold toggle, not handled.
                                                    extraInfo = "Secret item name " + no;
                                                    phrase = "(secret itemname " + no + ")";
                                                    break;

                                                case 't': phrase = GetString(StringType.Item, no); break;
                                                case 'z': phrase = GetString(StringType.Item, no); break;
                                                case 'o': phrase = GetString(StringType.Mob, no); break;
                                                case 'h': phrase = "(charname)"; break;
                                                case 'p': phrase = GetString(StringType.Npc, no); break;
                                                case 'm': phrase = GetString(StringType.Map, no, "mapName"); break;
                                                case 'q': phrase = GetString(StringType.Skill, no); break;
                                                case 'c': phrase = "(Items in inventory of ID " + no + ")"; break;
                                                case 'a':
                                                    {

                                                        int qrId = no / 10;
                                                        int idx = no % 10;
                                                        int start = idx * 3 - 3;

                                                        extraInfo = "Mob kill count idx " + idx;

                                                        int count = 0;
                                                        if (QuestRecord.ContainsKey(qrId))
                                                        {
                                                            var val = QuestRecord[qrId];
                                                            if (val.Length >= idx * 3)
                                                            {
                                                                int.TryParse(val.Substring(start, 3), out count);
                                                            }
                                                        }

                                                        phrase = count.ToString();
                                                        break;
                                                    }
                                                case 'M':
                                                    {
                                                        extraInfo = "(QuestMobName from mobid inside QuestRecord " + no + " eg 100100/0/0)";

                                                        int qrId = no;
                                                        phrase = "";

                                                        if (QuestRecord.ContainsKey(qrId))
                                                        {
                                                            var val = QuestRecord[qrId];

                                                            var x = DecodeSelectedMob(val);
                                                            if (x != null)
                                                            {
                                                                phrase = GetString(StringType.Mob, x.MobId);
                                                            }
                                                        }

                                                        break;
                                                    }
                                                case 'x':
                                                    {
                                                        extraInfo = "(QuestBonusEXP from QuestRecord " + no + " eg 0/a/1337, where a = abs)";

                                                        int qrId = no;
                                                        phrase = "";

                                                        if (QuestRecord.ContainsKey(qrId))
                                                        {
                                                            var val = QuestRecord[qrId];

                                                            var x = DecodeSelectedMob(val);
                                                            if (x != null)
                                                            {
                                                                phrase = string.Format(x.Abs ? "+{0}" : "{0}%", x.BonusEXP);
                                                            }
                                                        }

                                                        break;
                                                    }
                                                case 'y':
                                                    {
                                                        if (phrase.Length > 3 && phrase[3] == '@')
                                                        {
                                                            // Quest date/time parsing
                                                            phrase = phrase.Substring(2);
                                                            var printTime = phrase.Contains(':');

                                                            var dateTimeFormat = "yyyy-MM-dd" + (printTime ? " HH:mm" : "");

                                                            var questId = GetParameterNo(phrase);
                                                            string dateNumbers = null;
                                                            switch (phrase[2])
                                                            {
                                                                case '0':
                                                                case '2':
                                                                    dateNumbers = GetString(string.Format(
                                                                        "Quest/Check.img/{0}/0/start",
                                                                        questId
                                                                    ));

                                                                    break;

                                                                case '1':
                                                                case '3':
                                                                    dateNumbers = GetString(string.Format(
                                                                        "Quest/Check.img/{0}/0/end",
                                                                        questId
                                                                    ));
                                                                    break;

                                                                default:
                                                                    phrase = "(Unknown format?)";
                                                                    break;
                                                            }

                                                            DateTime date;
                                                            if (dateNumbers != null && DateTime.TryParseExact(dateNumbers,
                                                                    "yyyyMMddHH", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
                                                            {
                                                                phrase = date.ToString(dateTimeFormat);
                                                            }
                                                            else
                                                            {
                                                                phrase = "??? Date invalid ???";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            phrase = GetString(string.Format(
                                                                "Quest/QuestInfo.img/{0}/name",
                                                                GetParameterNo(phrase))
                                                            );
                                                        }

                                                        break;
                                                    }
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
                            line.image = img;
                            img = null;

                            yield return line;

                            break;
                        }
                }
            }
        }
    }
}
