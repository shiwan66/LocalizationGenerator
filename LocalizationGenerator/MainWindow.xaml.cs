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

        private void bntOpenExtractorWindow_Click(object sender, RoutedEventArgs e)
        {
            Window we = new ExtractXAMLelements();
            we.Show();
        }

        
    }
}
