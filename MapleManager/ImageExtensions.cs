using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MapleManager
{
    static class ImageExtensions
    {

        public static void CopyMultiFormatBitmapToClipboard(this Image img)
        {
            using (var msPNG = new MemoryStream())
            using (var msBMP = new MemoryStream())
            using (var msDIB = new MemoryStream())
            {
                IDataObject dataObject = new DataObject();

                img.Save(msPNG, ImageFormat.Png);
                dataObject.SetData("PNG", false, msPNG);

                dataObject.SetData(DataFormats.Bitmap, true, msBMP);

                img.Save(msBMP, ImageFormat.Bmp);
                msBMP.Position = 14;
                msBMP.CopyTo(msDIB);
                dataObject.SetData(DataFormats.Dib, true, msDIB);


                Clipboard.SetDataObject(dataObject, true, 20, 100);
            }
        }
    }
}
