using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
//For at kunne arbejde (konvertere) med pdf filer
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
//For at kunne arbejde med word filer
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
//for vores apps udseende
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;
//for at kunne starte en process vi skal bruge diagnostics
using System.Diagnostics;
//for at kunne arbejde (konvertere) med billeder
using System.Drawing;
//for at gemme og læse det filer som vi skal konvertere
using System.IO;



namespace ConverterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //skifter udseende af vores program
            FluentTheme fluentTheme = new FluentTheme()
            {
                ThemeName = "FluentLight",
                HoverEffectMode = HoverEffect.None,
                PressedEffectMode = PressedEffect.Glow,
                ShowAcrylicBackground = false
            };

            FluentLightThemeSettings themeSettings = new FluentLightThemeSettings();
            //themeSettings.BodyFontSize = 16;
            themeSettings.FontFamily = new System.Windows.Media.FontFamily("Barlow");
            SfSkinManager.RegisterThemeSettings("FluentLight", themeSettings);
            SfSkinManager.SetTheme(this, fluentTheme);
            InitializeComponent();
        }
        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if(pathTextBox.Text == String.Empty)
            {
                MessageBox.Show("Please select a file");
                return;
            }
            //vores dropdown menu hvor vi vælger til hvilken format skal filen konverteres
            switch (conversionDropDown.SelectedIndex) 
            {
                case 0://Konverterer Doc til PDf
                    ConvertDocToPDF(pathTextBox.Text);
                    break;
                 case 1:
                    ConvertPDFtoDoc(pathTextBox.Text);
                    break;
                case 2:
                    ConvertPNGToPDF(pathTextBox.Text);
                    break;

                default:
                    MessageBox.Show("Please select a option");
                    return ;
            }
            
            OpenFolder(pathTextBox.Text);
            MessageBox.Show("The file is successfully converted and saved on the same destination folder!");
            return;
        }
        private void ConvertDocToPDF(string docPath)
        {//konverterer vores doc til en pdf fil
            WordDocument doc = new WordDocument(docPath, FormatType.Automatic);
            DocToPDFConverter converter = new DocToPDFConverter();
            PdfDocument pdfDocument = converter.ConvertToPDF(doc);
            //gemmer filen
            string newPDFPath = docPath.Split('.')[0] + ".pdf";
            pdfDocument.Save(newPDFPath);
            pdfDocument.Close(true);
            doc.Close();

        }
        private void ConvertPNGToPDF (string pngPath)
        {
            PdfDocument pdfDoc = new PdfDocument();
            //åbner filen
            PdfImage pdfImage = PdfImage.FromStream(new FileStream(pngPath, FileMode.Open));
  
            PdfPage pdfPage = new PdfPage();
            PdfSection pdfSection = pdfDoc.Sections.Add();
            //konverterer billederne til sider
            pdfSection.Pages.Insert(0, pdfPage);
            pdfPage.Graphics.DrawImage(pdfImage, 0, 0);
            //samme split som tidligere
            string newPNGPath = pngPath.Split('.')[0] + ".pdf";
            //gemmer den konverterede fil
            pdfDoc.Save(newPNGPath);
            pdfDoc.Close(true);
        }
        private void ConvertPDFtoDoc(string pdfPath)
        {
            WordDocument wordDocument = new WordDocument();
            //tilføjer ny section til filen
            IWSection section = wordDocument.AddSection();
            //section kan bruges til at sætte margin til siden
            section.PageSetup.Margins.All = 0;

            IWParagraph firstParagraph = section.AddParagraph();

            SizeF defaultPageSize = new SizeF(wordDocument.LastSection.PageSetup.PageSize.Width,
                wordDocument.LastSection.PageSetup.PageSize.Height);
            //loader filen
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(pdfPath))
            {
                //går igennem alle sider i vores dokument
                for (int i = 0; i < loadedDocument.Pages.Count; i++)
                {
                    //eksporterer
                    using (var image = loadedDocument.ExportAsImage(i, defaultPageSize, false))
                    {
                        IWPicture picture = firstParagraph.AppendPicture(image);
                        picture.Width = image.Width;
                        picture.Height = image.Height;
                    }
                }
            };
            //gemmer filen til den konverterede format og lukker filen
            string newPDFPath = pdfPath.Split('.')[0] + ".docx";
            wordDocument.Save(newPDFPath);
            wordDocument.Dispose();

        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                //den kode giver os filnavnet af den valgte element
                //pathTextBox er den textbox som vi har i vores app
                pathTextBox.Text = ofd.FileName;
                
            }
            

        }
        //bruger vi for at åbne den mappe hvor der ligger filen
        private void OpenFolder(string folderPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                //åbner file explorer i Windows
                Arguments = folderPath.Substring(0, folderPath.LastIndexOf('\\')),
                FileName = "explorer.exe"
            };


        }

        //Indstiller vi vores Drag and Drop Box
        private void FileDropPanel_Drop(object sender,DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //herfra indstiller vi at den skal visse placeringen af filen uanset om det er brugt drag&drop eller choose file
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                pathTextBox.Text = files[0];
            }
        }

       
    }
}

