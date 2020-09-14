﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SimpleSearch;

namespace RimDef
{
    public partial class Form1 : Form
    {
        XMLReader xmlReader = new XMLReader();

        List<Def> defs = new List<Def>();
        List<Def> defsView = new List<Def>();

        public Form1()
        {
            InitializeComponent();

            txtModDir.Text = @"C:\Games\RimWorld Royalty\Mods";

            lwDetail.Columns.Add("amount", 50);
            lwDetail.Columns.Add("ingredients", 150);
            lwDetail.Columns.Add("products", 100);
            lwDetail.Columns.Add("work", 50);
        }

        private void loadModList(string path)
        {
            xmlReader.modDir = path;

            defs.Clear();
            lbMods.Items.Clear();
            lbDefTypes.Items.Clear();
            lwDefs.Items.Clear();
            gbRecipe.Visible = false;
            gbDesc.Visible = false;
            pictureBox1.Visible = false;

            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    string[] split = dir.Split('\\');
                    string name = split[split.Length - 1];
                    lbMods.Items.Add(name);

                    // depending on the number of mods, this can take very long.
                    //defs.AddRange(xmlReader.loadAllDefs(name));
                }
            }
            catch (Exception ex) { Console.WriteLine("Error loading modlist: " + ex); }
        }

        private void lbMods_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mod = lbMods.Items[lbMods.SelectedIndex].ToString();

            defs = xmlReader.loadAllDefs(mod);
            defsView = defs;
            //defsView = xmlReader.loadAllDefs(mod);

            lwDefs.Items.Clear();
            lwDefs.Columns.Clear();
            lbDefTypes.Items.Clear();
            xmlView.Clear();
            gbDesc.Visible = false;
            gbRecipe.Visible = false;
            pictureBox1.Visible = false;

            lwDefs.Columns.Add("Type", 100);
            lwDefs.Columns.Add("Name", 120);
            lwDefs.Columns.Add("Label", 150);

            foreach (Def def in defs)
            {
                string[] items = { def.defType, def.defName, def.label };
                var listViewItem = new ListViewItem(items);
                listViewItem.ToolTipText = "tooltip test";
                lwDefs.Items.Add(listViewItem);
            }
            
            foreach (string item in xmlReader.defTypes)
            {
                lbDefTypes.Items.Add(item);
            }
        }

        private void lbDefTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbDefTypes.SelectedIndices.Count > 0)
            {
                string selectedType = xmlReader.defTypes[lbDefTypes.SelectedIndices[0]];
                lwDefs.Items.Clear();

                defsView = new List<Def>();
                foreach (Def def in defs)
                {
                    if (def.defType == selectedType)
                    {
                        defsView.Add(def);
                        string[] items = { def.defType, def.defName, def.label };
                        var listViewItem = new ListViewItem(items);
                        lwDefs.Items.Add(listViewItem);
                    }
                }
            }
        }

        private void lwDefs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lwDefs.SelectedIndices.Count > 0)
            {
                Def def = defsView[lwDefs.SelectedIndices[0]];

                gbRecipe.Visible = false;
                gbDesc.Visible = false;
                pictureBox1.Visible = false;

                xmlView.Text = def.xml;

                if (def.defType == "RecipeDef")
                {
                    RecipeDef recipe = (RecipeDef) def;

                    lwDetail.Items.Clear();

                    foreach (string[] li in recipe.ingredients)
                    {
                        var listViewItem = new ListViewItem(li);
                        lwDetail.Items.Add(listViewItem);
                    }

                    gbRecipe.Visible = true;
                }

                if (def.defType == "ThingDef")
                {
                    Console.WriteLine("texture path = " + def.texture);

                    Bitmap image = new Bitmap(RimDef.Properties.Resources.nopic);
                    
                    if (File.Exists(def.texture))
                    {
                        try
                        {
                            image = new Bitmap(def.texture);
                        }
                        catch (Exception ex) { Console.WriteLine(ex); }
                    }

                    pictureBox1.Image = (Image)image;
                    pictureBox1.Visible = true;
                    pictureBox1.Refresh();
                }

                if (def.description != "")
                {
                    thingDesc.Text = def.description;
                    gbDesc.Visible = true;
                }
            }
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtModDir.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            loadModList(txtModDir.Text);
        }

        private SearchCore SearchCore { get; set; }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(sender, e);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var searchers = new List<Searcher> { new DefSearcher(defs) };
            SearchCore = new SearchCore(searchers);

            lwDefs.Items.Clear();
            lwDefs.Columns.Clear();
            lwDefs.Columns.Add("Mod", 150);
            lwDefs.Columns.Add("Type", 150);
            lwDefs.Columns.Add("Name", 150);
            lwDefs.Columns.Add("Label", 150);

            string searchText = txtSearch.Text;
            Console.WriteLine(searchText);

            var model = new SearchResponse();
            var s = new System.Diagnostics.Stopwatch();
            s.Start();
            model.Results = SearchCore.Search(searchText);
            s.Stop();
            model.TimeTaken = s.Elapsed;

            foreach (SearchResult result in model.Results)
            {
                Def def = result.Definition;
                string[] items = { def.modName, def.defType, def.defName, def.label };
                var listViewItem = new ListViewItem(items);
                lwDefs.Items.Add(listViewItem);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        { }
    }
}
