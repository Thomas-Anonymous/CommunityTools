using System.Collections.Generic;

namespace Test_Sniffeur {
    public class Crafts {
        public int resultat = 0;
        public List<int[]> items = new List<int[]> { };

        public Crafts(int _resu, List<int[]> _it1) {
            resultat = _resu;
            items = _it1;
        }
    }
}
