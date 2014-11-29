using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;

namespace LocalizationGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            helpLabel.ToolTip = helpLabelString;
        }

        /// <summary>
        /// Generate from fast#
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var s = txtFastSharp.Text;
            txtClass.Text = LocGen.GenerateClassFromFastSharp(s, txtOutput);
        }

        private void btnXMLfromFast_Click(object sender, RoutedEventArgs e)
        {
            var s = txtFastSharp.Text;
            txtXML.Text = LocGen.GenerateXMLFromFastSharp(s, txtOutput);
        }

        /// <summary>
        /// Extract elements from selected XAML files and generate fast# script from them.
        /// 
        /// Elements: TextBlock, Run, Label, Button
        /// 
        /// TextBlock and Run MUST HAVE property Text
        /// Label and Button MUST HAVE property Content
        /// 
        /// Content that is defined as body of the element e.g. <TextBlock>text</TextBlock> is ignored
        /// All elements must have Name or x:Name attribute
        /// </summary>
        private void bntExtractFromXAML_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Import XAML files(s) for extracting translatable elements into Fast# language",
                DefaultExt = ".xaml",
                Filter = "XAML files (.xaml)|*.xaml",
                Multiselect = true
            };

            txtFastSharp.Text = "";

            if (dlg.ShowDialog() == true)
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                ///for every selected file
                foreach (var f in dlg.FileNames)
                {
                    ///load the file for woriking winth LINQ to XML
                    XElement root = XElement.Load(f);

                    ///select all TextBlock elements (Descendants is recusrive, parameter XName contains the name in property LocalName)
                    var textblocks = root.Descendants().Where(d => d.Name.LocalName == "TextBlock")
                        ///only select those that have "Name" or "x:Name" attribute
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Name" || atr.Name.LocalName == "x:Name"))
                        ///and only select those that have "Text" property (this leaves out TextBlocks that only enclose Runs)
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Text")); //

                    var runs = root.Descendants().Where(d => d.Name.LocalName == "Run")
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Name" || atr.Name.LocalName == "x:Name"))
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Text"));

                    var labels = root.Descendants().Where(d => d.Name.LocalName == "Label")
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Name" || atr.Name.LocalName == "x:Name"))
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Content"));

                    var buttons = root.Descendants().Where(d => d.Name.LocalName == "Button")
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Name" || atr.Name.LocalName == "x:Name"))
                         .Where(txt => txt.Attributes().Any(atr => atr.Name.LocalName == "Content"));

                    ///Every file into its own section named after the filename
                    txtFastSharp.Text += Environment.NewLine + "##" + new FileInfo(f).Name + Environment.NewLine;

                    //process TextBlocks and Runs in one loop, they have the same properties (Text)
                    var texts = textblocks.Concat(runs);

                    foreach (var element in texts)
                    {
                        //get thë Name="btnExample"
                        txtFastSharp.Text += element.Attributes().First(attr => attr.Name.LocalName.Contains("Name")).Value;
                        txtFastSharp.Text += "#";
                        //get value of the text
                        txtFastSharp.Text += element.Attributes().First(attr => attr.Name.LocalName == "Text").Value; //.Contains bug TextWrapping
                        txtFastSharp.Text += Environment.NewLine;
                    }

                    var contents = labels.Concat(buttons);
                    
                    foreach (var element in contents)
                    {
                        txtFastSharp.Text += element.Attributes().First(attr => attr.Name.LocalName.Contains("Name")).Value;
                        txtFastSharp.Text += "#";
                        txtFastSharp.Text += element.Attributes().First(attr => attr.Name.LocalName.Contains("Content")).Value;
                        txtFastSharp.Text += Environment.NewLine;
                    }
                }

                //remove empty line from end
                txtFastSharp.Text = txtFastSharp.Text.TrimEnd();

                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }

        public string helpLabelString = @" Fast# ""language"" for generating localization XML and binding classes via LocalizationGenerator
semicilon as first character means that line is comment in fast# and will be ignored



#section comment that will be turned into <!-- XML comments --> or /// class triple comments
##SectionName
key#english text#element.property#comment for translation
Testno#Test No.#textbox1.Text#test number

****************************************************************
key # english text # element.property # comment for translation
****************************************************************

SectionName and key is mandotory, rest is optional
if only key is provided then english text=key (changes CamelWords into Camel Words)

referencing existing key: start line with @ e.g.: 
 @back##btnBack.Content
 @continue##btnContinue.Content
 these will be ommited from XML, only FillMethod will be generated";
    }
}
