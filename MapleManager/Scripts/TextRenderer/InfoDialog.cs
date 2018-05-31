using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MapleManager.Scripts.TextRenderer
{
    public partial class InfoDialog : Form
    {
        public ScriptNode MainScriptNode { get; set; }
        public InfoDialog()
        {
            InitializeComponent();
        }

        public struct InfoElement
        {
            public string Name;
            public string Explanation;
            public string Example;
            public Dictionary<long, string> ExampleQuestRecord;
            public override string ToString()
            {
                return Name;
            }
        }

        void AddElement(string name, string explanation, string example = "", Dictionary<long, string> exampleQuestRecord = null)
        {
            cbOptions.Items.Add(new InfoElement
            {
                Explanation = explanation.Trim(),
                Name = name,
                Example = example.Trim(),
                ExampleQuestRecord = exampleQuestRecord
            });
        }

        private void InfoDialog_Load(object sender, EventArgs e)
        {
            AddElement("#k #r #g #b #d (and maybe #y) - Font colors", @"
#k sets the font color back to the default color. For 'white based' chats, its white. Otherwise, its black.
#r sets the font color to red (alias: #Cred)
#g sets the font color to green (alias: #Cgreen)
#b sets the font color to blue (alias: #Cblue)
#d sets the font color to violet (alias: #Cviolet)
#y sets the font color to gray (alias: #Cgray)

#y might not work, as its also used for Quest info...

Its not possible to set the color yellow through a single character, use #Cyellow

", @"
#rRed
#gGreen
#bBlue
#dViolet
#Cgray#Gray
#Cyellow#Yellow!
#kDefault color
");



            AddElement("#C - Color aliases",
                @"
Format: #C(red|green|blue|violet|gray|yellow)#
There are color aliases you can use:
- Cred
- Cgreen
- Cblue
- Cviolet
- Cgray
- Cyellow (not actually an alias, the only way you can make the text yellow)
");

            AddElement("#h - Player name",
                @"
Format: #h(|1|2|3)#


Will show the character name.
There's an additional option, if one would append either 1, 2 or 3 after '#h'.
This is used for Korean name formatting.
#h1# will 'Add Josa EUN'
#h2# will 'Add Josa I'
#h3# will 'Add Josa EUL'
");
            AddElement("#e - Render (hidden) quest item",
                @"
Format: #e(?<questid>[0-9]+)(?<num>[0-9])#
- num is only 1 digit, and is the index of the demanded/required item to complete the quest.

If the player doesn't know about the item (the item is not in his inventory), it will render 
as item 3800088.
Otherwise, it will just show the item like #t would, except the itemid is fetched
from CheckSecretItemID (returning either 3800088 or the Quest Item ID)
", "#e100080#");

            AddElement("#v #i #z #t - Render regular item",
                @"
Format: #(v|i|z|t)(?<itemid>[0-9]+)#


");

            AddElement("#F - Canvas reference", 
                @"
Format: #F (?<UOL>[^#]+)#

Include a canvas image inside the text.
This can be anywhere inside the datafiles, the path supplied is absolute.
", @"
#F UI/UIWindow.img/Stat/backgrnd#
");

            AddElement("#a #M #x - Quest mob info",
                @"
Format: #(a|M|x)(?<questid>[0-9]+)(?<num>[0-9])#
- num is only 1 digit, and is the index of the demanded/required mob to complete the quest.

#a will be the amount of mobs that is demanded for the quest

#M and #x will both load from the Quest Record information. It expects this QuestRecord to be in the following format
(?<mobid>[0-9]+)/(?<isAbs>.)/(?<bonusExp>[0-9]+)

#M will be the mob name from the mobid inside the quest record
#x will be the bonus EXP from the quest record. If isAbs != 0, it is printed as +bonusExp, otherwise bonusExp%
", @"
Mob kill count: #a21051# (Quest record 2105)
QuestRecord 1000 mob name: #M1000#
QuestRecord 1000 EXP rate: #x1000#
QuestRecord 2000 mob name: #M2000#
QuestRecord 2000 EXP rate: #x2000#
", new Dictionary<long, string>
                {
                    { 2105, "005" },
                    { 1000, "100100/0/5" },
                    { 2000, "100100/1/50" }
                });

            AddElement("#Q - Quest Mate Name",
                @"
Format: #Q(?<questid>[0-9]+)#
-- CURRENTLY NOT SUPPORTED --
Inserts the mate name of the Quest ID, whatever that means.
Not part of KMST client.
");


            AddElement("#y - Quest info",
                @"
Format: 
  #y(?<questid>[0-9]+)#
Additional format: 
  #y.@(?<mode>[0-3])(?<questid>[0-9]+)(?<showTimeToo>[:])?#

Around GMS v.95, this was used for just displaying the quest name, resolved from
Quest/QuestInfo.img/(id)/name

In newer versions, it is possible to retrieve the quest start and end dates, using '@' as third element.
Additionally, you can retrieve the quest start and end times using ':' at the end of the string.

This information is loaded from Quest/Check.img/questid/0/{start|end}

Eh, well, the format is somewhat confusing, so here it is per element:

#y - Start of format
0 - Ignore quest id
@ - Start date/time format stuff
x - Either 0 or 2 for the start date/time, 1 or 3 for end date/time
... - Quest ID
: - Optional, shows the time too
# - End of format

#y0@01000#  - Show start date of questid 1000
#y0@01000:# - Show start date and time of questid 1000
#y0@11000#  - Show end date of questid 1000
#y0@11000:# - Show end date and time of questid 1000

", @"
#y1000# == Borrowing Sera's Mirror
#y1000# quest start date: #y0@01000# #y0@01000:#
#y1000# quest end date: #y0@11000# #y0@11000:#
");

            cbOptions.SelectedIndex = 0;
        }

        private void cbOptions_SelectedValueChanged(object sender, EventArgs e)
        {
            var ie = (InfoElement)cbOptions.SelectedItem;
            txtExplanation.Text = ie.Explanation;
            rtbExample.Clear();

            if (!string.IsNullOrEmpty(ie.Example))
            {
                var tp = new TextParser(ie.Example, MainScriptNode, ie.ExampleQuestRecord);
                tp.RenderText(rtbExample);

                txtExplanation.Text +=
                    Environment.NewLine +
                    Environment.NewLine +
                    "Code used by example:" + Environment.NewLine +
                    ie.Example;

                if (ie.ExampleQuestRecord != null)
                {
                    txtExplanation.Text += Environment.NewLine +
                        Environment.NewLine +
                        "QuestRecord data used:";

                    foreach (var kvp in ie.ExampleQuestRecord)
                    {
                        txtExplanation.Text += Environment.NewLine +
                                               kvp.Key + ": " + kvp.Value;
                    }
                }
            }
        }
    }
}
