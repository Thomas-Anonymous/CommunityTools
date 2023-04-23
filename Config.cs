using System;
using SharpPcap;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

namespace CommunityTools {
    public partial class ConfigSniff : Form {

        public CaptureDeviceList allDevices;
        public int choixDevices;

        // ### CONFIGUREZ VOS PATHS ! ###
        string urlFileDecomp = @"D:\\DecompDof"; // Dossier où vous décompilerez le jeu
        string urlDofInvok = @"D:\Ankama\Dofus\DofusInvoker.swf"; // Lien du DofusInvoker.swf
        string FFDec = @"C:\Program Files (x86)\FFDec"; // Le path vers votre fichier FFed
        string cmd = @"C:\Windows\system32\cmd.exe"; // Le path vers votre CMD

        public void ProgressMax(int val) {
            if (progressFiles.InvokeRequired) {
                MethodInvoker invoker = delegate { ProgressMax(val); };
                progressFiles.Invoke(invoker);
                return;
            }

            progressFiles.Maximum = val;
        }
        public void ProgressVisibility(bool val) {
            if (progressFiles.InvokeRequired) {
                MethodInvoker invoker = delegate { ProgressVisibility(val); };
                progressFiles.Invoke(invoker);
                return;
            }

            progressFiles.Visible = val;
        }
        public void ProgressAddValue(int val) {
            if (progressFiles.InvokeRequired) {
                MethodInvoker invoker = delegate { ProgressAddValue(val); };
                progressFiles.Invoke(invoker);
                return;
            }

            progressFiles.Value += val;
            progressFiles.Refresh();
            tip.Show(progressFiles.Value + " / " + (progressFiles.Maximum - 1), progressFiles);
        }
        public void ProgressSetValue(int val) {
            if (progressFiles.InvokeRequired) {
                MethodInvoker invoker = delegate { ProgressSetValue(val); };
                progressFiles.Invoke(invoker);
                return;
            }

            progressFiles.Value = val;
            progressFiles.Refresh();
            tip.Show(progressFiles.Value + " / " + (progressFiles.Maximum - 1), progressFiles);
        }
        public void tipRefresh(string text, Control ctrl) {
            if (ctrl.InvokeRequired) {
                MethodInvoker invoker = delegate { tipRefresh(text, ctrl); };
                ctrl.Invoke(invoker);
                return;
            }

            tip.Show(text, ctrl);
        }
        public void ctrlEnabled(bool enable, Control ctrl) {
            if (ctrl.InvokeRequired) {
                MethodInvoker invoker = delegate { ctrlEnabled(enable, ctrl); };
                ctrl.Invoke(invoker);
                return;
            }

            ctrl.Enabled = enable;
        }

        public ConfigSniff(load wf) {
            wf.fermer();
            InitializeComponent();
            
            allDevices = CaptureDeviceList.Instance;
            string networkDefaultName = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault().Name;
            foreach (LibPcapLiveDevice _devs in allDevices) try { comboBox1.Items.Add(_devs.Interface.FriendlyName); } catch { comboBox1.Items.Add(_devs.Interface.Name); }
            foreach (string cob in comboBox1.Items) {
                if (cob == networkDefaultName) {
                    comboBox1.SelectedItem = cob;
                    break;
                }
            }

            List<Server> _serv = Primaire.getAllServers();

            foreach (Server _s in _serv) comboBox2.Items.Add(_s.nom);

            try {
                if (comboBox1.SelectedIndex == -1) comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
            } catch {
                MessageBox.Show("Erreur de connexion", "Impossible de trouver un périphérique internet !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(3);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            choixDevices = comboBox1.SelectedIndex;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            if (textBox1.Text.Length >= 7) Primaire.ipSniff = textBox1.Text; else radioButton1.Checked = true;
        }
        private void button1_Click(object sender, EventArgs e) {
            if (radioButton1.Checked) Primaire.ipSniff = Dns.GetHostAddresses(new Uri(Primaire.getServer(comboBox2.SelectedIndex + 1).ip).Host)[0].ToString();
            else Primaire.ipSniff = textBox1.Text;
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ConfigSniff_FormClosing(object sender, FormClosingEventArgs e) {
            if (DialogResult != DialogResult.OK) Environment.Exit(65);
        }

        private void button2_Click(object sender, EventArgs e) {
            new Thread(() => downloadLangs()).Start();
        }
        void downloadLangs() {
            ctrlEnabled(false, btnMajLang);
            if (Directory.Exists(urlFileDecomp + "\\lang")) Directory.Delete(urlFileDecomp + "\\lang",true);
            Directory.CreateDirectory(urlFileDecomp + "\\lang\\scripts");
            Directory.CreateDirectory(urlFileDecomp + "\\lang\\SWF");
            string getFiles = new WebClient().DownloadString("http://dofusretro.cdn.ankama.com/lang/versions_fr.txt");

            string[] files = getFiles.Split('=')[1].Split('|');
            ProgressMax(files.Length - 1);
            ProgressVisibility(true);
            foreach (string f in files) {
                if (f.Contains(",")) {
                    string[] splitFile = f.Split(',');

                    new WebClient().DownloadFile("http://dofusretro.cdn.ankama.com/lang/swf/" + splitFile[0] + "_fr_" + splitFile[2] + ".swf", urlFileDecomp + "\\lang\\SWF\\" + splitFile[0] + ".swf");

                    Process process = Process.Start(infos("ffdec.bat -selectclass DoAction -export script \"" + urlFileDecomp + "\" \"" + urlFileDecomp + "\\lang\\SWF\\" + splitFile[0] + ".swf\""));
                    process.WaitForExit();
                    process.Close();
                    Directory.Move(urlFileDecomp + "\\scripts\\frame_1", urlFileDecomp + "\\lang\\scripts\\" + splitFile[0]);
                    File.Move(urlFileDecomp + "\\lang\\scripts\\" + splitFile[0] + "\\DoAction.as", urlFileDecomp + "\\lang\\scripts\\" + splitFile[0] + "\\DoAction_1.as");
                    string[] doacs = Directory.GetFiles(urlFileDecomp + "\\lang\\scripts\\" + splitFile[0]).OrderBy(x => Int32.Parse(x.Split('\\')[6].Substring(x.Split('\\')[6].IndexOf('_') + 1, x.Split('\\')[6].IndexOf('.') - x.Split('\\')[6].IndexOf('_') - 1))).ToArray();

                    string compile = "";
                    foreach (string doac in doacs) compile += File.ReadAllText(doac);
                    File.WriteAllText(urlFileDecomp + "\\lang\\" + splitFile[0] + ".txt", compile);

                    ProgressAddValue(1);
                }
            }
            ProgressVisibility(false);
            ProgressSetValue(0);
            ctrlEnabled(true, btnMajLang);
        }

        private void button3_Click(object sender, EventArgs e) {
            new Thread(() => downloadSources()).Start();
        }
        void downloadSources() {
            ctrlEnabled(false, btnMajSource);
            if (!Directory.Exists(urlFileDecomp)) Directory.CreateDirectory(urlFileDecomp);
            Process process = Process.Start(infos("ffdec.bat -selectclass com.ankamagames.dofus.network.** -export script " + urlFileDecomp + " " + urlDofInvok));
            process.WaitForExit();
            tipRefresh("Décompilation 1/2 terminé", btnValider);
            process.Close();
            process = Process.Start(infos("ffdec.bat -selectclass com.ankamagames.jerakine.network.** -export script " + urlFileDecomp + " " + urlDofInvok));
            process.WaitForExit();
            process.Close();
            tipRefresh("Décompilation 2/2 terminé", btnValider);
            ctrlEnabled(true, btnMajSource);
        }

        ProcessStartInfo infos(string arg) {
            ProcessStartInfo commandInfo = new ProcessStartInfo();
            commandInfo.WorkingDirectory = FFDec;
            commandInfo.CreateNoWindow = true;
            commandInfo.UseShellExecute = false;
            commandInfo.RedirectStandardInput = false;
            commandInfo.RedirectStandardOutput = false;
            commandInfo.FileName = cmd;
            commandInfo.Arguments = "/C " + arg + " & exit";
            return commandInfo;
        }
    }
}