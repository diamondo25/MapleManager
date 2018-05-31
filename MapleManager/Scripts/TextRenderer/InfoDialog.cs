using System;
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
            public override string ToString()
            {
                return Name;
            }
        }

        void AddElement(string name, string explanation, string example = "")
        {
            cbOptions.Items.Add(new InfoElement
            {
                Explanation = explanation.Trim(),
                Name = name,
                Example = example.Trim(),
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
There are color aliases you can use:
- Cred
- Cgreen
- Cblue
- Cviolet
- Cgray
- Cyellow (not actually an alias, the only way you can make the text yellow)
");
            AddElement("#F - Canvas reference", 
                @"
Include a canvas image inside the text.
This can be anywhere inside the datafiles, the path supplied is absolute.
", @"
#FUI/UIWindow.img/Stat/backgrnd#
");
            AddElement("#y - Quest info",
                @"
Format: #y1000# , where 1000 == quest id
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
                var tp = new TextParser(ie.Example, MainScriptNode);
                tp.RenderText(rtbExample);

                txtExplanation.Text += 
                    Environment.NewLine +
                    Environment.NewLine +
                    "Code used by example:" + Environment.NewLine + 
                    ie.Example;
            }
        }
    }
}
