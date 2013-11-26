using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using TimeManage.Code.RazorPDF;

namespace RazorPDF
{
    public class PdfView : IView, IViewEngine
    {
        private readonly ViewEngineResult _result;
        private MemoryStream _pdfstream;

        public PdfView(ViewEngineResult result)
        {
            _result = result;
        }

        public PdfView(ViewEngineResult result, MemoryStream pdfstream)
        {
            _result = result;
            _pdfstream = pdfstream;
        }


        void CreatePdf(Stream htmlInput, Stream pdfOutput)
        {
            using (var document = new Document(PageSize.A4, 30, 30, 30, 30))
            {
                var writer = PdfWriter.GetInstance(document, pdfOutput);
                var worker = XMLWorkerHelper.GetInstance();

                document.Open();
                worker.ParseXHtml(writer, document, htmlInput, null, Encoding.UTF8, new UnicodeFontFactory());

                document.Close();
            }
        }



        public void Render(ViewContext viewContext, TextWriter writer)
        {

                var sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                _result.View.Render(viewContext, tw);
                var resultCache = sb.ToString();


                using (Document document = new Document(PageSize.A4, 30, 30, 30, 30))
                {
                    TextReader xmlString = new StringReader(resultCache);

                    var wrt = PdfWriter.GetInstance(document, _pdfstream ?? viewContext.HttpContext.Response.OutputStream);
                    document.Open();

                    HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
                    htmlContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());
                    //    htmlContext.SetImageProvider(new ImageProvider());

                    ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(false);
                    cssResolver.AddCssFile(@"D:\demo\css\style.css", true);

                    IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(document, wrt)));
                    XMLWorker worker = new XMLWorker(pipeline, true);
                    XMLParser xmlParse = new XMLParser(true, worker);
                    xmlParse.Parse(xmlString);
                    xmlParse.Flush();

                    document.Close();
                    document.Dispose();
                }

        }

        private static XmlTextReader GetXmlReader(string source)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(source);
            MemoryStream stream = new MemoryStream(byteArray);

            var xtr = new XmlTextReader(stream);
            xtr.WhitespaceHandling = WhitespaceHandling.None; // Helps iTextSharp parse 
            return xtr;
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            throw new System.NotImplementedException();
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            _result.ViewEngine.ReleaseView(controllerContext, _result.View);
        }
    }
}
