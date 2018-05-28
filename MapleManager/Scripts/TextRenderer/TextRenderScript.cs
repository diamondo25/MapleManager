using System;
using MapleManager.Scripts.TextRenderer;

namespace MapleManager.Scripts
{
    class TextRenderScript : IScript
    {
        private TextRenderForm form = new TextRenderForm();
        public string GetScriptName()
        {
            return "TextRenderScript";
        }

        public void Start(ScriptNode mainScriptNode)
        {
            Program.MainForm.Shown += putScreenToFront;
            if (form == null || form.IsDisposed) form = new TextRenderForm();
            form.Show();
        }

        public void Stop()
        {
            form.Close();
        }


        private void putScreenToFront(object sender, EventArgs e)
        {
            if (form != null) form.BringToFront();
        }

    }
}
