using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommunityTools {
    public partial class load : Form {
        public Point downPoint = Point.Empty;

        public Label lbl2;

        public load() {
            InitializeComponent();
            lbl2 = label2;

            MouseDown += new MouseEventHandler(load_MouseDown);
            MouseMove += new MouseEventHandler(load_MouseMove);
            MouseUp += new MouseEventHandler(load_MouseUp);
        }

        private void load_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                downPoint = Point.Empty;
        }

        private void load_MouseMove(object sender, MouseEventArgs e) {
            if (downPoint != Point.Empty)
                Location = new Point(Left + e.X - downPoint.X, Top + e.Y);
        }

        private void load_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                downPoint = new Point(e.X, e.Y);
        }

        public void loadUpdate(string text) {
            if (lbl2.InvokeRequired) {
                MethodInvoker invoker = delegate { loadUpdate(text); };
                lbl2.Invoke(invoker);
                return;
            }

            lbl2.Text = text;
        }

        public void fermer() {
            if (InvokeRequired) {
                MethodInvoker invoker = delegate { fermer(); };
                Invoke(invoker);
                return;
            }

            Close();
        }
    }
}
