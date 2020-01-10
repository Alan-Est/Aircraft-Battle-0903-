using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aircraft_Battle
{
    public partial class UpdateBox : Form
    {
        public UpdateBox()
        {
            InitializeComponent();
        }

        private void UpdateBox_Load(object sender, EventArgs e)
        {
            //读取更新内容记事本显示文字
            try
            {
                string str2 = File.ReadAllText(Application.StartupPath.ToString() + "\\" + "更新日志.txt", Encoding.UTF8);
                richTextBox1.Text = str2;

                

                FileStream fS = new FileStream(Application.StartupPath.ToString() + "\\" + "更新日志.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
                StreamWriter sw = new StreamWriter(fS);
                sw.WriteLine("");
            }
            catch
            {
                this.Close();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
