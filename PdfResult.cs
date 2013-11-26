using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RazorPDF
{
    public class PdfResult : ViewResult
    {
        private MemoryStream _pdfstream;

        public PdfResult(object model, string name, MemoryStream pdfstream)
        {
            ViewData = new ViewDataDictionary(model);
            ViewName = name;
            _pdfstream = pdfstream;

        }

        public PdfResult(object model, string name)
        {
            ViewData = new ViewDataDictionary(model);
            ViewName = name;
        }
        public PdfResult()
            : this(new ViewDataDictionary(), "Pdf")
        {
        }
        public PdfResult(object model)
            : this(model, "Pdf")
        {
        }

        protected override ViewEngineResult FindView(ControllerContext context)
        {
            var result = base.FindView(context);
            if (result.View == null)
                return result;
            if (_pdfstream != null)
            {

                var pdfView = new PdfView(result, _pdfstream);
                return new ViewEngineResult(pdfView, pdfView);
            }
            else
            {
                var pdfView = new PdfView(result);
                return new ViewEngineResult(pdfView, pdfView);
            }
            
        }


     public override void ExecuteResult(ControllerContext context)
        {
         base.ExecuteResult(context);
        }
    }
}
