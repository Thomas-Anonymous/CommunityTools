using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Sniffeur {
    public class Items {
        public int id = -1;
        public string nom = "";
        public string description = "";
        public int qty = 0;

        public Items(int _id, string _nom, string _desc) {
            id = _id;
            qty = 0;
            description = _desc;
            nom = _nom;
        }
        public Items(int _id, string _nom, string _desc, int _qty) {
            id = _id;
            qty = _qty;
            description = _desc;
            nom = _nom;
        }
    }
}
