using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Test_Sniffeur {
    class TraitementPackets {

        string[] dragodindes = new string[] { "Dragodinde Amande Sauvage", "Dragodinde Ebène", "Dragodinde Rousse Sauvage", "Dragodinde Ebène et Ivoire", "Dragodinde Rousse", "Dragodinde Ivoire et Rousse", "Dragodinde Ebène et Rousse", "Dragodinde Turquoise", "Dragodinde Ivoire", "Dragodinde Indigo", "Dragodinde Dorée", "Dragodinde Pourpre", "Dragodinde Amande", "Dragodinde Emeraude", "Dragodinde Orchidée", "Dragodinde Prune", "Dragodinde Amande et Dorée", "Dragodinde Amande et Ebène", "Dragodinde Amande et Emeraude", "Dragodinde Amande et Indigo", "Dragodinde Amande et Ivoire", "Dragodinde Amande et Rousse", "Dragodinde Amande et Turquoise", "Dragodinde Amande et Orchidée", "Dragodinde Amande et Pourpre", "Dragodinde Dorée et Ebène", "Dragodinde Dorée et Emeraude", "Dragodinde Dorée et Indigo", "Dragodinde Dorée et Ivoire", "Dragodinde Dorée et Rousse", "Dragodinde Dorée et Turquoise", "Dragodinde Dorée et Orchidée", "Dragodinde Dorée et Pourpre", "Dragodinde Ebène et Emeraude", "Dragodinde Ebène et Indigo", "Dragodinde Ebène et Turquoise", "Dragodinde Ebène et Orchidée", "Dragodinde Ebène et Pourpre", "Dragodinde Emeraude et Indigo", "Dragodinde Emeraude et Ivoire", "Dragodinde Emeraude et Rousse", "Dragodinde Emeraude et Turquoise", "Dragodinde Emeraude et Orchidée", "Dragodinde Emeraude et Pourpre", "Dragodinde Indigo et Ivoire", "Dragodinde Indigo et Rousse", "Dragodinde Indigo et Turquoise", "Dragodinde Indigo et Orchidée", "Dragodinde Indigo et Pourpre", "Dragodinde Ivoire et Turquoise", "Dragodinde Ivoire et Orchidée", "Dragodinde Ivoire et Pourpre", "Dragodinde Turquoise et Rousse", "Dragodinde Orchidée et Rousse", "Dragodinde Pourpre et Rousse", "Dragodinde Turquoise et Orchidée", "Dragodinde Turquoise et Pourpre", "Dragodinde Dorée Sauvage", "Dragodinde Squelette", "Dragodinde Orchidée et Pourpre", "Dragodinde Prune et Amande", "Dragodinde Prune et Dorée", "Dragodinde Prune et Ebène", "Dragodinde Prune et Emeraude", "Dragodinde Prune et Indigo", "Dragodinde Prune et Ivoire", "Dragodinde Prune et Rousse", "Dragodinde Prune et Turquoise", "Dragodinde Prune et Orchidée", "Dragodinde Prune et Pourpre", "Dragodinde en armure" };
        public bool getBanque = false;
        string inventaireBanque;

        Primaire _i;
        public TraitementPackets(Primaire instance) {
            _i = instance;
        }

        public int Search(byte[] src, byte[] pattern) {
            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            for (int i = 0; i < maxFirstCharSlot; i++) {
                if (src[i] != pattern[0])
                    continue;

                for (int j = pattern.Length - 1; j >= 1; j--) {
                    if (src[i + j] != pattern[j]) break;
                    if (j == 1) return i + pattern.Length;
                }
            }
            return -1;
        }

        public void ParseBanque(string _bank) {
            string[] _splt = _bank.Split(new string[] { ";" }, StringSplitOptions.None);

            List<ListViewItem> _LVI = new List<ListViewItem> { };
            foreach (string _ress in _splt) {
                if (_ress != "") {
                    int id = int.Parse(_ress.Split(new string[] { "~" }, StringSplitOptions.None)[1], System.Globalization.NumberStyles.HexNumber);
                    int qty = int.Parse(_ress.Split(new string[] { "~" }, StringSplitOptions.None)[2], System.Globalization.NumberStyles.HexNumber);
                    Items _getItem = _i.listItems.allItems.Find(x => x.id == id);
                    if (_getItem != null) {
                        _getItem.qty = qty;
                        _i.listItems.banque.Add(_getItem);
                        ListViewItem LVI = new ListViewItem(_getItem.id.ToString());
                        LVI.SubItems.Add(_getItem.nom);
                        LVI.SubItems.Add(_getItem.qty.ToString());
                        LVI.SubItems.Add(_getItem.description);

                        _LVI.Add(LVI);
                    } else {
                        MessageBox.Show(id + " en quantité " + qty + " n'a pas été trouvé dans la bdd ...");
                    }
                }
            }
            _i.ListBegin(_i.listView1);
            _i.AddItem(_i.listView1, _LVI);
            _i.ListEnd(_i.listView1);
        }

        public bool Verif(string text, string verif) {
            if (text.Length < verif.Length) return false; else return text.Substring(0, verif.Length) == verif;
        }

        public void TraitementRecu(byte[] getPacket) {
            string[] getText = Encoding.ASCII.GetString(getPacket).Replace("\0", "\n").Replace("\n\n", "\n").Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (string takeText in getText) {

                if (takeText == "" || takeText == "\n") return;

                _i.WriteLogs(_i.richTextBox1, takeText + "\n");
                _i.WriteLogs(_i.richTextBox3, BitConverter.ToString(getPacket).Replace("-", " ") + "\n");
                if (Verif(takeText, "DV")) {
                    _i.WriteLogs(_i.richTextBox5, "Fin de récupération de la banque.\n");
                    getBanque = false;
                    ParseBanque(inventaireBanque);
                    _i.listCrafts.SearchCrafts();
                } else if (Verif(takeText, "EL")) {
                    _i.WriteLogs(_i.richTextBox5, "Récupération de la banque ...\n");
                    getBanque = true;
                    inventaireBanque = "";
                    _i.listItems.banque = new List<Items> { };
                    _i.ClearLv(_i.listView1);
                    inventaireBanque += takeText;
                } else if (getBanque) {
                    inventaireBanque += takeText;
                } else if (Verif(takeText, "HG")) {
                    _i.WriteLogs(_i.richTextBox5, "Le serveur s'identifie.\n");
                } else if (Verif(takeText, "BN")) {
                    //_i.WriteLogs(_i.richTextBox5, "No timeout ...\n");
                } else if (Verif(takeText, "Aq")) {
                    _i.WriteLogs(_i.richTextBox5, "Les informations comptes arrivent.\n");
                } else if (Verif(takeText, "AV")) {
                    _i.WriteLogs(_i.richTextBox5, "Erreur : " + takeText.Replace("AV", "").Replace("0", "aucune") + ".\n");
                } else if (Verif(takeText, "Af")) {
                    _i.WriteLogs(_i.richTextBox5, "Chargement de la suite.\n");
                } else if (Verif(takeText, "ATK")) {
                    _i.WriteLogs(_i.richTextBox5, "Confirmation de la conformitée du compte.\n");
                } else if (Verif(takeText, "ALK")) {
                    string textReturn = "";
                    string[] _splt1 = takeText.Split(new string[] { "|" }, StringSplitOptions.None);
                    textReturn += "Prise d'informations durée d'abonnement: \n";
                    textReturn += SecondToTime(_splt1[0].Replace("ALK", "").Replace("\0", "")) + "\n";
                    textReturn += "Nombre de personnages sur le serveur : " + _splt1[1] + "\n";

                    for (int i = 2; i < _splt1.Length; i++) {
                        string[] _splt2 = _splt1[i].Split(new string[] { ";" }, StringSplitOptions.None);
                        string[] _splt3 = _splt2[7].Split(new string[] { "," }, StringSplitOptions.None);
                        string stuffVisu = "";
                        for (int j = 0; j < _splt3.Length; j++) if (_splt3[j] != "") stuffVisu += " Stuff " + (j + 1) + " : " + getItems(int.Parse(_splt3[j], System.Globalization.NumberStyles.HexNumber).ToString());
                        textReturn += "ID: " + _splt2[0] + " Pseudo : " + _splt2[1] + " Niveau : " + _splt2[2] + "\nCouleur 1: #" + _splt2[4].ToUpper() + " Couleur 2: #" + _splt2[5].ToUpper() + " Couleur 3: #" + _splt2[6].ToUpper() + stuffVisu + "\n";
                    }

                    _i.WriteLogs(_i.richTextBox5, textReturn);
                } else if (Verif(takeText, "Re+")) {
                    string[] _splt1 = takeText.Split(new string[] { ":" }, StringSplitOptions.None);
                    _i.WriteLogs(_i.richTextBox5, "Dragodinde ID: " + _splt1[0].Replace("Re+", "") + " type: " + dragodindes[int.Parse(_splt1[1]) + 1] + " nom: " + _splt1[4] + " lvl: " + _splt1[7] + "\n");
                } else if (Verif(takeText, "cMK")) {
                    string[] _splt1 = takeText.Split(new string[] { "|" }, StringSplitOptions.None);
                    string textReturn = "";
                    if (Verif(takeText, "cMK|")) textReturn += "Général: ";
                    else if (Verif(takeText, "cMK?|")) textReturn += "Recrutement: ";
                    else if (Verif(takeText, "cMK:|")) textReturn += "Commerce: ";
                    else if (Verif(takeText, "cMK%|")) textReturn += "Guilde: ";
                    else if (Verif(takeText, "cMKe|")) {
                        textReturn += "Event: ";
                        string[] _splt2 = _splt1[3].Split(new string[] { "," }, StringSplitOptions.None);
                        if (_splt2[0] == "GAME_EVENT_2_F") textReturn += _splt2[1] + " à gagné " + _splt2[2] + "kamas à la roulette bouftou.";
                        return;
                    }  else textReturn += "Canal?: ";

                    string message = _splt1[3];
                    string[] stuffs = _splt1[4].Split(new string[] { "," }, StringSplitOptions.None);

                    for (int i = 0; i < stuffs.Length - 1; i++) {
                        string item = getItems(stuffs[i].Split(new string[] { "!" }, StringSplitOptions.None)[0]);
                        message = message.Replace("°" + i, item);
                    }
                    textReturn += "[" + _splt1[1] + "] " + _splt1[2] + " : " + message + "\n";

                    _i.WriteLogs(_i.richTextBox5, textReturn);
                } else if (Verif(takeText, "Im0153")) {
                    _i.WriteLogs(_i.richTextBox5, "IP Actuelle: " + takeText.Split(new string[] { ";" }, StringSplitOptions.None)[1] + "\n");
                }
            }
        }

        public string getItems(string id) {
            return _i.listItems.allItems.Find(x => x.id.ToString() == id).nom;
        }

        public void TraitementEnvoie(byte[] getPacket) {
            string[] getText = Encoding.ASCII.GetString(getPacket).Replace("\0", "\n").Replace("\n\n", "\n").Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (string takeText in getText) {

                if (takeText == "" || takeText == "\0") return;

                _i.WriteLogs(_i.richTextBox2, takeText + "\n");
                _i.WriteLogs(_i.richTextBox4, BitConverter.ToString(getPacket).Replace("-", " ") + "\n");

                if (Verif(takeText, "AT")) {
                    _i.WriteLogs(_i.richTextBox6, "Envoie de l'ID (" + takeText.Substring(2, takeText.Length - 2).Replace("\n", "") + ").\n");
                } else if (Verif(takeText, "Ak0")) {
                    _i.WriteLogs(_i.richTextBox6, "Confirmation du client.\n");
                } else if (Verif(takeText, "AV")) {
                    _i.WriteLogs(_i.richTextBox6, "Demande de l'erreur.\n");
                } else if (Verif(takeText, "Ag")) {
                    _i.WriteLogs(_i.richTextBox6, "Envoie de la langue " + takeText.Substring(2, takeText.Length - 2).Replace("\n", "") + ".\n");
                } else if (takeText.Substring(0, 2) == "Ai") {
                    _i.WriteLogs(_i.richTextBox6, "Envoie de l'ID unique " + takeText.Substring(2, takeText.Length - 2).Replace("\n", "") + ".\n");
                } else if (Verif(takeText, "AL")) {
                    _i.WriteLogs(_i.richTextBox6, "Le client est prêt.\n");
                } else if (Verif(takeText, "Af")) {
                    _i.WriteLogs(_i.richTextBox6, "Demande d'informations (compte, liste d'attente ..).\n");
                } else if (Verif(takeText, "AS")) {
                    _i.WriteLogs(_i.richTextBox6, "Connexion en cours au personnage avec l'ID" + takeText.Replace("AS", "") + ".\n");
                }
            }
        }

        public string SecondToTime(string seconds) {
            DateTime temps = DateTime.Now.AddMilliseconds(double.Parse(seconds));

            return "Fin d'abonnement le : " + temps.Day.ToString("00") + "/" + temps.Month.ToString("00") + "/" + temps.Year + " à " + temps.Hour.ToString("00") + ":" + temps.Minute.ToString("00") + ":" + temps.Second.ToString("00") + ".";
        }
    }
}
