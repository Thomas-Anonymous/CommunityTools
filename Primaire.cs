using System;
using System.IO;
using SharpPcap;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Test_Sniffeur {
    public partial class Primaire : Form {
        public ListCrafts listCrafts;
        public ListItems listItems;

        Sniffer sniffer;
        Thread _thBoucle;

        public string ipSniff;
        public ICaptureDevice device;


        public Primaire() {
            InitializeComponent();

            Visible = false;
            Opacity = 0;
            ShowInTaskbar = false;

            ConfigSniff _config = new ConfigSniff();
            DialogResult result = _config.ShowDialog();
            if (result == DialogResult.OK) {
                ipSniff = _config.choixIp;
                device = _config.allDevices[_config.choixDevices];

                listCrafts = new ListCrafts(this);
                listItems = new ListItems();
                sniffer = new Sniffer(this, device);
                Visible = true;
                Opacity = 100;
                ShowInTaskbar = true;
            }


            listCrafts.AddAllCrafts();
            listItems.AddAllItems();

            _thBoucle = new Thread(sniffer.Lecture);
            _thBoucle.Start();
        }

        public void ListBegin(ListView lv) {
            if (lv.InvokeRequired) {
                MethodInvoker invoker = delegate { ListBegin(lv); };
                lv.Invoke(invoker);
                return;
            }

            lv.BeginUpdate();
        }
        public void ListEnd(ListView lv) {
            if (lv.InvokeRequired) {
                MethodInvoker invoker = delegate { ListEnd(lv); };
                lv.Invoke(invoker);
                return;
            }

            lv.EndUpdate();
        }

        public void AddItem(ListView lv, ListViewItem lvi) {
            if (lv.InvokeRequired) {
                MethodInvoker invoker = delegate { AddItem(lv, lvi); };
                lv.Invoke(invoker);
                return;
            }

            lv.Items.Add(lvi);
        }
        public void AddItem(ListView lv, List<ListViewItem> lvi) {
            if (lv.InvokeRequired) {
                MethodInvoker invoker = delegate { AddItem(lv, lvi); };
                lv.Invoke(invoker);
                return;
            }

            lv.Items.AddRange(lvi.ToArray());
        }
        public void ClearLv(ListView lv) {
            if (lv.InvokeRequired) {
                MethodInvoker invoker = delegate { ClearLv(lv); };
                lv.Invoke(invoker);
                return;
            }

            lv.Items.Clear();
        }

        public void WriteLogs(RichTextBox rtb,string text) {
            if (rtb.InvokeRequired) {
                MethodInvoker invoker = delegate { WriteLogs(rtb,text); };
                rtb.Invoke(invoker);
                return;
            }
            
            rtb.AppendText(text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            try {
                sniffer.device.StopCapture();
                sniffer.device.Close();
            } catch { }
        }

        private void button1_Click(object sender, EventArgs e) {
            string saveStr = "";
            if (listView2.Items.Count > 0) {

                SaveFileDialog SFD = new SaveFileDialog();
                SFD.Title = "Sauvegarder sa liste de craft";
                SFD.DefaultExt = "txt";
                SFD.Filter = "txt files (*.txt)|*.txt";
                SFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                SFD.FileName = "liste crafts.txt";

                if (SFD.ShowDialog() != DialogResult.OK) return;

                foreach (ListViewItem item in listView2.Items) {
                    saveStr += item.SubItems[2].Text + "x" + item.SubItems[1].Text + " : " + item.SubItems[3].Text;
                    for (int i = 4; i < item.SubItems.Count; i++) {
                        string see = item.SubItems[i].Text;
                        if (see != "") {
                            saveStr += ", " + see;
                        }
                    }
                    saveStr += "\n";
                }

                File.WriteAllText(SFD.FileName, saveStr);
            }
        }
    }
}