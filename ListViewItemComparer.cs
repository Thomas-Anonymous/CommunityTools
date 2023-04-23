using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommunityTools {
    internal class ListViewItemComparer : IComparer {

        private static int column = -1;
        public static SortOrder Order = SortOrder.Ascending;

        public ListViewItemComparer(int columnIndex) {
            if (column == columnIndex && Order == SortOrder.Ascending) {
                Order = SortOrder.Descending;
            } else if (column == columnIndex) {
                Order = SortOrder.Ascending;
            }
            column = columnIndex;
        }

        public int Compare(object x, object y) {
            if (!(x is ListViewItem))
                return (0);
            if (!(y is ListViewItem))
                return (0);

            ListViewItem l1 = (ListViewItem)x;
            ListViewItem l2 = (ListViewItem)y;

            if (int.TryParse(l1.SubItems[column].Text.Replace(" ₭", "").Replace(" ", "").Replace("?","0"), out int number)) {
                float fl1 = float.Parse(l1.SubItems[column].Text.Replace(" ₭", "").Replace(" ", "").Replace("?", "0"));
                float fl2 = float.Parse(l2.SubItems[column].Text.Replace(" ₭", "").Replace(" ", "").Replace("?", "0"));

                if (Order == SortOrder.Ascending) {
                    return fl1.CompareTo(fl2);
                } else {
                    return fl2.CompareTo(fl1);
                }
            } else {
                string str1 = l1.SubItems[column].Text;
                string str2 = l2.SubItems[column].Text;

                if (Order == SortOrder.Ascending) {
                    return str1.CompareTo(str2);
                } else {
                    return str2.CompareTo(str1);
                }
            }
        }
    }
}
