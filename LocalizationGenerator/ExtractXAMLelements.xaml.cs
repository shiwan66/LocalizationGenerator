using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace LocalizationGenerator
{
    /// <summary>
    /// Interaction logic for ExtractXAMLelements.xaml
    /// </summary>
    public partial class ExtractXAMLelements : Window
    {
        public ExtractXAMLelements()
        {
            InitializeComponent();
        }

        string lastDirPath = @"C:\";

        private void txtOpenFile_Click_1(object sender, RoutedEventArgs e)
        {
            
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Import XAML files(s) for extracting translatable elements into Fast# language",
                DefaultExt = ".xaml",
                Filter = "XAML files (.xaml)|*.xaml",
                InitialDirectory = lastDirPath,
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                //string content = System.IO.File.ReadAllText(dlg.FileName);
                foreach(var f in dlg.FileNames)
                {

                    XElement root = XElement.Load(f);

                    var x = root.Ancestors("Grid");

                    var txb = root.Descendants().Where(d => d.Name.LocalName == "TextBlock");

                    //var textBlocks = root.Elements("TextBlock")
                    //     .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Name" || atr.Name.LocalName == "x:Name"))
                    //     .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Text"));

                    //var runs = root.Elements("Run")
                    //     .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Name" || atr.Name.LocalName == "x:Name"))
                    //     .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Text"));

                    //CsQuery.CQ xaml = File.ReadAllText(f);

                    //var c = xaml["TextBlock"].Count();

                }
                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }
    }
}
