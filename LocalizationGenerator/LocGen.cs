using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace LocalizationGenerator
{
    public class LocGen
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GenerateClassFromFastSharp(string source, TextBox erroroutput)
        {
            //Returned will be the whole class with Properties
            string result = string.Empty;

            //also generate fill method for target UserControl or Class that will set element values to localized strings
            string fillMethod = "public void FillLocalizationStrings() " + Environment.NewLine + "{" + Environment.NewLine;

            string currentClass = string.Empty;
            string constructor = string.Empty;
            List<KeyValuePair<string, string>> properties = new List<KeyValuePair<string, string>>(); //<key,eng.value>

            //split the source into lines
            var lines = Regex.Split(source, "\r\n|\r|\n");

            //whether section (class) declaration was found and not ended yet
            bool inclass = false;

            for (int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];

                ///skip Fast# comments
                if (line.StartsWith(";"))
                    continue;

                ///also skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                ///init new class
                if (line.StartsWith("##"))
                {
                    ///end previous clas before starting new one and attach Fill method
                    if (inclass)
                    {
                        ///close and insert constructor
                        constructor += "public Localized" + currentClass + "()" + Environment.NewLine + "{" + Environment.NewLine;
                        properties.ForEach(p =>
                        {

                            constructor += "\t" + p.Key + " = BIAS.Localization.LocManager.Instance.GetString(\"" + p.Key + "\");" + Environment.NewLine;
                        });
                        constructor += "} " + Environment.NewLine;
                        result += constructor;
                        constructor = "";
                        properties = new List<KeyValuePair<string, string>>();



                        ///close the Fill method and attach it before ending the class
                        fillMethod += Environment.NewLine + "}";
                        fillMethod = CommentPrefix(fillMethod);
                        result += fillMethod;
                        fillMethod = "public void FillLocalizationStrings() " + Environment.NewLine + "{" + Environment.NewLine;

                        
                        ///end the class
                        result += Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;
                        currentClass = string.Empty;
                    }

                    inclass = true;

                    ///if key is FileName.xaml, then name the class LocalizedFileName
                    result += Environment.NewLine + "public class Localized" + new String(line.Skip(2).TakeWhile(ch => ch != '.').ToArray<char>()) + Environment.NewLine + "{";
                    currentClass = new String(line.Skip(2).TakeWhile(ch => ch != '.').ToArray<char>());
                }
                ///section comments (will be class comments and XAML section comments)
                else if (line.StartsWith("#"))
                {
                    //if we are not in the very first or last lines of the file
                    if (i > 0 && i < lines.Count() - 1 )
                    {
                        ///and if this is comment and line before is empty and line after this is new class declaration
                        ///then end the class before writing comment (which belong following to class)
                        if (string.IsNullOrWhiteSpace(lines[i - 1]) && lines[i + 1].StartsWith("##") && inclass)
                        {
                            ///close and insert constructor
                            constructor += "public Localized" + currentClass + "()" + Environment.NewLine + "{" + Environment.NewLine;
                            properties.ForEach(p =>
                            {

                                constructor += "\t" + p.Key + " = BIAS.Localization.LocManager.Instance.GetString(\"" + p.Key + "\");" + Environment.NewLine;
                            });
                            constructor += "} " + Environment.NewLine;
                            result += constructor;
                            constructor = "";
                            properties = new List<KeyValuePair<string, string>>();


                            ///close the Fill method and attach it before ending the class
                            fillMethod += Environment.NewLine + "}";
                            fillMethod = CommentPrefix(fillMethod);
                            result += fillMethod;
                            fillMethod = "public void FillLocalizationStrings() " + Environment.NewLine + "{" + Environment.NewLine;

                            
                            ///end the class
                            result += Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;
                            inclass = false;
                            currentClass = string.Empty;
                        }
                    }
                    ///and add the comment
                    result += line.Replace("#", @"///") + Environment.NewLine;
                    continue;
                }
                ///items in section
                else
                {
                    var words = line.Split(new[] { "#" }, StringSplitOptions.None);

                    //key is mandatory
                    string key = words[0];
                    string englishText = string.Empty;
                    string elementProperty = string.Empty;
                    string trComment = string.Empty;
                    
                    //check that key is filled anyway
                    if (string.IsNullOrWhiteSpace(key))
                        erroroutput.Text += Environment.NewLine + "ERROR key is empty (line: " + i + ")";
                    
                    ///if there is only one thing, then it is key, and also english text
                    if (words.Count() == 1)
                    {
                        //turn CamelCase into Camel Case
                        englishText = Regex.Replace(key, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
                    }
    
                    //if there is at least one # separator on the line, then the second thing is english text
                    if (words.Count() >= 2)
                    {
                        englishText = words[1];
                        ///but if english is empty, then de-camelize the key and use that
                        if (string.IsNullOrWhiteSpace(englishText))
                            englishText = Regex.Replace(key, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
                    }

                    //third is element.Property for fill method
                    if (words.Count() >=3)
                        elementProperty = words[2];

                    if (!string.IsNullOrWhiteSpace(elementProperty))
                    {
                        fillMethod += elementProperty + " = Localized." + "key" + ";" + Environment.NewLine;
                    }

                    //last bit is comment for tranlators, so they know context of the word etc.
                    if (words.Count() >= 4)
                        trComment = words[3];


                    result += Environment.NewLine;
                    result += "\t" + @"/// <summary>" + Environment.NewLine;
                    if (englishText != string.Empty) result += "\t" + @"///" + "Eng: " + englishText + Environment.NewLine;
                    if (elementProperty != string.Empty) result += "\t" + @"///" + "Element.Property: " + elementProperty + Environment.NewLine;
                    if (trComment != string.Empty) result += "\t" + @"///" + "Comment for translation: " + trComment + Environment.NewLine;
                    result += "\t" + @"/// </summary>" + Environment.NewLine;

                    result += "\t" + "public string " + key + "{ get; set; }" + Environment.NewLine;

                    properties.Add(new KeyValuePair<string, string>(key, englishText));

                }

                ///if we are on the last line
                if (i == lines.Count() - 1) 
                {
                    ///close and insert constructor
                    constructor += "public Localized" + currentClass + "()" + Environment.NewLine + "{" + Environment.NewLine;
                    properties.ForEach(p =>
                    {

                        constructor += "\t" + p.Key + " = BIAS.Localization.LocManager.Instance.GetString(\"" + p.Key + "\");" + Environment.NewLine;
                    });
                    constructor += "} " + Environment.NewLine;
                    result += constructor;
                    constructor = "";
                    properties = new List<KeyValuePair<string, string>>();

                    
                    ///close the Fill method and attach it before ending the class
                    fillMethod += Environment.NewLine + "}";
                    fillMethod = CommentPrefix(fillMethod);
                    result += fillMethod;
                    fillMethod = "public void FillLocalizationStrings() " + Environment.NewLine + "{" + Environment.NewLine;


                    ///don't forget to end the last class
                    if (inclass)
                        result += Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

                    currentClass = string.Empty;
                }

            }

            return result;
        }


        /// <summary>
        /// Adds "///" prefix for each line in the fillMehodText
        /// Also prepends instruction about FillMethod
        /// </summary>
        /// <param name="fillMethodText"></param>
        public static string CommentPrefix(string fillMethodText)
        {
            string r = Environment.NewLine + @"/// Copy&Paste the Fill method into the target UserControl/Class to change the content of elements to localized versions." + Environment.NewLine;
            string[] lines = fillMethodText.Split(new[] { '\n' });
            return r + string.Join("\n", lines.Select(s => @"// " + s).ToArray()) + Environment.NewLine;
        }



        /// <summary>
        /// TODO refactor the two methods, so they use one base with parametric values (the parsing alg. is the same, only change is syntax - XML vs Class)
        /// (also there are the same blocks of code in the method, refactor this as well)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="txtOutput"></param>
        /// <returns></returns>
        internal static string GenerateXMLFromFastSharp(string source, TextBox erroroutput)
        {
            //Returned will be the whole class with Properties
            string result = string.Empty;

            //split the source into lines
            var lines = Regex.Split(source, "\r\n|\r|\n");

            //whether section declaration was found and not ended yet
            bool inclass = false;

            for (int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];

                ///skip Fast# comments
                if (line.StartsWith(";"))
                    continue;

                ///also skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                ///init new class
                if (line.StartsWith("##"))
                {
                    ///end previous section before starting new one
                    if (inclass)
                    {
                        ///end the section
                        result += "</section>" + Environment.NewLine + Environment.NewLine;
                    }

                    inclass = true;

                    ///if key is FileName.xaml, then name the section LocalizedFileName
                    result += Environment.NewLine + "<section key=\"" + new String(line.Skip(2).TakeWhile(ch => ch != '.').ToArray<char>()) + "\" >" + Environment.NewLine;
                }
                ///section comments
                else if (line.StartsWith("#"))
                {
                    //if we are not in the very first or last lines of the file
                    if (i > 0 && i < lines.Count() - 1)
                    {
                        ///and if this is comment and line before is empty and line after this is new class declaration
                        ///then end the section before writing comment (which belong following to class)
                        if (string.IsNullOrWhiteSpace(lines[i - 1]) && lines[i + 1].StartsWith("##") && inclass)
                        {
                            ///end the section
                            result += "</section>" + Environment.NewLine + Environment.NewLine;
                            inclass = false;
                        }
                    }
                    string tmpComent = line.Replace("#", "");
                    result += "<!--" + tmpComent + "-->";
                    continue;
                }
                ///items in section
                else
                {
                    var words = line.Split(new[] { "#" }, StringSplitOptions.None);

                    //key is mandatory
                    string key = words[0];
                    string englishText = string.Empty;
                    string elementProperty = string.Empty;
                    string trComment = string.Empty;

                    //check that key is filled anyway
                    if (string.IsNullOrWhiteSpace(key))
                        erroroutput.Text += Environment.NewLine + "ERROR key is empty (line: " + i + ")";

                    ///if there is only one thing, then it is key, and also english text
                    if (words.Count() == 1)
                    {
                        //turn CamelCase into Camel Case
                        englishText = Regex.Replace(key, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
                    }

                    //if there is at least one # separator on the line, then the second thing is english text
                    if (words.Count() >= 2)
                    {
                        englishText = words[1];
                        ///but if english is empty, then de-camelize the key and use that
                        if (string.IsNullOrWhiteSpace(englishText))
                            englishText = Regex.Replace(key, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
                    }

                    //third is element.Property for fill method - ignoring in XML
                    //if (words.Count() >= 3)
                    //    elementProperty = words[2];
                    //
                    //if (!string.IsNullOrWhiteSpace(elementProperty))
                    //{
                    //    fillMethod += elementProperty + " = Localized." + "key" + ";" + Environment.NewLine;
                    //}

                    //last bit is comment for tranlators, so they know context of the word etc.
                    if (words.Count() >= 4)
                        trComment = words[3];
                  
                    result += "\t<item key=\"" + key + "\"" + " TranslationContext=\"" + trComment + "\"" +  ">" + englishText + "</item>"  + Environment.NewLine;

                }

                ///if we are on the last line
                if (i == lines.Count() - 1)
                {

                    ///don't forget to end the last section
                    if (inclass)
                        result += "</section>" + Environment.NewLine + Environment.NewLine;
                }

            }

            return result;
        }
    }


    
}
