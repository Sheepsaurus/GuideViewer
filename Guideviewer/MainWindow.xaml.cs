﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.Win32;
using static Guideviewer.Progress;
using static Guideviewer.User;
using static Guideviewer.Data;

namespace Guideviewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public bool HasLoaded;
        
        public string UrlUserName
        {
            get => UrlUsername.Text;
            set => UrlUsername.Text = value.Replace(' ', '_');
        }

        //Parameters to handle GoogleRequest
        public IList<IList<object>> Values;
                
        public static List<string[]> ColumnList = new List<string[]>();

        //Struct to insert data from datasource in correct columns
        public struct Data
        {
            public string Qt { set; get; }
            public string L { set; get; }
            public string Mqc { set; get; }
            public string Cc { set; get; }
            public string Tcc { set; get; }
            public string Im { set; get; }
        }
        
        // ReSharper disable once RedundantDefaultMemberInitializer
        // Has the user clicked Apply?
        public static bool HasApplied = false;

        // The username, captured when the user clicks "Apply"
        public string ApplyUserName
        {
            get => ApplyUsername.Text;
            set => ApplyUsername.Text = value.Replace(' ', '_');
        }


        public MainWindow()
        {
            InitializeComponent();

            // Clear everything
            CheckboxesDictionary.Clear();
            ListViewSelectAllList.Clear();
            SelectAllCheckBoxes.Clear();
            AllCheckBoxes.Clear();
            ListViews.Clear();

            foreach (var tabcontrolItem in MainTabControl.Items)
            {
                if (tabcontrolItem is TabItem tabitem)
                {
                    foreach (var child in LogicalTreeHelper.GetChildren(tabitem))
                    {
                        if (child is Grid grid)
                        {
                            foreach (var gridChild in grid.Children)
                            {
                                if (gridChild is TabControl tabcontrol)
                                {
                                    foreach (var tabcontrolItem2 in tabcontrol.Items)
                                    {
                                        if (tabcontrolItem2 is TabItem tabitem2)
                                        {
                                            foreach (var child2 in LogicalTreeHelper.GetChildren(tabitem2))
                                            {
                                                if (child2 is Grid grid2)
                                                {
                                                    foreach (var grid2Child in grid2.Children)
                                                    {
                                                        if (grid2Child is ListView listview)
                                                        {
                                                            if (listview.Name.StartsWith("Mq") ||
                                                                listview.Name.StartsWith("Sa") ||
                                                                listview.Name.StartsWith("Co") ||
                                                                listview.Name.StartsWith("Tri"))
                                                            {
                                                                //MessageBox.Show("I just added " + listview.Name + " which is a ListView, to _listViews");
                                                                ListViews.Add(listview);
                                                            }
                                                            foreach (var checkbox in listview.Items)
                                                            {
                                                                if (checkbox is CheckBox cb)
                                                                {
                                                                    if (cb.Name.StartsWith("Sa"))
                                                                    {
                                                                        //MessageBox.Show("I just added " + cb.Name + " which is a SelectAll CheckBox, to _selectAllCheckBoxes");
                                                                        SelectAllCheckBoxes.Add(cb);
                                                                    }
                                                                    else
                                                                    {
                                                                        //MessageBox.Show("I just added " + cba.Name + " which is a CheckBox, to _allCheckBoxes");
                                                                        AllCheckBoxes.Add(cb);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < SelectAllCheckBoxes.Count; i++)
            {
                ListViewSelectAllList.Add(new Tuple<CheckBox, ListView>(SelectAllCheckBoxes[i], ListViews[i]));
            }

            foreach (var list in new List<List<CheckBox>>
            {
                // Master Quest + Completionist 
                new List<CheckBox> {Ann, Annc}, // Annihilator Title
                new List<CheckBox> {Aby, Abyc}, // The Abyss
                new List<CheckBox> {Bam, Bamc}, // Bandos Memories
                new List<CheckBox> {Frs, Frsc}, // Fremennik Sagas
                new List<CheckBox> {Tbo, Tboc}, // Tune Bane Ore
                new List<CheckBox> {Crv, Crvc}, // Carnillean Rising
                new List<CheckBox> {Tha, Thac}, // Thalassus
                new List<CheckBox> {Hag, Hagc}, // Hefin Agility Course
                new List<CheckBox> {Rsp, Rspc}, // Reconnect Spirit Tree
                new List<CheckBox> {Cle, Clec}, // Cleansing Shadow Cores
                new List<CheckBox> {Rob, Robc}, // Rush of Blood
                new List<CheckBox> {Mam, Mamc}, // Mahjarrat Memories
                new List<CheckBox> {Mge, Mgec}, // Memorial to Guthix Engrams
                new List<CheckBox> {Sme, Smec}, // Seren Memoriam
                new List<CheckBox> {Pme, Pmec}, // Prifddinas Memoriam
                new List<CheckBox> {Zme, Zmec}, // Zaros Memoriam
                new List<CheckBox> {Cme, Cmec}, // Core Memories
                new List<CheckBox> {Fko, Fkoc}, // Full Kudos Obtained
                new List<CheckBox> {Imt, Imtc}, // In Memory of the Myreque
                new List<CheckBox> {Rcl, Rclc}, // Returning Clarence
                new List<CheckBox> {Our, Ourc}, // Ouranaia Teleport
                new List<CheckBox> {Pre, Prec}, // Lost Potion Recipes
                new List<CheckBox> {Csr, Csrc}, // Crystal Singing Research
                new List<CheckBox> {Sop, Sopc}, // Stronghold of Player Safety
                new List<CheckBox> {Sos, Sosc}, // Stronghold of Security
                new List<CheckBox> {Ttr, Ttrc}, // The Lair of Tarn Razorlor
                new List<CheckBox> {Rco, Rcoc}, // Removing Corruption
                new List<CheckBox> {Hsw, Hswc}, // Hopespear's Will


                new List<CheckBox> {Sa03, Sa30}, // SelectAll Doric And Boric tasks
                new List<CheckBox> {D1, D1c}, // Doric Tasks
                new List<CheckBox> {D2, D2c},
                new List<CheckBox> {D3, D3c},
                new List<CheckBox> {D4, D4c},
                new List<CheckBox> {D5, D5c},
                new List<CheckBox> {D6, D6c},
                new List<CheckBox> {D7, D7c},
                new List<CheckBox> {D8, D8c},
                new List<CheckBox> {B1, B1c}, // Boric Tasks
                new List<CheckBox> {B2, B2c},
                new List<CheckBox> {B3, B3c},


                // Master Quest + Trimmed Completionist
                new List<CheckBox> {Ekm, Ekmt}, // Enchanted Key
                new List<CheckBox> {Aca, Acat}, // Ancient Cavern
                new List<CheckBox> {Ter, Tert}, // Temple Trekking
                new List<CheckBox> {Etr, Etrt}, // Eagle Transport Route
                new List<CheckBox> {Qbd, Qbdt}, // Queen Black Dragon Journals
                new List<CheckBox> {Bch, Bcht}, // Broken Home Challenges
                new List<CheckBox> {Cts, Ctst}, // Char's Treasured Symbol
                new List<CheckBox> {Uif, Uift}, // Upgrade Ivandis Flail
                new List<CheckBox> {Wip, Wipt}, // Witch's Potion
                new List<CheckBox> {Ton, Tont}, // Tales of Nomad
                new List<CheckBox> {Tgw, Tgwt}, // Tales of the God Wars
                new List<CheckBox> {Dsl, Dslt}, // Desert Slayer Dungeon
                new List<CheckBox> {Scn, Scnt}, // Scabarite Notes
                new List<CheckBox> {Sde, Sdet}, // Song from the Depths
                new List<CheckBox> {Shs, Shst}, // Sheep Shearer
                new List<CheckBox> {Mwk, Mwkt}, // Master White Knight
                new List<CheckBox> {Fta, Ftat}, // From Tiny Acorns
                new List<CheckBox> {Lhm, Lhmt}, // Lost Her Marbles
                new List<CheckBox> {Ago, Agot}, // A Guild of Our Own

                new List<CheckBox> {Swb, Swbt}, // Advanced Sweeping
                new List<CheckBox> {Swb2, Swb2t},
                new List<CheckBox> {Swb3, Swb3t},
                new List<CheckBox> {Swb4, Swb4t},
                new List<CheckBox> {Swb5, Swb5t},

                new List<CheckBox> {Bts, Btst}, // Around the World in Six Ways
                new List<CheckBox> {Bts2, Bts2t},
                new List<CheckBox> {Bts3, Bts3t},
                new List<CheckBox> {Bts4, Bts4t}
            })

            {
                CheckboxesDictionary.Add(list[0].Name, list);
                CheckboxesDictionary.Add(list[1].Name, list);
            }

            foreach (var cb in AllCheckBoxes)
            {
                if (cb.Content is TextBlock textB)
                {
                    foreach (Hyperlink hyperlink in LogicalTreeHelper.GetChildren(textB))
                    {
                        NameCompareTuples.Add(new Tuple<string, string>(cb.Name, hyperlink.ToString()));
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                ColumnList.Add(new string[new GoogleRequest().GoogleRequestInit().Execute().Values.Count]);
            }

            FirstLoad();
        }

        private void LoadOnline_OnClick (object sender, RoutedEventArgs e)
        {
            try
            {
                string runemetrics = new WebClient().DownloadString("https://apps.runescape.com/runemetrics/quests?user=" + UrlUserName);
                string hiscore = new WebClient().DownloadString("http://services.runescape.com/m=hiscore/index_lite.ws?player=" + UrlUserName);
                Loading.LoadUser(runemetrics, hiscore.Split('\n'), true);
                        SaveText(runemetrics, UrlUserName, new StreamWriter($"{UrlUserName}.txt"), DefaultIntArrayString);
            }
            catch (Exception d)
            {
                MessageBox.Show($"The username is either wrong or the user has set their profile to private. If the username is correct, contact a developer. \n\n Error: {d}");
            }
            finally
            {
                MessageBox.Show("User was successfully loaded, please \"Reload\"");
            }

            if (HasLoaded)
            {
                MessageBox.Show("Please use the Reset function before loading an accounts progress again");
            }

            HasLoaded = true;
        }

        //Fill all of the columns
        private void FillAllColumns()
        {
            if (Values != null)
            {
                for (int a = 0; a < Values.Count; a++)
                {
                    MyDataGrid.Items.Add(new Data
                    {
                        Qt = ColumnList[0][a],
                        L = ColumnList[1][a],
                        Mqc = ColumnList[2][a],
                        Cc = ColumnList[3][a],
                        Tcc = ColumnList[4][a],
                        Im = ColumnList[5][a]
                    });
                }
            }
        }

        private void FirstLoad()
        {
            //Google Request
            Values = new GoogleRequest().GoogleRequestInit().Execute().Values;

            //Insert the requested data into column arrays
            if (Values != null && Values.Count > 0)
            {
                for (var j = 0; j < Values.Count; j++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        ColumnList[i][j] = Values[j][i].ToString();
                    }
                }
            }
            
            FillAllColumns();

            for (int i = 0; i < LoadedSkillLevels.Length; i++)
            {
                if (i == 4)
                {
                    LoadedSkillLevels[i] = 10;
                    LoadedSkillExperiences[i] = 1154;
                }
                else
                {
                    LoadedSkillLevels[i] = 1;
                    LoadedSkillExperiences[i] = 0;
                }
                Levels[i] = new Tuple<string, int, int>(SkillNames[i], LoadedSkillLevels[i], LoadedSkillExperiences[i]);
            }
        }

        private void DeleteEmptyRows(object sender, RoutedEventArgs routedEventArgs)
        {            
            for (int i = ColumnList[0].Length - 1; i >= 0; i--)
            {
                var strings = new List<string> { ColumnList[0][i], ColumnList[1][i], ColumnList[2][i],
                                                ColumnList[3][i], ColumnList[4][i], ColumnList[5][i] };
                
                if (strings.All(x => x == strings.First()))
                {
                    MyDataGrid.Items.RemoveAt(i);
                }


            }
        }
        
        private void LoadFile_OnClick(object sender, RoutedEventArgs e)
        {
            Load();
            HasLoaded = false;
        }

        private void Reset(object sender, RoutedEventArgs routedEventArgs)
        {
            HasLoaded = false;
            MyDataGrid.Items.Clear();
            MessageBox.Show("ALL ITEMS WERE CLEARED");

            FirstLoad();
        }

        private void Reload(object sender, RoutedEventArgs e)
        {
            if (HasApplied)
            {
                HasApplied = false;
                CheckboxesBoolDictionary.Clear();

                for (int i = 0; i < AllCheckBoxes.Count; i++)
                {
                    switch (AllCheckBoxes[i].IsChecked)
                    {
                        case true:
                            CheckboxesBoolDictionary.Add(AllCheckBoxes[i].Name, true);
                            break;
                        case false:
                            CheckboxesBoolDictionary.Add(AllCheckBoxes[i].Name, false);
                            break;
                    }

                    MessageBox.Show(CheckboxesBoolDictionary.Count.ToString());

                    MessageBox.Show(AllCheckBoxes.Count.ToString());

                    MessageBox.Show(NameCompareTuples.Count.ToString());

                    

                    Specific.CheckBoxRemover(CheckboxesBoolDictionary, AllCheckBoxes[i], NameCompareTuples[i].Item1,
                        NameCompareTuples[i].Item2);
                }
            } else if (HasLoaded)
            {
                HasLoaded = false;
                CheckboxesBoolDictionary.Clear();

                for (int i = 0; i < AllCheckBoxes.Count; i++)
                {
                    switch (AllCheckBoxes[i].IsChecked)
                    {
                        case true:
                            CheckboxesBoolDictionary.Add(AllCheckBoxes[i].Name, true);
                            break;
                        case false:
                            CheckboxesBoolDictionary.Add(AllCheckBoxes[i].Name, false);
                            break;
                        default:
                            break;
                    }

                    Specific.CheckBoxRemover(CheckboxesBoolDictionary, AllCheckBoxes[i], NameCompareTuples[i].Item1, NameCompareTuples[i].Item2);
                }
            }

            MyDataGrid.Items.Clear();
            FillAllColumns();
        }
        private void Check(object sender, RoutedEventArgs routedEventArgs) { Switch(sender, true); }
        private void UnCheck(object sender, RoutedEventArgs routedEventArgs) { Switch(sender, false); }


        private void Switch(object sender, bool boolean)
        {
            if (sender is CheckBox senderBox)
            {
                // Main Duplicate Control
                if (CheckboxesDictionary.TryGetValue(senderBox.Name, out var value))
                {
                    foreach (var cb in value)
                    {
                        cb.IsChecked = boolean;
                    }
                }

                // Select All Control
                if (!senderBox.Name.StartsWith("Sa")) return;
                foreach (var listViewChild in LogicalTreeHelper.GetChildren(LogicalTreeHelper.GetParent(senderBox)))
                {
                    if (listViewChild is CheckBox cb)
                    {
                        cb.IsChecked = boolean;
                    }

                }
            }
        }

        public void HandleSelectAll()
        {
            foreach (var tabcontrolItem in MainTabControl.Items)
            {
                if (tabcontrolItem is TabItem tabitem)
                {
                    foreach (var child in LogicalTreeHelper.GetChildren(tabitem))
                    {
                        if (child is Grid grid)
                        {
                            foreach (var gridChild in grid.Children)
                            {
                                if (gridChild is TabControl tabcontrol)
                                {
                                    foreach (var tabcontrolItem2 in tabcontrol.Items)
                                    {
                                        if (tabcontrolItem2 is TabItem tabitem2)
                                        {
                                            foreach (var child2 in LogicalTreeHelper.GetChildren(tabitem2))
                                            {
                                                if (child2 is Grid grid2)
                                                {
                                                    foreach (var grid2Child in grid2.Children)
                                                    {
                                                        if (grid2Child is ListView listview)
                                                        {
                                                            List<CheckBox> availableCheckBoxes = new List<CheckBox>();
                                                            foreach (var checkbox in listview.Items)
                                                            {
                                                                if (checkbox is CheckBox cb && !cb.Name.StartsWith("Sa"))
                                                                {
                                                                    availableCheckBoxes.Add(cb);
                                                                }
                                                            }
                                                            foreach (var listviewItem in listview.Items)
                                                            {
                                                                if (listviewItem is CheckBox cb && cb.Name.StartsWith("Sa"))
                                                                {
                                                                    if (availableCheckBoxes.All(box => box.IsChecked == true))
                                                                    {
                                                                        cb.IsChecked = true;
                                                                    }
                                                                    else if (availableCheckBoxes.All(box => box.IsChecked == false))
                                                                    {
                                                                        cb.IsChecked = false;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public static string CheckboxStringSave(object sender, Dictionary<string, bool> boolDictionary)
        {
            CheckboxesBoolDictionary.Clear();
            string str = "";

            foreach (var cb in AllCheckBoxes)
            {
                switch (cb.IsChecked)
                {
                    case true:
                        CheckboxesBoolDictionary.Add(cb.Name, true);
                        str += "1,";
                        break;
                    case false:
                        CheckboxesBoolDictionary.Add(cb.Name, false);
                        str += "0,";
                        break;
                }
            }
            return str;
        }

        private void OnApplyOptions(object sender, RoutedEventArgs e)
        {
            HasApplied = true;
            string str = CheckboxStringSave(sender, CheckboxesBoolDictionary);
            string v = new WebClient().DownloadString("https://apps.runescape.com/runemetrics/quests?user=" + ApplyUserName);

            Progress.SaveText(v, ApplyUserName, new StreamWriter($"{ApplyUserName}.txt"), str);
        }

        private void OnOpenLoad(object sender, RoutedEventArgs routedEventArgs)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            string checkboxString = null;

            if (ofd.ShowDialog() == true)
            {
                string v = File.ReadLines(ofd.FileName).Skip(31).Take(1).First();
                if (v.EndsWith(","))
                {
                    checkboxString = v.Remove(v.LastIndexOf(','));
                }

                if (checkboxString != null)
                {
                    int[] checkBoxIntArray = Array.ConvertAll(checkboxString.Split(','), int.Parse);

                    for (var index = 0; index < checkBoxIntArray.Length; index++)
                    {
                        AllCheckBoxes[index].IsChecked = checkBoxIntArray[index] == 1;
                    }
                }
            }

            HandleSelectAll();

            ApplyUsername.Text = ofd.SafeFileName.Replace(".txt", "");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}