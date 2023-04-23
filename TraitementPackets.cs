using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CommunityTools {
    public class TraitementPackets {
        public string msReceive = "", msWrite = "";
        SQLiteConnection conn = Primaire.conn;

        public bool drago = false;

        public void Start() {
            new Thread(() => TraitementRecu()).Start();
            new Thread(() => TraitementEnvoie()).Start();
        }

        public void addMSReceive(string buff) {
            msReceive += buff;
        }
        public void addMSWrite(string buff) {
            msWrite += buff;
        }

        public void ParseBanque(string getBanque) {
            ClearBanque();
            string[] _splt = getBanque.Split(new string[] { ";" }, StringSplitOptions.None);

            List<ListViewItem> _LVI = new List<ListViewItem> { };
            foreach (string _ress in _splt) {
                if (_ress != "") {
                    try {
                        int id = int.Parse(_ress.Split(new string[] { "~" }, StringSplitOptions.None)[1], System.Globalization.NumberStyles.HexNumber);
                        int qty = int.Parse(_ress.Split(new string[] { "~" }, StringSplitOptions.None)[2], System.Globalization.NumberStyles.HexNumber);

                        Item _getItem = Primaire.getItems(id);
                        if (_getItem != null) {
                            addBanque(id, qty);

                            ListViewItem LVI = new ListViewItem(id.ToString());
                            LVI.SubItems.Add(_getItem.nom);
                            LVI.SubItems.Add(qty.ToString());
                            LVI.SubItems.Add(Math.Min(Math.Min(_getItem.prix1, _getItem.prix10 / 10), _getItem.prix100 / 100).ToString("# ### ### ##0") + " ₭");
                            LVI.SubItems.Add((_getItem.prixPnj / 10).ToString("# ### ### ##0") + " ₭");
                            _LVI.Add(LVI);
                        } else {
                            MessageBox.Show(id + " en quantité " + qty + " n'a pas été trouvé dans la bdd ...");
                        }

                    } catch { }
                }
            }
            Primaire.ListBegin(Primaire.lv_banque);
            Primaire.AddItem(Primaire.lv_banque, _LVI);
            Primaire.ListEnd(Primaire.lv_banque);
        }
        public void ParseInventaire(string inventairePerso) {
            ClearInventaire();
            string[] _splt = inventairePerso.Split(new string[] { "|" }, StringSplitOptions.None)[10].Split(new string[] { ";" }, StringSplitOptions.None);

            List<ListViewItem> _LVI = new List<ListViewItem> { };
            foreach (string _ress in _splt) {
                if (_ress != "") {
                    int id = int.Parse(_ress.Split(new string[] { "~" }, StringSplitOptions.None)[1], System.Globalization.NumberStyles.HexNumber);
                    int qty = int.Parse(_ress.Split(new string[] { "~" }, StringSplitOptions.None)[2], System.Globalization.NumberStyles.HexNumber);

                    Item _getItem = Primaire.getItems(id);
                    if (_getItem != null) {
                        addInventaire(id, qty);

                        ListViewItem LVI = new ListViewItem(id.ToString());
                        LVI.SubItems.Add(_getItem.nom);
                        LVI.SubItems.Add(qty.ToString());
                        _LVI.Add(LVI);
                    } else {
                        MessageBox.Show(id + " en quantité " + qty + " n'a pas été trouvé dans la bdd ...");
                    }
                }
            }

            Primaire.ListBegin(Primaire.lv_inventaire);
            Primaire.AddItem(Primaire.lv_inventaire, _LVI);
            Primaire.ListEnd(Primaire.lv_inventaire);
        }

        public void RefreshCraft() {
            Primaire.lv_craft.Items.Clear();
            Primaire.lv_rentpnj.Items.Clear();
            Primaire.lv_rentvente.Items.Clear();
            SQLiteCommand _cmd = conn.CreateCommand();

            List<Item> itemsBanque = new List<Item>();

            _cmd.CommandText = "SELECT * FROM Banque";
            SQLiteDataReader reader = _cmd.ExecuteReader();
            while (reader.Read()) {
                Item _it = Primaire.getItems((int)reader["id"]);
                _it.qty = (int)reader["qty"];
                itemsBanque.Add(_it);
            }

            _cmd = conn.CreateCommand();
            _cmd.CommandText = "SELECT * FROM Inventaire";
            reader = _cmd.ExecuteReader();
            while (reader.Read()) {
                Item _it = Primaire.getItems((int)reader["id"]);
                _it.qty = (int)reader["qty"];

                int _ind = itemsBanque.IndexOf(itemsBanque.Find(x => x.id == _it.id));
                if (_ind != -1) itemsBanque[_ind].qty += _it.qty; else itemsBanque.Add(_it);
            }

            _cmd = conn.CreateCommand();
            _cmd.CommandText = "SELECT * FROM Crafts";
            reader = _cmd.ExecuteReader();

            List<ListViewItem> _LVI = new List<ListViewItem> { };
            List<ListViewItem> _LVIRentPNJ = new List<ListViewItem> { };
            List<ListViewItem> _LVIRentVente = new List<ListViewItem> { };

            while (reader.Read()) {
                int maxCraft = 999999999;
                bool craftable = true;
                Item result = Primaire.getItems((int)reader["id"]);
                ListViewItem _lv = new ListViewItem(result.id.ToString());
                _lv.SubItems.Add(Primaire.getTypes(result.type));
                _lv.SubItems.Add(result.nom);

                string[] _ress = new string[] { (string)reader["ress1"], (string)reader["ress2"], (string)reader["ress3"], (string)reader["ress4"], (string)reader["ress5"], (string)reader["ress6"], (string)reader["ress7"], (string)reader["ress8"] };

                int coutCraft = 0;
                bool cout = true;
                foreach (string item in _ress) {
                    if (item.Length > 0) {
                        string[] _spl = item.Split(',');
                        string _qty = _spl[0];
                        string _id = _spl[1];
                        Item search = itemsBanque.Find(x => x.id == int.Parse(_id));

                        if (search == null) {
                            craftable = false;
                            break;
                        }
                        int getPrix = 999999999;
                        if (search.prix1 != -1) getPrix = Math.Min(getPrix, search.prix1);
                        if (search.prix10 != -1) getPrix = Math.Min(getPrix, (int)(search.prix10 / 10));
                        if (search.prix100 != -1) getPrix = Math.Min(getPrix, (int)(search.prix100 / 100));
                        if (search.prix1 == -1 && search.prix10 == -1 && search.prix100 == -1) cout = false;

                        if (getPrix != 999999999) coutCraft += (getPrix * int.Parse(_qty));

                        if (search.qty / int.Parse(_qty) >= 1) {
                            maxCraft = Math.Min(maxCraft, (int)(search.qty / int.Parse(_qty)));
                        } else {
                            craftable = false;
                            break;
                        }
                    } else break;
                }

                if (craftable) {
                    _lv.SubItems.Add(maxCraft.ToString());
                    if (_ress[0].Length > 0) _lv.SubItems.Add(_ress[0].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[0].Split(',')[1])).nom); else _lv.SubItems.Add("-");
                    if (_ress[1].Length > 0) _lv.SubItems.Add(_ress[1].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[1].Split(',')[1])).nom); else _lv.SubItems.Add("-");
                    if (_ress[2].Length > 0) _lv.SubItems.Add(_ress[2].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[2].Split(',')[1])).nom); else _lv.SubItems.Add("-");
                    if (_ress[3].Length > 0) _lv.SubItems.Add(_ress[3].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[3].Split(',')[1])).nom); else _lv.SubItems.Add("-");
                    if (_ress[4].Length > 0) _lv.SubItems.Add(_ress[4].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[4].Split(',')[1])).nom); else _lv.SubItems.Add("-");
                    if (_ress[5].Length > 0) _lv.SubItems.Add(_ress[5].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[5].Split(',')[1])).nom); else _lv.SubItems.Add("-");
                    if (_ress[6].Length > 0) _lv.SubItems.Add(_ress[6].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[6].Split(',')[1])).nom); else _lv.SubItems.Add("-");
                    if (_ress[7].Length > 0) _lv.SubItems.Add(_ress[7].Split(',')[0] + " x " + Primaire.getItems(int.Parse(_ress[7].Split(',')[1])).nom); else _lv.SubItems.Add("-");

                    int prixItem = 999999999;
                    if (result.prix1 != -1) prixItem = Math.Min(prixItem, result.prix1);
                    if (result.prix10 != -1) prixItem = Math.Min(prixItem, (int)(result.prix10 / 10));
                    if (result.prix100 != -1) prixItem = Math.Min(prixItem, (int)(result.prix100 / 100));
                    if (coutCraft != 0 && cout) _lv.SubItems.Add(coutCraft.ToString("# ### ### ##0") + " ₭"); else _lv.SubItems.Add("?");
                    if (prixItem != 999999999) _lv.SubItems.Add(prixItem.ToString("# ### ### ##0") + " ₭"); else _lv.SubItems.Add("?");
                    _lv.SubItems.Add((result.prixPnj / 10).ToString("# ### ### ##0") + " ₭");

                    if (prixItem != 999999999 && coutCraft != 0) if (coutCraft < prixItem) _LVIRentVente.Add((ListViewItem)_lv.Clone());
                    if (coutCraft != 0 && (result.prixPnj / 10) >= 1) if (coutCraft < (result.prixPnj / 10)) _LVIRentPNJ.Add((ListViewItem)_lv.Clone());

                    if (prixItem != 999999999 && coutCraft != 0) if (coutCraft < prixItem) _lv.BackColor = Color.MistyRose;
                    if (coutCraft != 0 && (result.prixPnj / 10) >= 1) if (coutCraft < (result.prixPnj / 10)) _lv.BackColor = Color.LightCoral;

                    _LVI.Add(_lv);
                }
            }
            reader.Close();

            Primaire.ListBegin(Primaire.lv_rentpnj);
            Primaire.AddItem(Primaire.lv_rentpnj, _LVIRentPNJ);
            Primaire.setOrder(Primaire.lv_rentpnj, 13);
            Primaire.ListEnd(Primaire.lv_rentpnj);

            Primaire.ListBegin(Primaire.lv_rentvente);
            Primaire.AddItem(Primaire.lv_rentvente, _LVIRentVente);
            Primaire.setOrder(Primaire.lv_rentvente, 12);
            Primaire.ListEnd(Primaire.lv_rentvente);

            Primaire.ListBegin(Primaire.lv_craft);
            Primaire.AddItem(Primaire.lv_craft, _LVI);
            Primaire.setOrder(Primaire.lv_craft, 0, true);
            Primaire.ListEnd(Primaire.lv_craft);
        }

        public bool Verif(string text, string verif) {
            if (text.Length < verif.Length) return false; else return text.Substring(0, verif.Length) == verif;
        }

        public void TraitementRecu() {
            while (true) {
                if (msReceive.Length > 0) {
                    int getPos = msReceive.IndexOf('\0');
                    if (getPos > -1) {
                        string getPacket = msReceive.Substring(0, getPos);
                        msReceive = msReceive.Remove(0, getPos + 1);
                        if (getPacket.Length > 0) {
                            switch (getPacket.Substring(0, 2)) {
                                case "AL":
                                    string[] _splt = getPacket.Substring(3).Split('|');

                                    string _abo = _splt[0];
                                    Primaire.WriteLogs(Primaire.rtb_r_action, SecondToTime(_abo));
                                    string _nbr = _splt[1];
                                    Primaire.WriteLogs(Primaire.rtb_r_action, "Nombre de personnages sur le serveur : " + _nbr + ".\n");
                                    for (int i = 0; i < int.Parse(_nbr); i++) {
                                        Perso perso = new Perso(_splt[i + 2]);
                                        Primaire.WriteLogs(Primaire.rtb_r_action, perso.getPerso());
                                    }
                                    break;
                                case "Re":
                                    if (drago) break; else drago = true;
                                    string[] _splt1 = getPacket.Split(new string[] { ":" }, StringSplitOptions.None);
                                    Primaire.WriteLogs(Primaire.rtb_r_action, Primaire.getDragodindes(int.Parse(_splt1[1]) + 1) + " nom: " + _splt1[4] + " lvl: " + _splt1[7] + "\n");
                                    break;
                                case "AS":
                                    Primaire.ClearLv(Primaire.lv_inventaire);
                                    ParseInventaire(getPacket);
                                    break;
                                case "EL":
                                    if (getPacket == "EL") break;

                                    Primaire.ClearLv(Primaire.lv_banque);
                                    ParseBanque(getPacket);
                                    break;
                                case "EH":
                                    switch (getPacket.Substring(0, 3)) {
                                        case "EHL": // les ressources dispo de cette catégorie
                                            break;
                                        case "EHP": // niveau max de l'hdv visité
                                            break;
                                        case "EHl":
                                            string[] _spl = getPacket.Replace("EHl", "").Split('|');
                                            int id = int.Parse(_spl[0]);

                                            int prix1min = -1, prix10min = -1, prix100min = -1;

                                            for (int i = 1; i < _spl.Length; i++) {
                                                string[] _spl2 = _spl[i].Split(';');

                                                if (_spl2[2].Length > 0 && (prix1min > int.Parse(_spl2[2]) || prix1min == -1)) prix1min = int.Parse(_spl2[2]);
                                                if (_spl2[3].Length > 0 && (prix10min > int.Parse(_spl2[3]) || prix10min == -1)) prix10min = int.Parse(_spl2[3]);
                                                if (_spl2[4].Length > 0 && (prix100min > int.Parse(_spl2[4]) || prix100min == -1)) prix100min = int.Parse(_spl2[4]);
                                            }

                                            Item _item = Primaire.getItems(id);
                                            int prixPnj = (_item.prixPnj+5) / 10;
                                            ListViewItem LVI = new ListViewItem(id.ToString());
                                            LVI.SubItems.Add(_item.lvl);
                                            LVI.SubItems.Add(_item.nom);
                                            if (prix1min > -1) LVI.SubItems.Add(prix1min.ToString("# ### ### ##0") + " ₭"); else LVI.SubItems.Add("-");

                                            if (prix10min > -1) LVI.SubItems.Add(prix10min.ToString("# ### ### ##0") + " ₭"); else LVI.SubItems.Add("-");
                                            if (prix10min > -1) LVI.SubItems.Add(((int)Math.Round((double)(prix10min / 10), MidpointRounding.AwayFromZero)).ToString("# ### ### ##0") + " ₭"); else LVI.SubItems.Add("-");


                                            if (prix100min > -1) LVI.SubItems.Add(prix100min.ToString("# ### ### ##0") + " ₭"); else LVI.SubItems.Add("-");
                                            if (prix100min > -1) LVI.SubItems.Add(((int)Math.Round((double)(prix100min / 100), MidpointRounding.AwayFromZero)).ToString("# ### ### ##0") + " ₭"); else LVI.SubItems.Add("-");

                                            LVI.SubItems.Add(prixPnj.ToString("# ### ### ##0") + " ₭");
                                            if ((prix1min < prixPnj && prix1min != -1) || (prix10min / 10 < prixPnj && prix10min != -1) || (prix100min / 100 < prixPnj && prix100min != -1)) {
                                                Primaire.WriteLogs(Primaire.rtb_r_action, "Item " + _item.nom + " lvl " + _item.lvl + " inférieur au prix rachat du pnj à " + prixPnj + ".\n"); ;
                                                LVI.BackColor = Color.DarkSalmon;
                                            }

                                            int _ind = Primaire.FindLv(Primaire.lv_hdv, id);
                                            if (_ind != -1) {
                                                if (prix1min > -1) Primaire.WriteItem(Primaire.lv_hdv, _ind, 3, prix1min.ToString("# ### ### ##0") + " ₭"); else Primaire.WriteItem(Primaire.lv_hdv, _ind, 3, "-");

                                                if (prix10min > -1) Primaire.WriteItem(Primaire.lv_hdv, _ind, 4, prix10min.ToString("# ### ### ##0") + " ₭"); else Primaire.WriteItem(Primaire.lv_hdv, _ind, 4, "-");
                                                if (prix10min > -1) Primaire.WriteItem(Primaire.lv_hdv, _ind, 5, ((int)Math.Round((double)(prix10min / 10), MidpointRounding.AwayFromZero)).ToString("# ### ### ##0") + " ₭"); else Primaire.WriteItem(Primaire.lv_hdv, _ind, 5, "-");

                                                if (prix100min > -1) Primaire.WriteItem(Primaire.lv_hdv, _ind, 6, prix100min.ToString("# ### ### ##0") + " ₭"); else Primaire.WriteItem(Primaire.lv_hdv, _ind, 6, "-");
                                                if (prix100min > -1) Primaire.WriteItem(Primaire.lv_hdv, _ind, 7, ((int)Math.Round((double)(prix100min / 100), MidpointRounding.AwayFromZero)).ToString("# ### ### ##0") + " ₭"); else Primaire.WriteItem(Primaire.lv_hdv, _ind, 7, "-");
                                            } else {
                                                Primaire.ListBegin(Primaire.lv_hdv);
                                                Primaire.AddItem(Primaire.lv_hdv, LVI);
                                                Primaire.ListEnd(Primaire.lv_hdv);
                                            }


                                            addHDVItem(id, prix1min, prix10min, prix100min);
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case "Em":
                                    switch (getPacket.Substring(0, 3)) {
                                        case "EmK":

                                            break;
                                        default:
                                            string[] _spl3 = getPacket.Split('|');
                                            int qty = int.Parse(_spl3[1]);
                                            int id2 = int.Parse(_spl3[2]);

                                            int _ind2 = Primaire.FindLv(Primaire.lv_inventaire, id2);
                                            Primaire.WriteItem(Primaire.lv_inventaire, _ind2, 2, (Primaire.getItems(id2).qty - qty).ToString());

                                            addItems(id2, qty: (Primaire.getItems(id2).qty - qty));
                                            break;
                                    }
                                    break;
                                case "IO":
                                    break;
                                case "GA":
                                    break;
                                case "GD":
                                    break;
                                case "GM":
                                    break;
                                case "Bp":
                                    break;
                                case "BN":
                                    break;
                                case "TT":
                                    break;
                                case "BZ":
                                    break;
                                case "BT":
                                    break;
                                case "hP":
                                    break;
                                case "Rp":
                                    break;
                                case "EC":
                                    break;
                                case "EM": // K ?
                                    break;
                                case "OQ":
                                    break;
                                case "OR":
                                    break;
                                case "Ow":
                                    break;
                                case "Im":
                                    break;
                                case "OA":
                                    break;
                                case "xC":
                                    break;
                                case "Ec":
                                    break;
                                case "JX":
                                    break;
                                case "Oa":
                                    break;
                                case "DC": // K ?
                                    break;
                                case "DQ":
                                    break;
                                case "As":
                                    break;
                                case "DV":
                                    break;
                                case "BD":
                                    break;
                                case "cM": // K = msg guilde
                                    break;
                                case "Es":
                                    break;
                                case "EV":
                                    break;
                                case "OM":
                                    break;
                                case "OS":
                                    break;
                                case "OT":
                                    break;
                                case "EW":
                                    break;
                                case "Ew":
                                    break;
                                case "eU":
                                    break;
                                case "fC":
                                    break;
                                case "ES": // K ?
                                    break;
                                case "Wc":
                                    break;
                                case "Wv":
                                    break;
                                case "EA":
                                    break;
                                case "Ea":
                                    break;
                                case "Ga":
                                    break;
                                case "EB": // K ?
                                    break;
                                case "ER":
                                    break;
                                case "EK":
                                    break;
                                case "Er":
                                    break;
                                default:
                                    string autre = getPacket;
                                    break;
                            }

                            Primaire.WriteLogs(Primaire.rtb_r_brute, getPacket + "\n");
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        public void TraitementEnvoie() {
            while (true) {
                if (msWrite.Length > 0) {
                    int getPos = msWrite.IndexOf('\0');
                    if (getPos > -1) {
                        string getPacket = msWrite.Substring(0, getPos);
                        msWrite = msWrite.Remove(0, getPos + 1);

                        Primaire.WriteLogs(Primaire.rtb_e_brute, getPacket + "\n");
                        // Ici on traite les envoies, flemme avec le nouveau système de décoder ...
                    }
                }
                Thread.Sleep(100);
            }
        }

        public string SecondToTime(string seconds) {
            DateTime temps = DateTime.Now.AddMilliseconds(double.Parse(seconds));

            return "Fin d'abonnement le : " + temps.Day.ToString("00") + "/" + temps.Month.ToString("00") + "/" + temps.Year + " à " + temps.Hour.ToString("00") + ":" + temps.Minute.ToString("00") + ":" + temps.Second.ToString("00") + ".\n";
        }

        void addHDVItem(int id, int _prix1 = -1, int _prix10 = -1, int _prix100 = -1) {
            addItems(id, prix1: _prix1, prix10: _prix10, prix100: _prix100, cout: Primaire.getCraft(id));
        }
        void addItems(int id, string nom = "", string desc = "", int lvl = 0, int prixPnj = 0, int prix1 = -1, int prix10 = -1, int prix100 = -1, int qty = -1, int cout = -1) {
            SQLiteCommand _cmd = conn.CreateCommand();
            _cmd.CommandText = "SELECT count(*) FROM Items WHERE \"id\"=" + id;
            int nbr = Convert.ToInt32(_cmd.ExecuteScalar());

            if (nbr == 1) {
                if (prix1 < 1 && prix10 < 1 && prix100 < 1 && qty == -1) return;
                List<string> _edits = new List<string>();

                if (prix1 > 0) _edits.Add(" \"prix1\" = " + prix1);
                if (prix10 > 0) _edits.Add(" \"prix10\" = " + prix10);
                if (prix100 > 0) _edits.Add(" \"prix100\" = " + prix100);
                if (qty != -1) _edits.Add(" \"qty\" = " + qty);

                _cmd = conn.CreateCommand();
                _cmd.CommandText = "UPDATE \"Items\" SET";
                for (int i = 0; i < _edits.Count; i++) {
                    _cmd.CommandText += _edits[i];
                    if (i < _edits.Count - 1) _cmd.CommandText += ",";
                }
                _cmd.CommandText += " WHERE \"id\" = " + id + ";";
                _cmd.ExecuteNonQuery();
            } else {
                MessageBox.Show("erreur ?");
                //_cmd.CommandText = "INSERT INTO \"Items\" VALUES (" + id + "," + lvl + ",\"" + nom + "\",\"" + desc + "\"," + prix1 + "," + prix10 + "," + prix100 + "," + prixPnj + "," + qty + ");";
                //_cmd.ExecuteNonQuery();
            }
        }
        void addCrafts(int id, int qty) {
            addItems(id, qty: qty);
        }
        void addAllCrafts(int id, params string[] ress) {
            string _req = "";
            for (int i = 0; i < ress.Length; i++) {
                if (i > 0) _req += ",";
                _req += "\"" + ress[i] + "\"";
            }

            for (int j = 8 - ress.Length; j > 0; j--) _req += ",\"\"";

            try {
                SQLiteCommand _cmd = conn.CreateCommand();
                _cmd.CommandText = "INSERT INTO \"Crafts\" VALUES (" + id + "," + _req + ");";
                _cmd.ExecuteNonQuery();
            } catch { }
        }
        void addBanque(int id, int qty) {
            SQLiteCommand _cmd = conn.CreateCommand();
            _cmd.CommandText = "SELECT count(*) FROM Banque WHERE \"id\"=" + id;
            int nbr = Convert.ToInt32(_cmd.ExecuteScalar());

            if (nbr == 1) {
                _cmd = conn.CreateCommand();
                _cmd.CommandText = "UPDATE Banque SET \"qty\" = " + qty + " WHERE \"id\" = " + id + ";";
                _cmd.ExecuteNonQuery();
            } else {
                _cmd.CommandText = "INSERT INTO Banque VALUES (" + id + "," + qty + ");";
                _cmd.ExecuteNonQuery();
            }
        }
        void addInventaire(int id, int qty) {
            SQLiteCommand _cmd = conn.CreateCommand();
            _cmd.CommandText = "SELECT count(*) FROM Inventaire WHERE \"id\"=" + id;
            int nbr = Convert.ToInt32(_cmd.ExecuteScalar());

            if (nbr == 1) {
                _cmd = conn.CreateCommand();
                _cmd.CommandText = "UPDATE Inventaire SET \"qty\" = " + qty + " WHERE \"id\" = " + id + ";";
                _cmd.ExecuteNonQuery();
            } else {
                _cmd.CommandText = "INSERT INTO Inventaire VALUES (" + id + "," + qty + ");";
                _cmd.ExecuteNonQuery();
            }
        }

        void ClearBanque() {
            SQLiteCommand _cmd = conn.CreateCommand();
            _cmd.CommandText = "DELETE FROM `Banque`";
            _cmd.ExecuteNonQuery();
        }
        void ClearInventaire() {
            SQLiteCommand _cmd = conn.CreateCommand();
            _cmd.CommandText = "DELETE FROM `Inventaire`";
            _cmd.ExecuteNonQuery();
        }
    }
}
