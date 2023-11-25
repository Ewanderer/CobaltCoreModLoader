using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoMod
{
    public partial class DemoAddinPanel : UserControl
    {
        public DemoAddinPanel()
        {
            InitializeComponent();
        }

        private void btnGenerateRandom_Click(object sender, EventArgs e)
        {
            tbValue.Text = RandomString(10);
        }

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
