using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapleManager
{
    static class ImageExtensions
    {

        public static void CopyMultiFormatBitmapToClipboard(this Image img)
        {
            using (MemoryStream msPNG = new MemoryStream())
            using (MemoryStream msBMP = new MemoryStream())
            using (MemoryStream msDIB = new MemoryStream())
            {
                IDataObject dataObject = new DataObject();

                img.Save(msPNG, ImageFormat.Png);
                dataObject.SetData("PNG", false, msPNG);

                img.Save(msBMP, ImageFormat.Bmp);
                msBMP.Position = 14;
                msBMP.CopyTo(msDIB);
                dataObject.SetData(DataFormats.Dib, true, msDIB);

                Clipboard.SetDataObject(dataObject, true, 20, 100);
            }
        }
    }
}
