using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Data;
using System.Windows.Input;
using WPF_LoginForm.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using static iTextSharp.text.pdf.hyphenation.TernaryTree;
using System.Transactions;
using System.Data.Common;
using Xceed.Wpf.Toolkit.Primitives;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class ExportViewModel : ViewModelBase
    {
        //Singleton
        private static ExportViewModel instance;

        public static ExportViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExportViewModel();
                }
                return instance;
            }
        }

        //Constructor
        public ExportViewModel()
        {
            ExportKassenbuchCommand = new ViewModelCommand(ExecuteExportKassenbuchCommand, CanExecuteExportKassenbuchCommand);
            ExportKassenberichtCommand = new ViewModelCommand(ExecuteExportKassenberichtCommand, CanExecuteExportKassenberichtCommand);
            ExportKassenprüfberichtCommand = new ViewModelCommand(ExecuteExportKassenprüfberichtCommand, CanExecuteExportKassenprüfberichtCommand);
        }

        //Commands
        public ICommand ExportKassenbuchCommand { get; }
        public ICommand ExportKassenberichtCommand { get; }
        public ICommand ExportKassenprüfberichtCommand { get; }

        //Fields
        private string _aktuelleMonthGroup;

        //Properties
        public string AktuelleMonthGroup
        {
            get { return _aktuelleMonthGroup; }
            set
            {
                _aktuelleMonthGroup = value;
                OnPropertyChanged(nameof(AktuelleMonthGroup));
            }
        }

        //Methods

        //Kassenbericht
        private bool CanExecuteExportKassenberichtCommand(object obj)
        {
            return true;
        }

        private void ExecuteExportKassenberichtCommand(object obj)
        {
            string fileName = HomeViewModel.Instance.AktuellesProjekt + string.Format("_Kassenbericht_{0:yyyyMMddHHmmss}.pdf", DateTime.Now);
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            Single tableSpacing = 5f;
            Single tableSpacing2 = 15f;

            // create a new document
            using (var document = new Document(PageSize.A4, marginLeft: 35f, marginTop: 20f, marginRight: 35f, marginBottom: 40f))
            {
                // create a new PdfWriter and bind it to the document
                using (var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create)))
                {
                    // open the document for writing
                    document.Open();

                    var headerFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 8);
                    var cellFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 8);

                    document.Add(new Paragraph("Kassenbericht " + HomeViewModel.Instance.AktuellesProjekt) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph("\n"));

                    //Tabelle Bestand Vorjahr

                    // create a table with 3 columns
                    var tableBestandVorjahr = new PdfPTable(3);

                    tableBestandVorjahr.WidthPercentage = 100;
                    tableBestandVorjahr.LockedWidth = false;

                    tableBestandVorjahr.SetWidths(new float[] { 1.5f, 8f, 1f }); // set column widths

                    tableBestandVorjahr.AddCell(new PdfPCell(new Phrase("Vereinsbestand", headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandVorjahr.AddCell(new PdfPCell(new Phrase("31.12." + HomeViewModel.Instance.VorherigesProjekt, headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandVorjahr.AddCell(new PdfPCell(new Phrase(KassenbuchViewModel.Instance.Kassenstand[4].ToString("C2"), cellFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    //Tabelle Einnahmen gesamt
                    var tableEinnahmenGesamt = new PdfPTable(3);

                    tableEinnahmenGesamt.SpacingBefore = tableSpacing2;

                    tableEinnahmenGesamt.WidthPercentage = 100;
                    tableEinnahmenGesamt.LockedWidth = false;

                    double betragEinnahmenGesamt = 0.00;

                    for (int i = 0; i < KassenberichtViewModel.Instance.GruppeEinnahmen.Count; i++)
                    {
                        betragEinnahmenGesamt += KassenberichtViewModel.Instance.GruppeEinnahmen[i].Betrag;
                    }

                    tableEinnahmenGesamt.SetWidths(new float[] { 1.5f, 8f, 1f }); // set column widths

                    tableEinnahmenGesamt.AddCell(new PdfPCell(new Phrase("Einnahmen", headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableEinnahmenGesamt.AddCell(new PdfPCell(new Phrase("31.12." + HomeViewModel.Instance.AktuellesProjekt, headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableEinnahmenGesamt.AddCell(new PdfPCell(new Phrase(betragEinnahmenGesamt.ToString("C2"), cellFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    //Tabelle Einnahmen einzeln
                    var tableEinnahmenEinzeln = new PdfPTable(2);

                    tableEinnahmenEinzeln.SpacingBefore = tableSpacing;

                    tableEinnahmenEinzeln.WidthPercentage = 90;
                    tableEinnahmenEinzeln.LockedWidth = false;

                    tableEinnahmenEinzeln.SetWidths(new float[] { 9f, 1f }); // set column widths

                    foreach (var eintrag in KassenberichtViewModel.Instance.GruppeEinnahmen)
                    {
                        tableEinnahmenEinzeln.AddCell(new PdfPCell(new Phrase(eintrag.Gruppe, cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                        tableEinnahmenEinzeln.AddCell(new PdfPCell(new Phrase(eintrag.Betrag.ToString("C2"), cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                    }

                    //Tabelle Ausgaben gesamt
                    var tableAusgabenGesamt = new PdfPTable(3);

                    tableAusgabenGesamt.SpacingBefore = tableSpacing2;

                    tableAusgabenGesamt.WidthPercentage = 100;
                    tableAusgabenGesamt.LockedWidth = false;

                    double betragAusgabenGesamt = 0.00;

                    for (int i = 0; i < KassenberichtViewModel.Instance.GruppeAusgaben.Count; i++)
                    {
                        betragAusgabenGesamt += KassenberichtViewModel.Instance.GruppeAusgaben[i].Betrag;
                    }

                    tableAusgabenGesamt.SetWidths(new float[] { 1.5f, 8f, 1f }); // set column widths

                    tableAusgabenGesamt.AddCell(new PdfPCell(new Phrase("Ausgaben", headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableAusgabenGesamt.AddCell(new PdfPCell(new Phrase("31.12." + HomeViewModel.Instance.AktuellesProjekt, headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableAusgabenGesamt.AddCell(new PdfPCell(new Phrase(betragAusgabenGesamt.ToString("C2"), cellFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    //Tabelle Ausgaben einzeln
                    var tableAusgabenEinzeln = new PdfPTable(2);

                    tableAusgabenEinzeln.SpacingBefore = tableSpacing;

                    tableAusgabenEinzeln.WidthPercentage = 90;
                    tableAusgabenEinzeln.LockedWidth = false;

                    tableAusgabenEinzeln.SetWidths(new float[] { 9f, 1f }); // set column widths

                    foreach (var eintrag in KassenberichtViewModel.Instance.GruppeAusgaben)
                    {
                        tableAusgabenEinzeln.AddCell(new PdfPCell(new Phrase(eintrag.Gruppe, cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                        tableAusgabenEinzeln.AddCell(new PdfPCell(new Phrase(eintrag.Betrag.ToString("C2"), cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                    }

                    //Tabelle Bestand gesamt
                    var tableBestandGesamt = new PdfPTable(3);

                    tableBestandGesamt.SpacingBefore = tableSpacing2;

                    tableBestandGesamt.WidthPercentage = 100;
                    tableBestandGesamt.LockedWidth = false;

                    tableBestandGesamt.SetWidths(new float[] { 1.5f, 8f, 1f }); // set column widths

                    tableBestandGesamt.AddCell(new PdfPCell(new Phrase("Vereinsbestand", headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandGesamt.AddCell(new PdfPCell(new Phrase("31.12." + HomeViewModel.Instance.AktuellesProjekt, headerFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandGesamt.AddCell(new PdfPCell(new Phrase(KassenbuchViewModel.Instance.Kassenstand[0].ToString("C2"), cellFont)) { BorderWidthBottom = 1, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    //Tabelle Bestand einzeln
                    var tableBestandEinzeln = new PdfPTable(2);

                    tableBestandEinzeln.SpacingBefore = tableSpacing;

                    tableBestandEinzeln.WidthPercentage = 90;
                    tableBestandEinzeln.LockedWidth = false;

                    tableBestandEinzeln.SetWidths(new float[] { 9f, 1f }); // set column widths

                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase("Ausschankkasse", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase(KassenbuchViewModel.Instance.Kassenstand[3].ToString("C2"), cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase("Handkasse", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase(KassenbuchViewModel.Instance.Kassenstand[2].ToString("C2"), cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase("Kassenstand", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase((KassenbuchViewModel.Instance.Kassenstand[2] + KassenbuchViewModel.Instance.Kassenstand[3]).ToString("C2"), cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase("Kontostand", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableBestandEinzeln.AddCell(new PdfPCell(new Phrase(KassenbuchViewModel.Instance.Kassenstand[1].ToString("C2"), cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                    //Tabelle Prüfer
                    var tablePrüfer = new PdfPTable(7);

                    tablePrüfer.WidthPercentage = 100;
                    tablePrüfer.LockedWidth = false;

                    tablePrüfer.SetWidths(new float[] { 1f, 0.5f, 2f, 0.5f, 3f, 0.5f, 2f }); // set column widths

                    tablePrüfer.AddCell(new PdfPCell(new Phrase("Prüfdatum:", headerFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });

                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", headerFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });

                    tablePrüfer.AddCell(new PdfPCell(new Phrase("Kassenwart:", headerFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });

                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", headerFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });

                    tablePrüfer.AddCell(new PdfPCell(new Phrase("Prüfer 1:", headerFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });

                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", headerFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });

                    tablePrüfer.AddCell(new PdfPCell(new Phrase("Prüfer 2:", headerFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthBottom = 0, BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });
                    tablePrüfer.AddCell(new PdfPCell(new Phrase(" ", cellFont)) { BorderWidthTop = 0, BorderWidthLeft = 0, BorderWidthRight = 0, HorizontalAlignment = Element.ALIGN_LEFT });



                    // add the table to the document
                    document.Add(tableBestandVorjahr);
                    document.Add(tableEinnahmenGesamt);
                    document.Add(tableEinnahmenEinzeln);
                    document.Add(tableAusgabenGesamt);
                    document.Add(tableAusgabenEinzeln);
                    document.Add(tableBestandGesamt);
                    document.Add(tableBestandEinzeln);
                    document.Add(new Paragraph("\n"));
                    document.Add(tablePrüfer);

                    // close the document
                    document.Close();
                }
            }
        }

        //Kassenbuch
        private bool CanExecuteExportKassenbuchCommand(object obj)
        {
            return true;
        }

        private void ExecuteExportKassenbuchCommand(object obj)
        {
            string fileName = HomeViewModel.Instance.AktuellesProjekt + string.Format("_Kassenbuch_{0:yyyyMMddHHmmss}.pdf", DateTime.Now);
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            // create a new document
            using (var document = new Document(PageSize.A4.Rotate(), marginLeft: 35f, marginTop: 50f, marginRight: 35f, marginBottom: 40f))
            {
                // create a new PdfWriter and bind it to the document
                using (var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create)))
                {
                    // open the document for writing
                    document.Open();

                    // create a dictionary to store the transactions by month
                    var monthGroups = new Dictionary<string, List<KassenbucheintragModel>>();

                    // iterate over the transactions and group them by month
                    foreach (var transaction in KassenbuchViewModel.Instance.AktuellesKassenbuch)
                    {
                        var month = transaction.MonthGroup;
                        if (!monthGroups.ContainsKey(month))
                        {
                            monthGroups[month] = new List<KassenbucheintragModel>();
                        }
                        monthGroups[month].Add(transaction);
                    }

                    writer.PageEvent = new HeaderFooter();

                    Dictionary<string, KassenbuchsummenModel> kassenbuchSummen = new Dictionary<string, KassenbuchsummenModel>();

                    string previousMonth = null;

                    // iterate over the month groups and create a table for each month
                    foreach (var monthGroup in monthGroups)
                    {
                        // add a new page for the table
                        document.NewPage();

                        AktuelleMonthGroup = monthGroup.Key;

                        // format cells
                        var headerFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 8);
                        var cellFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 8);

                        // create a table with 17 columns
                        var sumTableAlt = new PdfPTable(17);
                        sumTableAlt.HorizontalAlignment = Element.ALIGN_JUSTIFIED;

                        sumTableAlt.WidthPercentage = 100;
                        sumTableAlt.LockedWidth = false;

                        sumTableAlt.SetWidths(new float[] { 3f, 7f, 1f, 1.25f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f }); // set column widths

                        if (previousMonth == null)
                        {
                            // add the sums for the current month to the table
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("Summe " + HomeViewModel.Instance.VorherigesProjekt, headerFont)) { Colspan = 4, Rowspan = 2, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT });

                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { Rowspan = 2, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                            // add the sums for the current month to the table
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(KassenbuchViewModel.Instance.Kassenstand[5].ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(KassenbuchViewModel.Instance.Kassenstand[6].ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("0,00 €", cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                        }

                        else
                        {
                            // add the sums for the current month to the table
                            sumTableAlt.AddCell(new PdfPCell(new Phrase("Summe " + previousMonth, headerFont)) { Colspan = 4, Rowspan = 2, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT });

                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeBetrag.ToString("C2"), cellFont)) { Rowspan = 2, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeKontoEin.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeKontoAus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeKasseEin.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeKasseAus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer1Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer1Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer2Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer2Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer3Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer3Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer4Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer4Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                            // add the sums for the current month to the table
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeKonto.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeKasse.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer1.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer2.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer3.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                            sumTableAlt.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[previousMonth].SummeSteuer4.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                        }

                        previousMonth = monthGroup.Key;

                        // create a table with 17 columns
                        var table = new PdfPTable(17);
                        table.HorizontalAlignment = Element.ALIGN_JUSTIFIED;

                        table.WidthPercentage = 100;
                        table.LockedWidth = false;

                        table.SetWidths(new float[] { 3f, 7f, 1f, 1.25f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f }); // set column widths

                        // add the column headers to the table
                        table.AddCell(new PdfPCell(new Phrase("Datum", headerFont)) { HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase("Position", headerFont)) { HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase("B", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase("Nr.", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase("Betrag", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                        table.AddCell(new PdfPCell(new Phrase("Konto (Ein.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Konto (Aus.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Kasse (Ein.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Kasse (Aus.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer I (Ein.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer I (Aus.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer II (Ein.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer II (Aus.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer III (Ein.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer III (Aus.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer IV (Ein.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase("Steuer IV (Aus.)", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                        // Anzahl der Überschriftenzeilen festlegen
                        table.HeaderRows = 1;

                        // add the transactions for the current month to the table
                        foreach (var transaction in monthGroup.Value)
                        {
                            table.AddCell(new PdfPCell(new Phrase(transaction.Datum, cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Position, cellFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Kontobeleg, cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Belegnummer.ToString("N0"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Betrag.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.KontoEinnahme.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.KontoAusgabe.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.KasseEinnahme.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.KasseAusgabe.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer1Einnahme.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer1Ausgabe.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer2Einnahme.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer2Ausgabe.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer3Einnahme.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer3Ausgabe.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer4Einnahme.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(transaction.Steuer4Ausgabe.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        }

                        // create a table with 17 columns
                        var sumTableNeu = new PdfPTable(17);
                        sumTableNeu.HorizontalAlignment = Element.ALIGN_JUSTIFIED;

                        sumTableNeu.WidthPercentage = 100;
                        sumTableNeu.LockedWidth = false;

                        sumTableNeu.SetWidths(new float[] { 3f, 7f, 1f, 1.25f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f }); // set column widths

                        foreach (var transaction in monthGroup.Value)
                        {
                            if (!kassenbuchSummen.ContainsKey(monthGroup.Key))
                            {
                                kassenbuchSummen[monthGroup.Key] = new KassenbuchsummenModel() { Monat = monthGroup.Key };
                            }

                            kassenbuchSummen[monthGroup.Key].SummeBetrag += transaction.Betrag;
                            kassenbuchSummen[monthGroup.Key].SummeKontoEin += transaction.KontoEinnahme;
                            kassenbuchSummen[monthGroup.Key].SummeKontoAus += transaction.KontoAusgabe;
                            kassenbuchSummen[monthGroup.Key].SummeKasseEin += transaction.KasseEinnahme;
                            kassenbuchSummen[monthGroup.Key].SummeKasseAus += transaction.KasseAusgabe;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer1Ein += transaction.Steuer1Einnahme;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer1Aus += transaction.Steuer1Ausgabe;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer2Ein += transaction.Steuer2Einnahme;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer2Aus += transaction.Steuer2Ausgabe;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer3Ein += transaction.Steuer3Einnahme;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer3Aus += transaction.Steuer3Ausgabe;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer4Ein += transaction.Steuer4Einnahme;
                            kassenbuchSummen[monthGroup.Key].SummeSteuer4Aus += transaction.Steuer4Ausgabe;
                        }

                        // add the sums for the current month to the table
                        sumTableNeu.AddCell(new PdfPCell(new Phrase("Summe " + monthGroup.Key, headerFont)) { Colspan = 4, Rowspan = 2, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT });

                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeBetrag.ToString("C2"), cellFont)) { Rowspan = 2, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeKontoEin.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeKontoAus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeKasseEin.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeKasseAus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer1Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer1Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer2Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer2Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer3Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer3Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer4Ein.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer4Aus.ToString("C2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                        // add the sums for the current month to the table
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeKonto.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeKasse.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer1.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer2.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer3.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                        sumTableNeu.AddCell(new PdfPCell(new Phrase(kassenbuchSummen[monthGroup.Key].SummeSteuer4.ToString("C2"), cellFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });

                        // add the table to the document
                        document.Add(sumTableAlt);
                        document.Add(new Paragraph("\n"));

                        // add the table to the document
                        document.Add(table);
                        document.Add(new Paragraph("\n"));

                        // add the table to the document
                        document.Add(sumTableNeu);
                    }

                    // close the document
                    document.Close();
                }
            }
        }

        private bool CanExecuteExportKassenprüfberichtCommand(object obj)
        {
            return true;
        }

        private void ExecuteExportKassenprüfberichtCommand(object obj)
        {

            string fileName = HomeViewModel.Instance.AktuellesProjekt + string.Format("_Kassenprüfbericht_{0:yyyyMMddHHmmss}.pdf", DateTime.Now);
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            // create a new document
            using (var document = new Document(PageSize.A4, marginLeft: 35f, marginTop: 30f, marginRight: 35f, marginBottom: 40f))
            {
                // create a new PdfWriter and bind it to the document
                using (var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create)))
                {
                    // open the document for writing
                    document.Open();

                    // create a new font with a bigger size and bold style
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var textFont1 = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                    var textFont2 = FontFactory.GetFont(FontFactory.HELVETICA, 8);



                    // neue Seite
                    document.NewPage();

                    // create a new Paragraph with the title text and the title font
                    var title1 = new Paragraph("Kassenprüfbericht " + HomeViewModel.Instance.AktuellesProjekt, titleFont);

                    // ...
                    var par1 = new Paragraph("\ndes Fußballsportvereins FSV 1920 Guteborn e.V.\n\ndurch die Kassenprüfer:\n\n" +
                        "(1)            " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[1].Nachname + ", " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[1].Vorname + " (" + EinstellungenViewModel.Instance.AktuelleKassenpruefer[1].Strasse + ", " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[1].Ort + ")\n\n" +
                        "(2)            " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[2].Nachname + ", " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[2].Vorname + " (" + EinstellungenViewModel.Instance.AktuelleKassenpruefer[2].Strasse + ", " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[2].Ort + ")\n\n", textFont1);

                    par1.TabSettings = new TabSettings(100f);

                    var par3 = new Paragraph("\nund dem Kassenwart:\n\n" +
                        "(3)            " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[0].Nachname + ", " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[0].Vorname + " (" + EinstellungenViewModel.Instance.AktuelleKassenpruefer[0].Strasse + ", " + EinstellungenViewModel.Instance.AktuelleKassenpruefer[0].Ort + ")\n\n", textFont1);

                    par3.TabSettings = new TabSettings(100f);

                    var par4 = new Paragraph("\nDie Prüfung erfolgte am ____________ in der Zeit von _______ bis _______ Uhr.\n\n" +
                        "Zur Kassenprüfung wurden sämtliche Umsätze des Vereinskontos und des Bargeldbestandes des Geschäftsjahres " + HomeViewModel.Instance.AktuellesProjekt + " herangezogen. Nach den vorgelegten Unterlagen und den Erläuterungen des Kassenwarts erfolgte die Kassenprüfung ordnungsgemäß.\n\n" +
                        "Sämtliche Kontoauszüge sowie Rechnungen und Quittungen lagen vollständig vor und wurden von den oben genannten Personen sorgfältig geprüft.\n\n" +
                        "Der schriftliche Kassenbericht und die Kassenbücher des Vereins wurden von den in der Mitgliederversammlung gewählten Kassenprüfern kontrolliert und für richtig befunden. Dabei wurden auch die weiteren Unterlagen und die vorgelegten Belege eingesehen und stichprobenartige Kontrollen durchgeführt.\n\n" +
                        "Die Kassenbestände stimmen mit den in den Kassenbüchern vorgetragenen Salden überein. Richtigkeit besteht auch bei den komplett vorgelegten Bankauszügen des Vereins, bei Buchungen und bei den entsprechenden Belegen. Die Belege der Vereinsbuchführung waren feststellbar, übersichtlich und chronologisch geordnet.\n\n" +
                        "Unregelmäßigkeiten, also fehlende Belege, Verstöße oder Fehler in der Kassenführung, wurden nicht festgestellt.\n\nDer Kassenstand am 31.12." + HomeViewModel.Instance.AktuellesProjekt + " ist mit " + string.Format("{0:C2}", KassenbuchViewModel.Instance.Kassenstand[0]) + " ausgewiesen.\n\n" +
                        "Die Kassenprüfer empfehlen der Jahreshauptversammlung die Entlastung des Vorstandes.", textFont1);

                    var par5 = new Paragraph("\n\n\n____________________________                                                                                                    ____________________________", textFont2);

                    var par6 = new Paragraph("Unterschrift Kassenprüfer 1                                                                                                                Unterschrift Kassenprüfer 2", textFont2);

                    var par7 = new Paragraph("\nDie Prüfung erfolgte am ____________ in der Zeit von _______ bis _______ Uhr.\n\n" +
                        "Zur Kassenprüfung wurden sämtliche Umsätze des Vereinskontos und des Bargeldbestandes des Geschäftsjahres " + HomeViewModel.Instance.AktuellesProjekt + " herangezogen. Nach den vorgelegten Unterlagen und den Erläuterungen des Kassenwarts erfolgte die Kassenprüfung ordnungsgemäß.\n\n" +
                        "Sämtliche Kontoauszüge sowie Rechnungen und Quittungen lagen vollständig vor und wurden von den oben genannten Personen sorgfältig geprüft.\n\n" +
                        "Der schriftliche Kassenbericht und die Kassenbücher des Vereins wurden von den in der Mitgliederversammlung gewählten Kassenprüfern kontrolliert und für richtig befunden. Dabei wurden auch die weiteren Unterlagen und die vorgelegten Belege eingesehen und stichprobenartige Kontrollen durchgeführt.\n\n" +
                        "Die Kassenbestände stimmen mit den in den Kassenbüchern vorgetragenen Salden überein. Richtigkeit besteht auch bei den komplett vorgelegten Bankauszügen des Vereins, bei Buchungen und bei den entsprechenden Belegen. Die Belege der Vereinsbuchführung waren feststellbar, übersichtlich und chronologisch geordnet.\n\n" +
                        "Unregelmäßigkeiten, also fehlende Belege, Verstöße oder Fehler in der Kassenführung, wurden nicht festgestellt.\n\nDer Kassenstand am 31.12." + HomeViewModel.Instance.AktuellesProjekt + " ist mit 350,00 € ausgewiesen.\n\n" +
                        "Die Kassenprüfer empfehlen der Jahreshauptversammlung die Entlastung des Vorstandes.", textFont1);

                    //Blocksätze
                    par4.Alignment = Element.ALIGN_JUSTIFIED;
                    par7.Alignment = Element.ALIGN_JUSTIFIED;

                    // add the title to the document
                    document.Add(title1);
                    document.Add(par1);
                    document.Add(par3);
                    document.Add(par4);
                    document.Add(par5);
                    document.Add(par6);

                    // neue Seite
                    document.NewPage();

                    // create a new Paragraph with the title text and the title font
                    var title2 = new Paragraph("Kassenprüfbericht Ausschankkasse " + HomeViewModel.Instance.AktuellesProjekt, titleFont);

                    // add the title to the document
                    document.Add(title2);
                    document.Add(par1);
                    document.Add(par3);
                    document.Add(par7);
                    document.Add(par5);
                    document.Add(par6);

                    // add a blank line
                    document.Add(new Paragraph("\n"));

                    // close the document
                    document.Close();
                }
            }
        }
    }
}