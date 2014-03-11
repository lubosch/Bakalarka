using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Office.Core;
using System.Text.RegularExpressions;
using Wordik.SQL;
using Updates_namespace;
//using MSMQ_Class;

namespace Wordik
{
    public partial class Ribbon1
    {
        private bool has_focus;
        private Microsoft.Office.Interop.Word.Application app;

        private IntPtr HWND = IntPtr.Zero;

        Word_SQL document_sql = null;



        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();


        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            new Updates(0);
            app = Globals.ThisAddIn.Application;
            has_focus = true;
            app.WindowActivate += app_WindowActivate;
            app.WindowDeactivate += app_WindowDeactivate;
        }

        private void app_WindowActivate(Document Doc, Window Wn)
        {
            if (this.HWND == IntPtr.Zero)
            {
                this.HWND = GetForegroundWindow();
            }
        }


        private void app_WindowDeactivate(Document Doc, Window Wn)
        {
            if (!checkBox1.Checked || app.Documents.Count == 0) return;
            try
            {
                Document document = app.ActiveDocument;

                String filePath = document.FullName;
                String docName = document.Name;

                DateTime timestamp = DateTime.Now;

                Boolean isVariable = false;
                foreach (Variable o in document.Variables)
                {
                    if (o.Name == "Bakalarka UID")
                    {
                        isVariable = true;
                        document_sql = new Word_SQL(int.Parse(o.Value));
                        document_sql.save(timestamp);
                    }
                }

                if (!isVariable || document_sql == null || document_sql.ID == -1)
                {
                    document_sql = new Word_SQL(docName, filePath);
                    document_sql.save(timestamp);
                    document.Variables.Add("Bakalarka UID", document_sql.ID);
                }


                String[] titles = new String[10];

                Paragraph paragraph = null;
                int actualLevel = 10;
                paragraph = app.Selection.Paragraphs[1];
                while (paragraph.Previous() != null && actualLevel != 1)
                {
                    Style s = paragraph.get_Style();
                    Match match = Regex.Match(s.NameLocal, @"\ \d$");
                    if (match.Success)
                    {

                        int level = int.Parse(match.Value.Trim());
                        if (actualLevel == 10)
                        {
                            titles[level + 1] = null;
                        }
                        if (level < actualLevel)
                        {
                            actualLevel = level;
                            titles[level] = paragraph.Range.Text.Trim();
                        }
                    }
                    paragraph = paragraph.Previous();
                }

                Title_SQL title_SQL = null;
                int i = 1;
                while (titles[i] != null)
                {
                    int previous_id = -1;
                    if (title_SQL != null)
                    {
                        previous_id = title_SQL.ID;
                    }
                    title_SQL = new Title_SQL(document_sql.ID, i, titles[i], previous_id);
                    title_SQL.save();
                    Debug.WriteLine(i + ". " + titles[i] + " ID> " + title_SQL.ID);
                    i += 1;
                }


                paragraph = app.Selection.Paragraphs[1].Previous();
                string slovo = "";

                if (paragraph != null)
                {
                    slovo = paragraph.Range.Text;
                }

                paragraph = app.Selection.Paragraphs[1];
                String slovoMain = paragraph.Range.Text;
                slovo = slovo + paragraph.Range.Text;
                paragraph = app.Selection.Paragraphs[1].Next();
                if (paragraph != null)
                {
                    slovo = slovo + paragraph.Range.Text;
                }

                if (slovo.Length > 1000)
                {
                    slovo = slovoMain;
                    if (slovo.Length > 1000)
                    {
                        slovo = slovo.Substring(0, 1000);
                    }

                }

                Text_SQL text_sql = new Text_SQL(document_sql.ID, slovo);
                text_sql.save();


                Debug.WriteLine(slovo);
                //komunikator.SendMessageToQueue(slovo);
            }
            catch (System.Exception ex)
            {
                if (ex is System.Data.SqlClient.SqlException || ex is WebException)
                {
                    checkBox1.Checked = false;
                }
                Error.Show(ex, User.get_uniq_id().ToString());
            }

        }

    }

}
