using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace USBSpeedTest
{
    public partial class ADForm : Form
    {

        public ADForm()
        {
            InitializeComponent();
        }

        public void Init_Table()
        {

            dataGridView1.DataSource = Data.dt_AD01;
            dataGridView1.AllowUserToAddRows = false;


            dataGridView2.DataSource = Data.dt_AD02;
            dataGridView2.AllowUserToAddRows = false;
        }

        private void ADForm_Load(object sender, EventArgs e)
        {
            Data.AdFrmIsAlive = true;
            Init_Table();
        }

        private void ADForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Data.AdFrmIsAlive = false;
            this.Dispose();
        }
    }
}
