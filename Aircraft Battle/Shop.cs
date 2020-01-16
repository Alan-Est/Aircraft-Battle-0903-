using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aircraft_Battle
{
    public partial class Shop : Form
    {
        public Shop()
        {
            InitializeComponent();
        }
        List<Image> IL = new List<Image>();
        PictureBox[] PB = new PictureBox[3];
        Button[] Btn = new Button[3];
        ToolTip[] TT = new ToolTip[3];
        private void Shop_Load(object sender, EventArgs e)
        {
            PB[0] = pictureBox1;
            PB[1] = pictureBox2;
            PB[2] = pictureBox3;
            Btn[0] = button1;
            Btn[1] = button2;
            Btn[2] = button3;
            IL.Add(Image.FromFile(Path.Shop_Boom));
            IL.Add(Image.FromFile(Path.Shop_Light));
            IL.Add(Image.FromFile(Path.Shop_HP));
            string[] Title = { "额外的炸弹", "武器升级", "装甲强化" };
            string[] Str = { "开局炸弹数量+1", "武器升级为激光武器", "生命值上限+1" };
            for(int i = 0;i<TT.Count();i++)
            {
                PB[i].SizeMode = PictureBoxSizeMode.Zoom;
                PB[i].Image = IL[i];
                TT[i] = new ToolTip();
                TT[i].ToolTipTitle = Title[i];
                TT[i].SetToolTip(PB[i], Str[i]);
                Btn[i].Text = "购买";
            }
        }

        private void Item_Buy(object sender, EventArgs e)
        {
            if ((Button)sender == Btn[0])
            {
                //购买了炸弹+1
            }
            else if ((Button)sender == Btn[1])
            {
                //购买了激光武器
            }
            else if ((Button)sender == Btn[2])
            {
                //购买了生命值+1
            }

        }

    }
    /// <summary>
    /// 物品购买记录 用于交互
    /// </summary>
    public static class BuyLog
    {
        public static bool Boom = false;
        public static bool Light = false;
        public static bool HP = false;
    }
}
