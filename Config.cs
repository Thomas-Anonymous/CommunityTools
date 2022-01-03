using System;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Test_Sniffeur {
    public partial class ConfigSniff : Form {

        public CaptureDeviceList allDevices;
        public int choixDevices;
        public string choixIp;

        List<string[]> listServeurs = new List<string[]> {
            new string[] { "Boune", "172.65.226.26" }
        };

        public ConfigSniff() {
            InitializeComponent();
            allDevices = CaptureDeviceList.Instance;
            foreach (LibPcapLiveDevice _devs in allDevices) {
                try { comboBox1.Items.Add(_devs.Interface.FriendlyName); } catch { comboBox1.Items.Add(_devs.Interface.Name); }
                }
            foreach (string[] item in listServeurs) comboBox2.Items.Add(item[0]);

            try {
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
            } catch {
                MessageBox.Show("Erreur de connexion", "Impossible de trouver un périphérique internet !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(3);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            choixDevices = comboBox1.SelectedIndex;
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) {
            choixIp = listServeurs[comboBox2.SelectedIndex][1];
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            choixIp = listServeurs[comboBox1.SelectedIndex][1];
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            if (textBox1.Text.Length >= 7) choixIp = textBox1.Text;  else radioButton1.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ConfigSniff_FormClosing(object sender, FormClosingEventArgs e) {
            if (DialogResult != DialogResult.OK) Environment.Exit(65);
        }
    }
}
