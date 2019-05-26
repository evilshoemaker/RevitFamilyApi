using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FamilyApi
{
    public partial class LoadFamilyForm : System.Windows.Forms.Form
    {
        private Document doc;

        public Family Family = null;
        public FamilySymbol Symbol { get; set; }

        public double StepSize
        {
            get { return (double)numericUpDown.Value; }
        }

        public LoadFamilyForm(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
        }

        private void LoadFamilyForm_Load(object sender, EventArgs e)
        {
            LoadFamilyName();
        }

        /// <summary>
        /// Загрузка списка семейств
        /// </summary>
        private void LoadFamilyName()
        {
            cmbFamily.Items.Clear();

            List<string> files = GetFilesFromPath(Util.FamilyForder);
            foreach (string i in files)
            {
                cmbFamily.Items.Add(Path.GetFileNameWithoutExtension(i));
            }
        }

        /// <summary>
        /// Возвращает список файлов по указанному пути
        /// </summary>
        /// <param name="patch"></param>
        /// <returns></returns>
        private List<string> GetFilesFromPath(string patch)
        {
            List<string> files = new List<string>(); // список для имен файлов 
            try
            {
                var txtFiles = Directory.EnumerateFiles(patch, "*.rfa", SearchOption.AllDirectories);

                foreach (string currentFile in txtFiles)
                {
                    //if (extensions.Contains(Path.GetExtension(currentFile).ToLower()))
                        files.Add(currentFile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return files;
        }

        private void button1_Click(object sender, EventArgs ev)
        {
            if (cmbFamily.SelectedItem != null)
            {
                string name = cmbFamily.SelectedItem.ToString();
                string FamilyPath = Util.FamilyForder + name + ".rfa";

                FilteredElementCollector a = new FilteredElementCollector(doc)
                    .OfClass(typeof(Family));

                Family = a.FirstOrDefault<Element>(
                  e => e.Name.Equals(name))
                    as Family;


                if (null == Family)
                {
                    if (!File.Exists(FamilyPath))
                    {
                        return;
                    }

                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Load Family");
                        doc.LoadFamily(FamilyPath, out Family);
                        tx.Commit();
                    }
                }

                foreach (FamilySymbol s in Family.Symbols)
                {
                    Symbol = s;
                    break;
                }
                //MessageBox.Show(count.ToString());

                /*FamilySymbolSet symbols = Family.Symbols;
                List<FamilySymbol> symbols2
                  = new List<FamilySymbol>(
                    symbols.Cast<FamilySymbol>());

                cmbType.DataSource = symbols2;
                cmbType.DisplayMember = "Name";*/
            }
            else
            {
                
            }
        }
    }
}
