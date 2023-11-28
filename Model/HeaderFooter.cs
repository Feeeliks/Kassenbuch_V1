using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_LoginForm.ViewModels.Pages;

namespace WPF_LoginForm.Model
{
    public class HeaderFooter : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            var content = writer.DirectContent;
            var pageBoundary = document.PageSize;

            var footerFont = FontFactory.GetFont("Arial", 8);
            var headerFont = FontFactory.GetFont("Arial", 8);

            // Kopfzeile hinzufügen
            ColumnText.ShowTextAligned(
                content,
                Element.ALIGN_CENTER,
                new Phrase("Kassenbuch " + HomeViewModel.Instance.AktuellesProjekt + " (" + ExportViewModel.Instance.AktuelleMonthGroup + ")"),
                pageBoundary.GetLeft(document.LeftMargin) + (pageBoundary.Width - document.LeftMargin - document.RightMargin) / 2,
                pageBoundary.GetTop(document.TopMargin) + 15,
                0);

            // Fußzeile hinzufügen
            ColumnText.ShowTextAligned(
                content,
                Element.ALIGN_CENTER,
                new Phrase("Steuer I - Ideeller Bereich (Übungs - und Trainingsbetrieb)      Steuer II - Vermögensverwaltung      Steuer III - Zweckbetriebe (sportliche Veranstaltungen)      Steuer IV - Steuerpflichtiger, wirtschaftlicher Geschäftsbetrieb", footerFont),
                pageBoundary.GetLeft(document.LeftMargin) + (pageBoundary.Width - document.LeftMargin - document.RightMargin) / 2,
                pageBoundary.GetBottom(document.BottomMargin) - 10,
                0);

            // Fußzeile hinzufügen
            ColumnText.ShowTextAligned(
                content,
                Element.ALIGN_CENTER,
                new Phrase($"Seite {writer.PageNumber}", footerFont),
                pageBoundary.GetLeft(document.LeftMargin) + (pageBoundary.Width - document.LeftMargin - document.RightMargin) / 2,
                pageBoundary.GetBottom(document.BottomMargin) - 25,
                0);
        }
    }
}
