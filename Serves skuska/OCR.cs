using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Updates_namespace;

namespace Tabber
{
    class OCR
    {

        //public void DumpResult(List<tessnet2.Word> result)
        //{
        //    foreach (tessnet2.Word word in result)
        //        Console.WriteLine("{0} : {1}", word.Confidence, word.Text);
        //}

        //public List<tessnet2.Word> DoOCRNormal(Bitmap image, string lang)
        //{
        //    tessnet2.Tesseract ocr = new tessnet2.Tesseract();
        //    ocr.Init("Kniznice\\tessdata", lang, false);
        //    List<tessnet2.Word> result = ocr.DoOCR(image, Rectangle.Empty);
        //    DumpResult(result);
        //    return result;
        //}

        //ManualResetEvent m_event;

        //public void DoOCRMultiThred(Bitmap image, string lang)
        //{
        //    try
        //    {
        //        Error.Show("", Form1.get_user_id().ToString());
        //        tessnet2.Tesseract ocr = new tessnet2.Tesseract();
        //        ocr.Init("Kniznice\\tessdata", lang, false);
        //        // If the OcrDone delegate is not null then this'll be the multithreaded version
        //        ocr.OcrDone = new tessnet2.Tesseract.OcrDoneHandler(Finished);
        //        // For event to work, must use the multithreaded version
        //        ocr.ProgressEvent += new tessnet2.Tesseract.ProgressHandler(ocr_ProgressEvent);
        //        m_event = new ManualResetEvent(false);
        //        ocr.DoOCR(image, Rectangle.Empty);
        //        // Wait here it's finished
        //        m_event.WaitOne();

        //    }
        //    catch (System.Exception ex)
        //    {
        //        Error.Show(ex.ToString(), Form1.get_user_id().ToString());
        //    }
        //}


        //public void Finished(List<tessnet2.Word> result)
        //{
        //    DumpResult(result);
        //    m_event.Set();
        //}

        //void ocr_ProgressEvent(int percent)
        //{
        //    Console.WriteLine("{0}% progression", percent);
        //}


    }
}
