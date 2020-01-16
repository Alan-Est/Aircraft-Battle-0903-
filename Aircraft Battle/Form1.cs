using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.Media;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.DirectX.DirectSound;
using Microsoft.DirectX;
using Microsoft.VisualBasic;
using SharpDX.Direct3D;
/*
       -----制作相关-----------
 * 1.飞机的移动模块
 * 2.飞机的子弹发射
 * 3.中心绘制模块
 * 4.子弹碰撞
 * 5.敌机来袭
 * 6.飞机出界判断
 * 7.自适应屏幕大小，改变敌机出现范围
 * 8.飞机间碰撞
 * 9.飞机击毁特效
 * 10.飞机游戏音效【准备中】 需要内容：玩家射进音效 Boss预警音效 Boss 攻击音效
 * 11.背景
 * 12.关卡设计【准备中】 分开模块 【关卡模式 & 无尽模式】
 * 13.新的飞机种类
 * 14.背景乐
 * 15.炸弹
 * 16.boss 在60秒时出现 之后每隔120s 刷新
 * 17.暂停界面
 * 18.开始界面
 * 19.结算界面
 * 20.新的武器类型-Light 激光
 * 21.更换新的贴图？包装游戏？ 【准备中】
 * 22.商店界面【待】
 * 23.服务器更新
 * 24.更新内容显示
 * 25.联机[Socket]模块 & 用户登录模块【待】
 * 
 */
/* 已知问题：
 * 
 * 1 BOSS攻击采用匿线程等待缺少了一个BOSS已经死亡的判断.【已修正】
 *   
 * 
 */
namespace Aircraft_Battle
{
    public partial class Form1 : Form
    {
        static public bool IsDebuging = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }
        
        #region 变量区块
        static public bool GameOver = false;
        static public Form1 Form;
        static public int BossID;
        static public bool DB = false;
        static public bool GameIsStop = false;
        public int IT_Count = 0;
        /// <summary>
        /// 0:暂停，1：游戏，2：开始界面
        /// </summary>
        static public int Canves;
        /// <summary>
        /// 玩家点击开始游戏之后，启用游戏绘图器
        /// </summary>
        static public bool OnGame = true;
        //static public Image screenImage;
        /// <summary>
        /// 背景乐播放器
        /// </summary>
        SoundPlayer splayer;
        string GameMusic = Application.StartupPath.ToString() + "\\" + "Resources\\Sound\\1.wav";
        //Device dv;
        public class MP3Player
        {
            /// <summary>   
            /// 文件地址   
            /// </summary>   
            public string FilePath;

            /// <summary>   
            /// 播放   
            /// </summary>   
            public void Play()
            {
                mciSendString("close all", "", 0, 0);
                mciSendString("open " + FilePath + " alias media", "", 0, 0);
                mciSendString("play media", "", 0, 0);
            }

            /// <summary>   
            /// 暂停   
            /// </summary>   
            public void Pause()
            {
                mciSendString("pause media", "", 0, 0);
            }

            /// <summary>   
            /// 停止   
            /// </summary>   
            public void Stop()
            {
                mciSendString("close media", "", 0, 0);
            }

            /// <summary>   
            /// API函数   
            /// </summary>   
            [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
            private static extern int mciSendString(
             string lpstrCommand,
             string lpstrReturnString,
             int uReturnLength,
             int hwndCallback
            );
        }
        /// <summary>
        /// 公共随机数发生器
        /// </summary>
        static readonly Random rand = new Random();
        /// <summary>
        /// 飞机集合
        /// </summary>
        static public readonly List<Plane> PlaneFlakes = new List<Plane>();
        /// <summary>
        /// 子弹集合
        /// </summary>
        static public readonly List<Bullet> BulletFlakes = new List<Bullet>();
        /// <summary>
        /// 敌人飞机集合
        /// </summary>
        static public readonly List<Enemy> EnemyFlakes = new List<Enemy>();
        /// <summary>
        /// 特效层
        /// </summary>
        static public readonly List<Effect> EffectFlakes = new List<Effect>();
        /// <summary>
        /// 激光
        /// </summary>
        static public readonly List<Light> LightFlakes = new List<Light>();
        static public readonly List<LightII> LightIIFlakes = new List<LightII>();
        static public int Tick = 0;
        /// <summary>
        /// UI路径类
        /// </summary>
        static public class UI
        {
            /// <summary>
            /// 血条
            /// </summary>
            static public Image hpimage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\hpbar.png");
            /// <summary>
            /// 血条框
            /// </summary>
            static public Image hpimage2 = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\hp.png");
            /// <summary>
            /// 设置菜单
            /// </summary>
            static public Image setmeum;
            /// <summary>
            /// 炸弹贴图
            /// </summary>
            static public Image Boom = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\boom.png");
            /// <summary>
            /// 背景贴图
            /// </summary>
            static public Image BG = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\backGround.png");
            static public Image gold = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\gold.png");
            static public Image Score = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\Score.png");
            static public Image BackG = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\bg.png");
            static public Image PlayBtn = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\btn_play_2.png");
            static public string PlayBtnPath = Application.StartupPath.ToString() + "\\" + "Resources\\UI\\btn_play_2.png";
            static public Image ShopBtn = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\otherBtn_2.png");
            static public string ShopBtnPath = Application.StartupPath.ToString() + "\\" + "Resources\\UI\\otherBtn_2.png";
            static public Image SoundImage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\soundoff.png");
            static public Image Stop = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\Stop.png");
            static public Image Mouse = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\mouseon.png");
            static public float BGX = 0;
            static public float BGY = 0;
            static public float BGY2 = 600;
        }
        /// <summary>
        /// 玩家的飞机对象
        /// </summary>
        public class Plane
        {
            //改用speed+reg 方式计算位置
            public float Rotation = 0;
            public float RotVelocity;
            public float Scale;//大小
            public float X;
            public float Y;
            public Image image;//飞机贴图
            public double reg;//角度
            public short Size = 72;//碰撞范围 圆形
            public int hp = 3;//生命值
            public int maxhp = 3;//最大生命值
            public int shield = 0;//护盾
            public int attack = 1;//攻击力
            public int lv = 1;//等级
            public float speed = 0;//当前移动速度
            public float speedbase = 15;//基础移动速度
            public int Attack_interval = 5;//采用计数式间隔即几个游戏时钟周期
            public int AI_Count = 0;//攻击间隔计数
            public byte BulletID = 1;//子弹贴图ID
            public float deviation = 20;
            public int WeaponID = 1;//
            public bool Invincible = false;//是否无敌
            public float MoveX = 10;//偏移X
            public float MoveY = 10;//偏移Y
            public float BulletMoveX = 10;//子弹偏移
            public float BulletMoveY = 10;//子弹偏移
            public float ImageMoveCount = 0;
        }
        /// <summary>
        /// 飞机的子弹对象
        /// </summary>
        public class Bullet
        {
            //改用speed+reg 方式计算位置
            public float Rotation;
            public float RotVelocity;
            public float Scale = 1;
            public float X;
            public float Y;
            public Image image;//子弹贴图
            public double reg = 90;//角度
            public Color cor;
            public byte Mode = 1;//动画模式
            public short Size = 20;//碰撞范围 圆形
            public float speed = 25;//子弹速度
            public int damage = 1;//子弹伤害
            /// <summary>
            /// 0为敌人的子弹，1为玩家的子弹，默认为0
            /// </summary>
            public int ownning = 0;//子弹所属 
            public float deviation = 10;
            public float MoveX = 10;
            public float MoveY = 10;
            public int hp = 1;//子弹生命值
            public bool speedup = false;//是否拥有加速度
            public float upspeed = 0;//加速度值
        }
        /// <summary>
        /// 敌人的飞机对象
        /// </summary>
        public class Enemy
        {
            //改用speed+reg 方式计算位置
            public float Rotation = 0;
            public float RotVelocity;
            public float Scale;
            public float X;
            public float Y;
            public Image image;//飞机贴图
            public double reg = 90;//角度
            public Color cor;
            public byte Mode;//动画模式
            public short Size = 44;//碰撞范围 圆形
            public int hp;//= 3;//生命值
            public int maxhp = 3;//最大生命值
            public int shield;//护盾
            public int attack = 1;// = 1;//攻击力
            public int lv = 1;//等级
            public float speed;// = 5;//当前移动速度
            public float speedbase = 5;//基础移动速度
            public int Attack_interval = 40;//采用计数式间隔即几个游戏时钟周期
            public int AI_Count = 0;//攻击间隔计数
            /// <summary>
            /// 子弹贴图ID
            /// </summary>
            public byte BulletID = 0;
            public float deviation = 22;
            public int WeaponID = 0;
            public int score = 1;
            public float MoveX = 22;
            public float MoveY = 22;
            public bool IsBoss = false;
            public float BulletMoveX = 10;
            public float BulletMoveY = 10;
            public int Weapon2 = 999;
            public int Attack_interval2 = 999;//攻击方式2的攻击间隔采用计数式间隔即几个游戏时钟周期
            public int weapon3 = 999;
            public int Attack_interval3 = 999;//攻击方式3的攻击间隔采用计数式间隔即几个游戏时钟周期
        }

        /*public class Weapon
        {
            public int AttackID = 1;
        }*/
        public class Effect
        {
            public float Rotation;
            public float RotVelocity;
            public float Scale;
            public float X;
            public float XVelocity;
            public float Y;
            public float YVelocity;
            public byte actID = 1;
            public double reg = 90;//角度
            public int speed = 0;// = 5;//当前移动速度
            public int imageID = 1;
            public Image image;
            public int lifetime;
            public Color cor;
            public byte Mode = 1;
            public int lifemax = 25;
            public byte brushtime = 5;
            public int size = 64;
            public float MoveX = 32;
            public float MoveY = 32;

        }
        /// <summary>
        /// 激光-类
        /// </summary>
        public class Light
        {
            //改用speed+reg 方式计算位置
            public float Rotation;
            public float RotVelocity;
            public float Scale = (float)0.5;
            public float X;
            public float Y;
            public Image image;//激光贴图
            public int id = 1;
            public int imgcount = 2;
            public double reg = 90;//角度
            public Color cor;
            public byte Mode = 1;//动画模式
            public short Size = 5;//粗细
            public int damage = 1;//激光伤害
            /// <summary>
            /// 0为敌人的子弹，1为玩家的子弹，默认为0
            /// </summary>
            public int ownning = 0;//子弹所属 
            public float MoveX = 0;
            public float MoveY = 600;
            public int hp = 1;
            public int lifemax = 25;
            public int life = 0;
            public byte brushtime = 5;
            public int imageID = 1;
            public int count = 0;
            public int hurttime = 10;
            public int unitid = 0;
        }
        public class LightII
        {
            //改用speed+reg 方式计算位置
            public float Rotation;
            public float RotVelocity;
            public float Scale = (float)0.5;
            public float X;
            public float Y;
            public Image image;//激光贴图
            public int id = 1;
            public int imgcount = 2;
            public double reg = 90;//角度
            public Color cor;
            public byte Mode = 1;//动画模式
            public short Size = 5;//粗细
            public int damage = 1;//激光伤害
            /// <summary>
            /// 0为敌人的子弹，1为玩家的子弹，默认为0
            /// </summary>
            public int ownning = 0;//子弹所属 
            public float MoveX = 0;
            public float MoveY = 600;
            public int hp = 1;
            public int lifemax = 25;
            public int life = 0;
            public byte brushtime = 5;
            public int imageID = 1;
            public int count = 0;
            public int hurttime = 10;
            public int unitid = 0;
        }
        /// <summary>
        /// 玩家类
        /// </summary>
        static public class Player
        {
            static public int lv = 1;
            static public int PlaneID = 1;
            /// <summary>
            /// 1:Up 2:Left 3:Down 4:Right
            /// </summary>
            static public bool[] KeyIsDown = new bool[6];
            static public int Score = 0;
            static public int BoomCount = 3;
            static public int Exp = 0;
            static public int Gold = 0;
            static public int MaxScore = 0;
        }
        static public class Game
        {
            /// <summary>
            /// 难度系数 得分等于 Player.Score * 0.5+(0.5*Game.Difficult)向下取整;
            /// </summary>
            static public int Difficult = 1;
            static public bool Music = false;
            static public float MouseX;
            static public float MouseY;
            static public bool UseMouse = true;

        }
        public bool P = true;
        private int EnCount = 0;
        #endregion
        
        #region Other
        /// <summary>
        /// 播放一个波形 -DirectSound
        /// </summary>
        /// <param name="path"></param>
        public void SoundPlay(string path)
            {
                if(Game.Music)
                try
                {
                    Device dv = new Device();
                    dv.SetCooperativeLevel(this, CooperativeLevel.Priority);
                    BufferDescription buffer = new BufferDescription();
                    buffer.GlobalFocus = true;
                    buffer.ControlVolume = true;
                    buffer.ControlPan = true;
                    SecondaryBuffer buf = new SecondaryBuffer(path, buffer, dv);
                    buf.Play(0, BufferPlayFlags.Default);
                }
                catch(SoundException se)
                {
                    MessageBox.Show(se.ToString());
                }
            }
        /// <summary>
        ///重新开始游戏，清除分数，回复HP,重置所有状态 移除所有对象
        /// </summary>
        public void GameStart()
        {
            timer1.Enabled = true;
            timer2.Enabled = true;
            timer3.Enabled = true;
            timer3.Interval = 60000;
            timer1.Start();
            timer2.Start();
            timer3.Start();
            GameCenterTimer.Enabled = true;
            PlaneFlakes[0].hp = 3;
            PlaneFlakes[0].maxhp = 3;
            if (Player.MaxScore > 400)
            {
                PlaneFlakes[0].WeaponID = 201;
            }
            else
            {
                PlaneFlakes[0].WeaponID = 1;
            }
            Player.Score = 0;
            Player.KeyIsDown[1] = false;
            Player.KeyIsDown[2] = false;
            Player.KeyIsDown[3] = false;
            Player.KeyIsDown[4] = false;
            Player.KeyIsDown[5] = false;
            for (int bb = 0; bb < EnemyFlakes.Count; bb++)
            {
                EnemyFlakes[bb].X = -32;
                EnemyFlakes[bb].Y = -32;
            }
            for (int i = 0; i < BulletFlakes.Count; i++)
            {
                if (BulletFlakes[i].ownning != 1)
                {
                    BulletFlakes[i].hp -= 1;
                }
            }
            PlaneFlakes[0].Invincible = true;
            Player.BoomCount = 3;

            //player.Play(); //启用新线程播放
        }
        /// <summary>
        /// true 则暂停游戏 false继续游戏
        /// </summary>
        /// <param name="b"></param>
        public void GameStop(bool b)
        {
            if (b == true)
            {
                //游戏暂停
                timer1.Enabled = false;
                timer2.Enabled = false;
                timer3.Enabled = false;
                timer3.Stop();
                timer2.Stop();
                timer1.Stop();
                if (Canves == 1)
                {
                    Canves = 0;
                }
                //GameCenterTimer.Enabled = false;
            }
            else
            {
                //游戏继续
                timer1.Start();
                timer2.Start();
                timer3.Start();
                timer1.Enabled = true;
                timer2.Enabled = true;
                timer3.Enabled = true;
                if (Canves == 0)
                {
                    Canves = 1;
                }
                //GameCenterTimer.Enabled = true;
            }
        }
        public void GamePaint(object sender, EventArgs e)
        {
            Invalidate();
        }
        /// <summary>
        /// 创建特效
        /// </summary>
        /// <param name="effID">特效ID</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void CreateEffect(int effID, float x, float y)
        {
            if (effID == 1)
            {
                Effect s = new Effect();
                Random rd = new Random();
                //Trace.WriteLine(x.ToString());
                s.actID = 1;
                s.imageID = 1;
                s.X = x;
                s.Y = y;
                //s.reg = 90;
                //s.speed = 3;
                s.Scale = 1;
                s.lifetime = 0;
                s.lifemax = 25;
                s.brushtime = 5;
                s.size = 64;
                s.MoveX = 32;
                s.MoveY = 32;
                try
                {
                    s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Effect\\E" + effID+ "\\1.png");
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                    return;
                }
                EffectFlakes.Add(s);
            }
            if (effID == 2)
            {
                Effect s = new Effect();
                Random rd = new Random();
                //Trace.WriteLine(x.ToString());
                s.actID = Convert.ToByte(effID);
                s.imageID = 1;
                s.X = x;
                s.Y = y;
                //s.reg = 90;
                //s.speed = 3;
                s.Scale = (float)1;
                s.lifetime = 0;
                s.lifemax = 11;
                s.brushtime = 2;
                s.size = 24;
                s.MoveX = 12;
                s.MoveY = -6;
                try
                {
                    s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Effect\\E" + effID + "\\1.png");
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                    return;
                }
                EffectFlakes.Add(s);
            }
            if (effID == 3)
            {
                Effect s = new Effect();
                Random rd = new Random();
                //Trace.WriteLine(x.ToString());
                s.actID = Convert.ToByte(effID);
                s.imageID = 1;
                s.X = x;
                s.Y = y;
                //s.reg = 90;
                //s.speed = 3;
                s.Scale = (float)1;
                s.lifetime = 0;
                s.lifemax = 14;
                s.brushtime = 3;
                s.size = 0;
                s.MoveX = 5;
                s.MoveY = 5;
                s.Scale = 5;
                try
                {
                    s.image = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Effect\\E" + effID + "\\1.png");
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                    return;
                }
                EffectFlakes.Add(s);
            }
        }
        /// <summary>
        /// 1：一架破飞机 攻击1 生命值2 射速40 弹速 快
        /// 2：散弹 攻击力1 生命值1 射速40 弹速 慢
        /// 3: 环形 攻击力1 生命值5 射速50 弹速 很慢 
        /// 4: 十字 攻击力1 生命值3 射速50 弹速 很慢
        /// 5: 自瞄？
        /// 6: 一个BOSS？
        /// 7: 特殊-过场特殊单位，拥有ID为2-5随机的攻击方式， 低间隔的攻击与较高的移动速度
        /// </summary>
        /// <param name="ID"></param>
        public void CreateEnemy(int ID)
        {
            int EnemyID = ID;
            try
            {
                if (ID == 1)
                {
                    Enemy s = new Enemy();
                    Random rd = new Random();
                    float x = (float)rd.Next(s.Size, this.Width - s.Size);
                    //Trace.WriteLine(x.ToString());
                    s.X = x;
                    s.Y = 32;
                    s.reg = 90;
                    s.hp = 1;
                    s.speed = 3;
                    s.BulletID = 7;
                    s.Scale = 1;
                    s.attack = 1;
                    s.WeaponID = 0;
                    s.score = 1;
                    s.BulletMoveX = -8;
                    s.BulletMoveY = 2;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Enemy\\" + EnemyID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                        return;
                    }
                    EnemyFlakes.Add(s);
                }
                if (ID == 2)
                {
                    Enemy s = new Enemy();
                    Random rd = new Random();
                    float x = (float)rd.Next(s.Size, this.Width - s.Size);
                    //Trace.WriteLine(x.ToString());
                    s.X = x;
                    s.Y = 32;
                    s.reg = 90;
                    s.hp = 1;
                    s.speed = 2;
                    s.BulletID = 10;
                    s.Scale = 1;
                    s.attack = 1;
                    s.WeaponID = 2;
                    s.score = 2;
                    s.Attack_interval = 40;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Enemy\\" + EnemyID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                        return;
                    }
                    EnemyFlakes.Add(s);
                }
                if (ID == 3)
                {
                    Enemy s = new Enemy();
                    Random rd = new Random();
                    float x = (float)rd.Next(s.Size, this.Width - s.Size);
                    //Trace.WriteLine(x.ToString());
                    s.X = x;
                    s.Y = 32;
                    s.reg = 90;
                    s.hp = 3;
                    s.speed = (float)0.75;
                    s.BulletID = 11;
                    s.Scale = 1;
                    s.attack = 1;
                    s.WeaponID = 3;
                    s.score = 5;
                    s.Attack_interval = 60;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Enemy\\" + EnemyID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                        return;
                    }
                    EnemyFlakes.Add(s);
                } 
                if (ID == 4)
                {
                    Enemy s = new Enemy();
                    Random rd = new Random();
                    float x = (float)rd.Next(s.Size, this.Width - s.Size);
                    //Trace.WriteLine(x.ToString());
                    s.X = x;
                    s.Y = 32;
                    s.reg = 90;
                    s.hp = 3;
                    s.speed = (float)1.5;
                    s.BulletID = 9;
                    s.Scale = 1;
                    s.attack = 1;
                    s.WeaponID = 4;
                    s.score = 3;
                    s.Attack_interval = 50;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Enemy\\" + EnemyID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                        return;
                    }
                    EnemyFlakes.Add(s);
                }
                if (ID == 5)
                {
                    Enemy s = new Enemy();
                    Random rd = new Random();
                    float x = (float)rd.Next(s.Size, this.Width - s.Size);
                    //Trace.WriteLine(x.ToString());
                    s.X = x;
                    s.Y = 32;
                    s.reg = 90;
                    s.hp = 1;
                    s.speed = (float)3;
                    s.BulletID = 8;
                    s.Scale = 1;
                    s.attack = 1;
                    s.WeaponID =5;
                    s.score = 2;
                    s.Attack_interval = 35;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Enemy\\" + EnemyID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                        return;
                    }
                    EnemyFlakes.Add(s);
                }
                if (ID == 6)
                {
                    DB = true;

                    Enemy s = new Enemy();
                    Random rd = new Random();
                    float x = (float)this.Width / 2;
                    //Trace.WriteLine(x.ToString());
                    s.X = x;
                    s.Y = 32;
                    s.reg = 90;
                    s.hp = 150;
                    s.maxhp = 150;
                    s.speed = (float)0.5;
                    s.BulletID = 6;
                    s.Scale = 1;
                    s.attack = 1;
                    s.WeaponID = 100;
                    s.score = 2;
                    s.Attack_interval = 300;
                    s.IsBoss = true;
                    s.AI_Count = 100;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Enemy\\" + EnemyID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                        return;
                    }
                    EnemyFlakes.Add(s);
                    for (int i = 0; i < EnemyFlakes.Count; i++)
                    {
                        if (EnemyFlakes[i].IsBoss == true)
                        {
                            BossID = i;
                            break;
                        }
                    }
                    Thread theader = new Thread(new ThreadStart(new Action(() =>
                    {
                        Thread.Sleep(5000);
                        s.speed = 0;
                    })));
                    theader.Start();

                }
                if (ID == 7)
                {
                    Enemy s = new Enemy();
                    Random rd = new Random(Convert.ToInt32(new Guid().GetHashCode()));
                    float x = (float)(this.Width - 50);
                    //Trace.WriteLine(x.ToString());
                    s.X = x;
                    s.Y = 64;
                    s.reg = 180;
                    s.hp = 1;
                    s.maxhp = 2;
                    s.speed = (float)15;
                    s.BulletID = 6;
                    s.Scale = 1;
                    s.attack = 1;
                    s.WeaponID = (int)rand.Next(2,6);
                    rd = new Random(Convert.ToInt32(new Guid().GetHashCode()));
                    s.Weapon2 = rd.Next(6, 20); ;
                    s.score = 2;
                    rd = new Random(Convert.ToInt32(new Guid().GetHashCode()));
                    s.Attack_interval =rd.Next(6,20);
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Enemy\\7.png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 03", "Aircraft Battle Error Message");
                        return;
                    }
                    EnemyFlakes.Add(s);

                }
            }
            catch (Exception Ex)
            {
                //MessageBox.Show("Error02:" + Ex.Message + "::Location 04", "Aircraft Battle Error Message");
                return;
            }

        }
        /// <summary>
        /// 飞机移动设置
        /// </summary>
        private void PlaneMove()
        {
            try
            {
                if (Canves != 1)
                {
                    return;
                }
                if (Player.KeyIsDown[1] == true)
                {
                    PlaneFlakes[0].reg = 270;
                }
                else if (Player.KeyIsDown[3] == true)
                {
                    PlaneFlakes[0].reg = 90;
                }
                if (Player.KeyIsDown[2] == true)
                {
                    PlaneFlakes[0].reg = 180;
                }
                else if (Player.KeyIsDown[4] == true)
                {
                    PlaneFlakes[0].reg = 0;
                }
                if (Player.KeyIsDown[1] == true && Player.KeyIsDown[2] == true)
                {
                    PlaneFlakes[0].reg = 225;
                }
                else if (Player.KeyIsDown[3] == true && Player.KeyIsDown[4] == true)
                {
                    PlaneFlakes[0].reg = 45;
                }
                if (Player.KeyIsDown[1] == true && Player.KeyIsDown[4] == true)
                {
                    PlaneFlakes[0].reg = 315;
                }
                else if (Player.KeyIsDown[3] == true && Player.KeyIsDown[2] == true)
                {
                    PlaneFlakes[0].reg = 135;
                }

                /*bool b = false;
                for (int i = 1; i == 4; i++)
                {
                    if (Player.KeyIsDown[i] == true)
                    {
                        b = true;
                        //Trace.WriteLine(i.ToString() + "-> b = true");
                    }
                }*/
                if (Player.KeyIsDown[1] == false && Player.KeyIsDown[2] == false && Player.KeyIsDown[3] == false && Player.KeyIsDown[4] == false)
                {
                    PlaneFlakes[0].speed = 00;
                }
                else
                {
                    if (Player.KeyIsDown[5] == false)
                    {
                        PlaneFlakes[0].speed = 10;
                    }
                    else
                    {
                        PlaneFlakes[0].speed = 5;
                    }
                    //Trace.WriteLine("Stop");
                }
                //Trace.WriteLine("reg:" + PlaneFlakes[0].reg.ToString());
                //Trace.WriteLine("speed:" + PlaneFlakes[0].speed.ToString());
            }
            catch { }
        }
        /// <summary>
        /// 获取坐标间距离
        /// </summary>
        /// <param name="x">X1</param>
        /// <param name="y">Y1</param>
        /// <param name="a">X2</param>
        /// <param name="b">Y2</param>
        /// <returns></returns>
        public float GetDistance(float x, float y, float a, float b)
        {
            return (float)Math.Sqrt((b - y) * (b - y) + (a - x) * (a - x));
        }
        /// <summary>
        /// 获取坐标间角度
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public float Atan2ForCoordinate(float x, float y, float a, float b)
        {
            return (float)(Math.Atan2(b - y, a - x) * 180 / Math.PI);
        }
        /// <summary>
        /// 0:敌人的直线向下攻击，武器ID1：玩家的直线向上攻击
        /// </summary>
        /// <param name="WeaponID">武器序号</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="attack"></param>
        /// <param name="BulletID">子弹贴图序号</param>
        public void Weapon(int WeaponID, float x, float y,int size, int attack, int BulletID,int unitid=0)
        {
            
            //武器ID0：敌人的直线向下攻击
            if (WeaponID == 0)
            {
                try
                {
                    Bullet s = new Bullet();
                    //Random rd = new Random();
                    s.X = x/* + (float)size / 2*/;
                    s.Y = y/* + (float)size / 2*/;
                    s.ownning = 0;
                    s.reg = 90;
                    s.Rotation = 90;
                    s.damage = attack;
                    s.speed = 10;
                    //s.RotVelocity = rand.Next(-3, 3) * 2;
                    //s.lifetime = 0;
                    s.Scale = 2;
                    //ss1.BulletID = 2;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 07", "Aircraft Battle Error Message");
                        return;
                    }
                    BulletFlakes.Add(s);
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error02:" + Ex.Message + "::Location 08", "Aircraft Battle Error Message");
                    return;
                }
            }
                //武器ID1：玩家的直线向上攻击
            if (WeaponID == 1)
            {
                try
                {
                    Bullet s = new Bullet();
                    //Random rd = new Random();
                    s.X = x + (float)size / 2;
                    s.Y = y + (float)size / 2;
                    s.ownning = 1;
                    s.reg = 270;
                    s.damage = attack;
                    //s.RotVelocity = rand.Next(-3, 3) * 2;
                    //s.lifetime = 0;
                    s.Scale = 1;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 05", "Aircraft Battle Error Message");
                        return;
                    }
                    BulletFlakes.Add(s);
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error02:" + Ex.Message + "::Location 06", "Aircraft Battle Error Message");
                    return;
                }
            }
            if (WeaponID == 2)
            {
                //Trace.WriteLine(WeaponID);
                try
                {
                    //Trace.WriteLine(BulletID);
                    for (int i = 0; i <5; i++)
                    {
                        //Trace.WriteLine("循环被执行");
                        Bullet s = new Bullet();
                        //Random rd = new Random();
                        
                        s.X = x /*+ (float)size / 2*/;
                        s.Y = y /*+ (float)size / 2*/;
                        s.ownning = 0;
                        s.reg = 30+30*i;
                        //s.Rotation = 90;
                        s.damage = attack;
                        //s.RotVelocity = rand.Next(-3, 3) * 2;
                        //s.lifetime = 0;
                        s.Scale = 3;
                        s.speed = 10;
                        //ss1.BulletID = 2;
                        try
                        {
                            s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                        }
                        catch (Exception Ex)
                        {
                            //MessageBox.Show("Error:" + Ex.Message + "::Location 07", "Aircraft Battle Error Message");
                            return;
                        }
                        BulletFlakes.Add(s);
                    }
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error02:" + Ex.Message + "::Location 08", "Aircraft Battle Error Message");
                    return;
                }
            }
            if (WeaponID == 3)
            {
                //Trace.WriteLine(WeaponID);
                try
                {
                    //Trace.WriteLine(BulletID);
                    for (int i = 0; i < 12; i++)
                    {
                        Bullet s = new Bullet();
                        s.X = x;
                        s.Y = y;
                        s.ownning = 0;
                        s.reg = 30 * i;
                        s.damage = attack;
                        s.Scale = (float)1;
                        s.speed = 5;

                        try
                        {
                            s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                        }
                        catch
                        {
                        }
                        BulletFlakes.Add(s);
                    }
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error02:" + Ex.Message + "::Location 08", "Aircraft Battle Error Message");
                    return;
                }
            }
            if (WeaponID == 4)
            {
                //Trace.WriteLine(WeaponID);
                try
                {
                    //Trace.WriteLine(BulletID);
                    for (int i = 0; i < 4; i++)
                    {
                        //Trace.WriteLine("循环被执行");
                        Bullet s = new Bullet();
                        //Random rd = new Random();

                        s.X = x /*+ (float)size / 2*/;
                        s.Y = y /*+ (float)size / 2*/;
                        s.ownning = 0;
                        s.reg = 90 * i;
                        //s.Rotation = 90;
                        s.damage = attack;
                        //s.RotVelocity = rand.Next(-3, 3) * 2;
                        //s.lifetime = 0;
                        s.Scale = (float)1;
                        s.speed = 5;
                        //ss1.BulletID = 2;
                        try
                        {
                            s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                        }
                        catch (Exception Ex)
                        {
                            //MessageBox.Show("Error:" + Ex.Message + "::Location 07", "Aircraft Battle Error Message");
                            return;
                        }
                        BulletFlakes.Add(s);
                    }
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error02:" + Ex.Message + "::Location 08", "Aircraft Battle Error Message");
                    return;
                }
            }
            if (WeaponID == 5)
            {
                //Trace.WriteLine(WeaponID);
                try
                {

                        Bullet s = new Bullet();
                        s.X = x /*+ (float)size / 2*/;
                        s.Y = y /*+ (float)size / 2*/;
                        s.ownning = 0;
                        s.reg = Atan2ForCoordinate(x, y, PlaneFlakes[0].X, PlaneFlakes[0].Y);
                        //s.Rotation = 90;
                        s.damage = attack;
                        //s.RotVelocity = rand.Next(-3, 3) * 2;
                        //s.lifetime = 0;
                        s.Scale = 2;
                        s.speed = 7;
                        //ss1.BulletID = 2;
                        try
                        {
                            s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                        }
                        catch (Exception Ex)
                        {
                            //MessageBox.Show("Error:" + Ex.Message + "::Location 07", "Aircraft Battle Error Message");
                            return;
                        }
                        BulletFlakes.Add(s);
                    
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error02:" + Ex.Message + "::Location 08", "Aircraft Battle Error Message");
                    return;
                }
            }
            if (WeaponID == 6)
            {
                //Trace.WriteLine(WeaponID);
                try
                {

                    Bullet s = new Bullet();
                    s.X = x /*+ (float)size / 2*/;
                    s.Y = y /*+ (float)size / 2*/;
                    s.ownning = 0;
                    s.reg = Atan2ForCoordinate(x, y, PlaneFlakes[0].X, PlaneFlakes[0].Y);
                    s.damage = attack;
                    s.Scale = 2;
                    s.speed = 7;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //return;
                    }
                    BulletFlakes.Add(s);
                    s = new Bullet();
                    s.X = x /*+ (float)size / 2*/;
                    s.Y = y /*+ (float)size / 2*/;
                    s.ownning = 0;
                    s.reg = 90;
                    s.damage = attack;
                    s.Scale = 2;
                    s.speed = 10;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + BulletID + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //return;
                    }
                    BulletFlakes.Add(s);

                }
                catch (Exception Ex)
                {
                    //return;
                }
            }
            if (WeaponID == 7)
            {
                //Trace.WriteLine(WeaponID);
                try
                {

                    Bullet s = new Bullet();
                    s.X = x /*+ (float)size / 2*/;
                    s.Y = y /*+ (float)size / 2*/;
                    s.ownning = 0;
                    s.reg = Atan2ForCoordinate(x, y, PlaneFlakes[0].X, PlaneFlakes[0].Y);
                    s.damage = attack;
                    s.Scale = 3;
                    s.speed = 0;
                    s.speedup = true;
                    s.reg = 90;
                    s.Rotation = 90;
                    s.upspeed = (float)0.2;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + 6 + ".png");
                    }
                    catch (Exception Ex)
                    {
                        //return;
                    }
                    BulletFlakes.Add(s);
                   
                }
                catch (Exception Ex)
                {
                    //return;
                }
            }
            //武器ID201：玩家的直线激光向上攻击
            if (WeaponID == 201)
            {
                try
                {
                    Light s = new Light();
                    s.X = x + (float)size / 2;
                    s.Y = y + (float)size / 2;
                    s.ownning = 1;
                    s.reg = 270;
                    s.damage = attack;
                    s.Scale = 1;
                    s.imageID = 1;
                    s.id = BulletID;
                    s.count = 0;
                    s.brushtime = 2;
                    s.lifemax = 3;
                    s.life = 0;
                    s.unitid = unitid;
                    s.imgcount = 2;
                    s.MoveX = 25;
                    s.MoveY = 760;
                    s.Size = 30;
                    s.count = 2;
                    s.hurttime = 3;
                    try
                    {
                        s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Light\\" + s.id+"\\"+ s.imageID+ ".png");
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Error:" + Ex.Message + "::Location 05", "Aircraft Battle Error Message");
                        return;
                    }
                    LightFlakes.Add(s);
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show("Error02:" + Ex.Message + "::Location 06", "Aircraft Battle Error Message");
                    return;
                }
            }
            if (WeaponID == 100)
            {
                //Trace.WriteLine("BOSS攻击触发");
                Bullet s = new Bullet();
                s.X = x /*+ (float)size / 2*/;
                s.Y = y /*+ (float)size / 2*/;
                s.ownning = 0;
                s.reg = Atan2ForCoordinate(x, y, PlaneFlakes[0].X, PlaneFlakes[0].Y);
                s.damage = attack;
                s.Scale = 3;
                s.speed = 7;
                 try
                 {
                       s.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + 11+ ".png");
                  }
                  catch (Exception Ex)
                  {
                  //MessageBox.Show("Error:" + Ex.Message + "::Location 07", "Aircraft Battle Error Message");
                   return;
                  }
                  BulletFlakes.Add(s);
                  
                   Thread theader = new Thread(new ThreadStart(new Action(() =>
                   {
                       Thread.Sleep(600);
                       if (DB)
                       for (int i = 0; i < 12; i++)
                       {
                           Bullet bs = new Bullet();
                           
                           bs.X = x;
                           bs.Y = y;
                           bs.ownning = 0;
                           bs.reg = 30 * i;
                           bs.damage = attack;
                           bs.Scale = 3;
                           bs.speed = 5;
                           try
                           {
                               bs.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + 6 + ".png");
                           }
                           catch (Exception Ex)
                           {
                               //MessageBox.Show("Error:" + Ex.Message + "::Location 07", "Aircraft Battle Error Message");
                               return;
                           }
                           BulletFlakes.Add(bs);
                       }     
                       Thread.Sleep(1000);
                       //第三下
                       if (DB)
                           for (int a = 0; a < 18; a++)
                       {
                           for (int i = 0; i < 6; i++)
                           {
                               Bullet bs = new Bullet();
                               Random rd = new Random();
                               double d = rd.NextDouble() * 5;
                               d = 5;
                               double reg2 = a * d;
                               double reg3 = 60 * i;
                               
                               //bs.X = x;
                               //bs.Y = y;
                               float x2 = x + (float)(Math.Cos((reg3 + reg2) / 180.0 * Math.PI) * 100.0);
                               float y2 = y + (float)(Math.Sin((reg3 + reg2) / 180.0 * Math.PI) * 100.0) ;
                               bs.reg = Atan2ForCoordinate(x2, y2, x, y);
                               bs.X = x2;
                               bs.Y = y2;
                               bs.ownning = 0;
                               
                               bs.damage = attack;
                               bs.Scale = 3;
                               bs.speed = 5;
                               bs.deviation = 4;
                               try
                               {
                                   bs.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + 8 + ".png");
                               }
                               catch
                               {
                               }
                               BulletFlakes.Add(bs);

                               bs = new Bullet();
                               //bs.X = x ;
                               //bs.Y = y;
                               bs.ownning = 0;
                               //bs.reg = a * -5 + -60 * i;
                               rd = new Random();
                               d = rd.NextDouble() * 5;
                               d = 5;
                               reg2 = a * -1 * d;
                               reg3 = 60 * i;
                               //bs.X = x;
                               //bs.Y = y;
                               x2 = x + (float)(Math.Cos((reg3 + reg2) / 180.0 * Math.PI) * 100.0);
                               y2 = y + (float)(Math.Sin((reg3 + reg2) / 180.0 * Math.PI) * 100.0);
                               bs.reg = Atan2ForCoordinate(x2, y2, x, y);
                               bs.X = x2;
                               bs.Y = y2;

                               bs.damage = attack;
                               bs.Scale = 3;
                               bs.speed = 7;
                               bs.deviation = 4;
                               try
                               {
                                   bs.image = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\Bullet\\" + 2 + ".png");
                               }
                               catch
                               {
                               }
                               BulletFlakes.Add(bs);
                           }
                           Thread.Sleep(150);
                       }
                       //Thread.Sleep(1000);
                       //第四下
                       /*for (int a = 0; a < 18; a++)
                       {
                           for (int i = 0; i < 6; i++)
                           {
                               
                           }
                           Thread.Sleep(150);
                       }
                       Thread.Sleep(150);*/
                       //Trace.WriteLine("BOSS攻击触发");
                       if (DB)
                       {
                           s = new Bullet();
                           s.X = x /*+ (float)size / 2*/;
                           s.Y = y /*+ (float)size / 2*/;
                           s.ownning = 0;
                           s.reg = Atan2ForCoordinate(x, y, PlaneFlakes[0].X, PlaneFlakes[0].Y);
                           s.damage = attack;
                           s.Scale = 3;
                           s.speed = 7;
                           try
                           {
                               s.image = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Bullet\\" + 8 + ".png");
                           }
                           catch (Exception Ex)
                           {
                               //MessageBox.Show("Error:" + Ex.Message + "::Location 07", "Aircraft Battle Error Message");
                               return;
                           }
                           BulletFlakes.Add(s);
                       }
                   })));
                   theader.Start();
            }
        }
        /// <summary>
        /// 使用炸弹
        /// <para>清除屏幕的所有非玩家的子弹并且对所有敌人造成demage（20）点伤害</para>
        /// </summary> 
        public void Boom(int demage = 20)
        { 
            if (Player.BoomCount>0)
            {
                Player.BoomCount --;
                
                for (int i = 0; i<EnemyFlakes.Count; i++)
                {
                    EnemyFlakes[i].hp -= demage;
                    CreateEffect(1, EnemyFlakes[i].X, EnemyFlakes[i].Y);
                }
                for (int i = 0; i < BulletFlakes.Count; i++)
                {
                    if (BulletFlakes[i].ownning != 1)
                    {
                        BulletFlakes[i].hp -= 1;
                        //CreateEffect(2, BulletFlakes[i].X, BulletFlakes[i].Y);
                    }
                }
                //CreateBoomEffect

            }

        }
        #endregion

        #region Game timer event         
        private void InvincibleTimer_Tick(object sender, EventArgs e)
        {
            if (PlaneFlakes[0].Invincible == true)
            {
                IT_Count++;
                if (IT_Count % 10 == 0)
                {
                    IT_Count = 0;
                    PlaneFlakes[0].Invincible = false;
                }
            }
        }
        /// <summary>
        /// 敌人刷新器
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {

            Random rd = new Random(Convert.ToInt32(Guid.NewGuid().GetHashCode()));
            if (rd.Next(1, 100) < 5)
            {
                //随机生成飞机
                Thread theader = new Thread(new ThreadStart(new Action(() =>
                {
                    Thread.Sleep(50);
                    CreateEnemy(4);
                })));
                theader.Start();

            }
            rd = new Random(Convert.ToInt32(Guid.NewGuid().GetHashCode()));
            if (rd.Next(1, 100) < 5)
            {
                //随机生成飞机
                Thread theader = new Thread(new ThreadStart(new Action(() =>
                {
                    Thread.Sleep(50);
                    CreateEnemy(7);
                })));
                theader.Start();

            }
            rd = new Random(Convert.ToInt32(Guid.NewGuid().GetHashCode()));
            if (rd.Next(1, 100) < 20)
            {
                //随机生成飞机
                Thread theader = new Thread(new ThreadStart(new Action(() =>
                {
                    Thread.Sleep(150);
                    CreateEnemy(5);
                })));
                theader.Start();

            }
            Thread.Sleep(1);
            CreateEnemy(1);
        }
        /// <summary>
        /// 特殊的敌人刷新器
        /// </summary>
        private void timer2_Tick(object sender, EventArgs e)
        {

            EnCount++;
            if (EnCount % 3 == 0)
            {
                CreateEnemy(3);
            }
            else
            {
                CreateEnemy(2);
            }
            if (EnCount > 1000)
            {
                EnCount = 0;
            }
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            this.timer3.Enabled = false;
            timer3.Stop();
            this.timer1.Enabled = false;
            this.timer2.Enabled = false;
            GameMusic = Application.StartupPath.ToString() + "\\" + "Resources\\Sound\\2.wav";
            if (Game.Music == true)
            {
                splayer.Stop();
                splayer.SoundLocation = GameMusic;
                splayer.PlayLooping();
            }
            Thread theader = new Thread(new ThreadStart(new Action(() =>
            {
                Thread.Sleep(5000);
                Player.BoomCount++;
                Boom();
                Thread.Sleep(1000);
                CreateEnemy(6);
            })));
            theader.Start();

        }
        private void timer4_Tick(object sender, EventArgs e)
        {

        }
        #endregion

        #region 绘图相关
        /// <summary>
        /// 绘制玩家的飞机 - 
        /// </summary>
        /// <param name="g"></param>
        private void Drawtest(Graphics g)
        {
            // Create image.
            Image newImage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Plane\\A02.png");

            // Create rectangle for displaying image.
            Rectangle destRect = new Rectangle(100, 100, 400, 100);

            // Create coordinates of rectangle for source image.
            int x = 200;
            int y = 0;
            int width = 100;
            int height = 100;
            GraphicsUnit units = GraphicsUnit.Pixel;

            g.ResetTransform();
            g.ScaleTransform((float)0.2, (float)0.2, MatrixOrder.Append); //scale
            g.TranslateTransform(350, 350, MatrixOrder.Append); //pan
            g.DrawImage(newImage, destRect, x, y, width, height, units); //draw


        }
        private void DrawBullet(Graphics g)
        {
            for (int ii = 0; ii < BulletFlakes.Count; ii++)
            {
                Bullet a1 = BulletFlakes[ii];
                if (a1.ownning == 0)
                {
                    Random rd = new Random(Convert.ToInt32(Guid.NewGuid().GetHashCode()));
                    //int it = rd.Next(0, 1);
                    if (a1.speedup == true)
                    {
                        a1.speed += a1.upspeed;
                    }
                }
                a1.X = (float)a1.X + (float)Math.Cos(a1.reg / 180 * Math.PI) * a1.speed;
                a1.Y = (float)a1.Y + (float)Math.Sin(a1.reg / 180 * Math.PI) * a1.speed;
                //碰撞判断
                if (a1.Scale > 1)
                {
                    a1.Scale -= (float)0.5;
                }
                else
                {
                    a1.Scale = (float)1;
                }
                float PlayerX = PlaneFlakes[0].X + PlaneFlakes[0].Size / 2;
                float PlayerY = PlaneFlakes[0].Y + PlaneFlakes[0].Size / 2;
                float PlayerSize = PlaneFlakes[0].Size;
                if (a1.ownning == 0)
                {
                    //敌人的子弹
                    //Trace.WriteLine(GetDistance(a1.X + (a1.Size / 2), a1.Y + (a1.Size / 2), (PlayerX + PlayerSize / 2), (PlayerY + PlayerSize / 2)));
                    if (GetDistance(a1.X + (a1.Size / 2), a1.Y + (a1.Size / 2), (PlayerX /*+ PlayerSize / 2*/), (PlayerY /*+ PlayerSize / 2*/)) <= a1.deviation + PlaneFlakes[0].deviation)
                    {
                        //子弹命中
                        //Trace.WriteLine("敌人的子弹命中");
                        if (PlaneFlakes[0].Invincible == false)
                        {
                            CreateEffect(2, PlaneFlakes[0].X + PlaneFlakes[0].Size / 2, PlaneFlakes[0].Y);
                            PlaneFlakes[0].hp -= a1.damage;
                            PlaneFlakes[0].Invincible = true;
                            a1.X = -1;
                            a1.Y = -1;
                        }
                    }
                }
                else
                {
                    //玩家的子弹
                    for (int bb = 0; bb < EnemyFlakes.Count; bb++)
                    {
                        //碰撞判断
                        float X = EnemyFlakes[bb].X + EnemyFlakes[bb].Size / 2;
                        float Y = EnemyFlakes[bb].Y + EnemyFlakes[bb].Size / 2;
                        float Size = EnemyFlakes[bb].Size;
                        //玩家的子弹
                        //Trace.WriteLine(GetDistance(GetDistance(a1.X + a1.Size / 2, a1.Y + a1.Size / 2, X - Size / 2, Y - Size / 2));
                        if (GetDistance(a1.X + (a1.Size / 2), a1.Y + (a1.Size / 2), X /*+ (Size / 2)*/, Y /*+ (Size / 2)*/) <= a1.deviation + EnemyFlakes[bb].deviation)
                        {
                            //子弹命中
                            //Trace.WriteLine("玩家的子弹命中");
                            CreateEffect(2, EnemyFlakes[bb].X, EnemyFlakes[bb].Y);
                            EnemyFlakes[bb].hp -= a1.damage;
                            a1.X = -1;
                            a1.Y = -1;
                        }

                    }
                }
                if (a1.X > this.Width || a1.Y > this.Height || a1.X < 0 || a1.Y < 0 || a1.hp < 1)
                {
                    //离开画面，清除
                    a1.Scale = 1f;
                    a1.image = null;
                    BulletFlakes.RemoveAt(ii);
                }
                else
                {
                    //绘制图像
                    g.ResetTransform();
                    g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                    g.ScaleTransform(a1.Scale, a1.Scale, MatrixOrder.Append); //scale
                    g.RotateTransform(a1.Rotation, MatrixOrder.Append); //rotate
                    g.TranslateTransform(a1.X + a1.Size / 2, a1.Y + a1.Size / 2, MatrixOrder.Append); //pan
                    g.DrawImage(a1.image, 0, 0); //draw
                }
            }
        }
        private void DrawLight(Graphics g)
        {
            for (int ii = 0; ii < LightFlakes.Count; ii++)
            {
                Light a1 = LightFlakes[ii];
                //移动
                if (a1.ownning == 1)
                {
                    a1.X = PlaneFlakes[0].X + (float)PlaneFlakes[0].Size / 2;
                    a1.Y = PlaneFlakes[0].Y + (float)PlaneFlakes[0].Size / 2;
                }

                //伤害
                a1.count++;
                if (a1.count % a1.hurttime == 0 && a1.count != 0)
                {
                    //a1.count = 0;
                    for (int bb = 0; bb < EnemyFlakes.Count; bb++)
                    {
                        //碰撞判断
                        float X = EnemyFlakes[bb].X + EnemyFlakes[bb].Size / 2;
                        float Y = EnemyFlakes[bb].Y + EnemyFlakes[bb].Size / 2;
                        float Size = EnemyFlakes[bb].Size;

                        if (X > a1.X && X < a1.X + a1.Size && Y < a1.Y)
                        {
                            CreateEffect(2, EnemyFlakes[bb].X, EnemyFlakes[bb].Y);
                            EnemyFlakes[bb].hp -= a1.damage;
                        }
                    }
                }
                //刷新动画
                if (a1.count % a1.brushtime == 0)
                {
                    a1.imageID++;
                    if (a1.imageID > a1.imgcount)
                    {
                        a1.imageID = 1;
                    }
                    a1.image = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Light\\" + a1.id + "\\" + a1.imageID + ".png");
                }
                a1.life++;
                if (a1.life > a1.lifemax)
                {
                    //移除
                    a1.count = 0;
                    LightFlakes.Remove(a1);
                }
                else
                {
                    //绘制特效图像
                    g.ResetTransform();
                    g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                    g.ScaleTransform(a1.Scale, a1.Scale, MatrixOrder.Append); //scale
                    g.RotateTransform(a1.Rotation, MatrixOrder.Append); //rotate
                    g.TranslateTransform(a1.X - ((a1.MoveX) * a1.Scale), a1.Y - ((a1.MoveY) * a1.Scale), MatrixOrder.Append); //pan
                    g.DrawImage(a1.image, 0, 0); //draw
                }

            }
        }
        private void DrawEnemy(Graphics g)
        {
            for (int ii = 0; ii < EnemyFlakes.Count; ii++)
            {
                Enemy ss1 = EnemyFlakes[ii];
                /*if (ss1.IsBoss == true)
                {
                    BossID = ii;
                }*/
                ss1.X = (float)ss1.X + (float)Math.Cos(ss1.reg / 180 * Math.PI) * ss1.speed;
                ss1.Y = (float)ss1.Y + (float)Math.Sin(ss1.reg / 180 * Math.PI) * ss1.speed;
                if (ss1.hp <= 0 || (ss1.X > this.Width || ss1.Y > this.Height || ss1.X < 0 || ss1.Y < 0))
                {
                    //单位被摧毁
                    SoundPlay(SoundPath.UnitDieSound);
                    CreateEffect(1, ss1.X, ss1.Y);
                    if (ss1.IsBoss == true)
                    {
                        DB = false;
                        PlaneFlakes[0].hp++;
                        PlaneFlakes[0].maxhp++;
                        Player.BoomCount++;
                        Player.Score += 25;
                        timer1.Enabled = true;
                        timer2.Enabled = true;
                        GameMusic = Application.StartupPath.ToString() + "\\" + "Resources\\Sound\\1.wav";
                        if (Game.Music == true)
                        {
                            splayer.Stop();
                            splayer.SoundLocation = GameMusic;
                            splayer.PlayLooping();
                        }
                        timer3.Enabled = true;
                        timer3.Interval = 120000;
                        timer3.Start();
                    }
                    ss1.Scale = 1f;
                    ss1.image = null;
                    EnemyFlakes.RemoveAt(ii);
                    Player.Score += ss1.score;
                    ss1.AI_Count = 0;
                }
                else
                {
                    //绘制画面图像
                    g.ResetTransform();
                    g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                    g.ScaleTransform(ss1.Scale, ss1.Scale, MatrixOrder.Append); //scale
                    g.RotateTransform(ss1.Rotation, MatrixOrder.Append); //rotate
                    g.TranslateTransform(ss1.X - ss1.Size / 2, ss1.Y - ss1.Size / 2, MatrixOrder.Append); //pan
                    g.DrawImage(ss1.image, 0, 0); //draw
                }
                if (ss1.AI_Count % ss1.Attack_interval == 0 && ss1.hp > 0)
                {
                    //发射子弹

                    Weapon(ss1.WeaponID, ss1.X + ss1.BulletMoveX, ss1.Y + ss1.BulletMoveY, 0, ss1.attack, ss1.BulletID);
                    Weapon(ss1.Weapon2, ss1.X + ss1.BulletMoveX, ss1.Y + ss1.BulletMoveY, 0, ss1.attack, ss1.BulletID);
                }

                //计数增加
                ss1.AI_Count++;

            }
        }
        private void DrawPlane(Graphics g)
        {
            // 绘制玩家的飞机
            for (int ii = 0; ii < PlaneFlakes.Count; ii++)
            {
                Plane ss1 = PlaneFlakes[ii];
                if (Game.UseMouse == true)
                {
                    PlaneFlakes[0].reg = Atan2ForCoordinate(PlaneFlakes[0].X, PlaneFlakes[0].Y, Game.MouseX, Game.MouseY);
                    if (GetDistance(PlaneFlakes[0].X, PlaneFlakes[0].Y, Game.MouseX, Game.MouseY) > 10)
                    {
                        if (Player.KeyIsDown[5] == false)
                        {
                            PlaneFlakes[0].speed = 10;
                        }
                        else
                        {
                            PlaneFlakes[0].speed = 5;
                        }
                    }
                    else
                    {
                        PlaneFlakes[0].speed = GetDistance(PlaneFlakes[0].X, PlaneFlakes[0].Y, Game.MouseX, Game.MouseY);
                    }
                }
                if (!((float)ss1.X + (float)Math.Cos(ss1.reg / 180 * Math.PI) * ss1.speed > this.Width - ss1.Size
                    || (float)ss1.Y + (float)Math.Sin(ss1.reg / 180 * Math.PI) * ss1.speed > this.Height - ss1.Size
                    || (float)ss1.X + (float)Math.Cos(ss1.reg / 180 * Math.PI) * ss1.speed < 0
                    || (float)ss1.Y + (float)Math.Sin(ss1.reg / 180 * Math.PI) * ss1.speed < 0))
                {
                    ss1.X = (float)ss1.X + (float)Math.Cos(ss1.reg / 180 * Math.PI) * ss1.speed;
                    ss1.Y = (float)ss1.Y + (float)Math.Sin(ss1.reg / 180 * Math.PI) * ss1.speed;
                }
                for (int bb = 0; bb < EnemyFlakes.Count; bb++)
                {
                    //碰撞判断
                    float X = EnemyFlakes[bb].X + EnemyFlakes[bb].Size / 2;
                    float Y = EnemyFlakes[bb].Y + EnemyFlakes[bb].Size / 2;
                    float Size = EnemyFlakes[bb].Size;
                    //玩家的子弹
                    //Trace.WriteLine(GetDistance(GetDistance(a1.X + a1.Size / 2, a1.Y + a1.Size / 2, X - Size / 2, Y - Size / 2));
                    if (GetDistance(ss1.X + (ss1.Size / 2), ss1.Y + (ss1.Size / 2), X /*+ (Size / 2)*/, Y /*+ (Size / 2)*/) <= ss1.deviation + EnemyFlakes[bb].deviation)
                    {
                        if (ss1.Invincible == false)
                        {
                            ss1.hp--;
                            EnemyFlakes[bb].hp -= 5;
                        }
                    }

                }
                if (ss1.hp <= 0)
                {
                    //游戏结束
                    //根据玩家分数增加玩家金币数量
                    Player.Gold = Player.Gold + Convert.ToInt32(Player.Score / 10);
                    /*ss1.Scale = 1f;
                    PlaneFlakes.RemoveAt(ii);*/
                    //画字----------------------
                    g.ResetTransform();
                    g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan                    
                    g.TranslateTransform(0, 100, MatrixOrder.Append); //pan
                    Font font = new Font("微软雅黑", 72);
                    Brush brush = new SolidBrush(Color.White);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                    g.DrawString("Game Over", font, brush, PointF.Empty);
                    //-----------------------
                    //画字----------------------
                    g.ResetTransform();
                    g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                    font = new Font("微软雅黑", 50);
                    g.TranslateTransform(0, 200, MatrixOrder.Append); //pan
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                    g.DrawString("按下空格键回到开始界面", font, brush, PointF.Empty);
                    //-----------------------
                    //画字----------------------
                    g.ResetTransform();
                    g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan                    
                    g.TranslateTransform(0, 300, MatrixOrder.Append); //pan
                    font = new Font("微软雅黑", 60);
                    brush = new SolidBrush(Color.White);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                    g.DrawString("你的得分为：" + Player.Score.ToString(), font, brush, PointF.Empty);
                    //-----------------------
                    GameCenterTimer.Enabled = false;
                    timer1.Enabled = false;
                    timer2.Enabled = false;
                    timer3.Enabled = false;
                    timer3.Stop();
                    timer2.Stop();
                    timer1.Stop();
                    //MessageBox.Show("Game Over,你的得分为：" + Player.Score.ToString() + "点击确定后，按下空格键继续");
                    GameOver = true;
                    //记录最高分数据
                    if (Player.Score > Player.MaxScore)
                    {
                        Player.MaxScore = Player.Score;

                    }
                    WriteXML();//存档
                }
                else
                {
                    Image newImage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Plane\\A02.png");
                    // Create rectangle for displaying image.
                    Rectangle destRect = new Rectangle((int)100, (int)100, 400, 100);
                    int a = 1;
                    ss1.ImageMoveCount++;
                    if (ss1.ImageMoveCount / 2 > 4)
                    {
                        ss1.ImageMoveCount = 1;
                        a = 1;
                    }
                    if (ss1.ImageMoveCount / 2 < 1)
                    {
                        ss1.ImageMoveCount = 1;
                        a = 1;
                    }
                    a = (int)ss1.ImageMoveCount / 4;
                    if (ss1.Invincible == false)
                    {
                        newImage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Plane\\A02.png");
                    }
                    else
                    {
                        //if(ss1.ImageMoveCount%2==0)
                        newImage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Plane\\A02.1.png");

                    }
                    // Create coordinates of rectangle for source image.
                    int x = 0;//100*a;
                    int y = 0;
                    int width = 100;
                    int height = 100;
                    GraphicsUnit units = GraphicsUnit.Pixel;
                    g.ResetTransform();
                    g.ScaleTransform((float)0.2, (float)0.2, MatrixOrder.Append); //scale
                    g.TranslateTransform(ss1.X - 40, ss1.Y - 22, MatrixOrder.Append); //pan
                    g.DrawImage(newImage, destRect, x, y, width, height, units); //draw

                }
                if (ss1.AI_Count == ss1.Attack_interval)
                {
                    //发射子弹
                    ss1.AI_Count = 0;
                    Weapon(ss1.WeaponID, ss1.X, ss1.Y, ss1.Size, ss1.attack, ss1.BulletID);
                }
                else
                {
                    //计数增加
                    ss1.AI_Count++;
                }
            }
        }
        private void DrawEffect(Graphics g)
        {
            for (int ii = 0; ii < EffectFlakes.Count; ii++)
            {
                Effect ss1 = EffectFlakes[ii];
                if (ss1.Mode == 1)
                {
                    ss1.lifetime++;
                    if (ss1.lifetime % ss1.brushtime == 0)
                    {
                        ss1.imageID++;
                        ss1.image = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Effect\\E" + ss1.actID + "\\" + ss1.imageID + ".png");

                    }
                    if (ss1.Y > this.Height || ss1.lifetime >= ss1.lifemax || ss1.X > this.Width)
                    {
                        //ss1.X = 1f;
                        ss1.XVelocity = 0f;
                        ss1.YVelocity = 0f;
                        ss1.lifetime = 0;
                        ss1.Scale = 1f;
                        EffectFlakes.RemoveAt(ii);
                    }
                    else
                    {
                        //绘制特效图像
                        g.ResetTransform();
                        g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                        g.ScaleTransform(ss1.Scale, ss1.Scale, MatrixOrder.Append); //scale
                        g.RotateTransform(ss1.Rotation, MatrixOrder.Append); //rotate
                        g.TranslateTransform(ss1.X - ((ss1.MoveX) * ss1.Scale), ss1.Y - ((ss1.MoveY) * ss1.Scale), MatrixOrder.Append); //pan
                        g.DrawImage(ss1.image, 0, 0); //draw
                    }
                }
            }
        }
        private void DrawUI(Graphics g)
        {
            //绘制音量图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.5, (float)0.5, MatrixOrder.Append); //scale
            g.TranslateTransform(730, 15, MatrixOrder.Append); //pan
            g.DrawImage(UI.SoundImage, 0, 0); //draw
                                              //绘制暂停图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)1.5, (float)1.5, MatrixOrder.Append); //scale
            g.TranslateTransform(715, 38, MatrixOrder.Append); //pan
            g.DrawImage(UI.Stop, 0, 0); //draw
                                        //绘制鼠标图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.5, (float)0.5, MatrixOrder.Append); //scale
            g.TranslateTransform(670, 20, MatrixOrder.Append); //pan
            g.DrawImage(UI.Mouse, 0, 0); //draw
                                         //画框
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform(1, (float)1.1, MatrixOrder.Append); //scale
            g.TranslateTransform(4, 5, MatrixOrder.Append); //pan
            g.DrawImage(UI.hpimage2, 0, 0); //draw
                                            //画条
            if (PlaneFlakes[0].hp > 0)
            {
                g.ResetTransform();
                g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                g.ScaleTransform((float)PlaneFlakes[0].hp / (float)PlaneFlakes[0].maxhp, 1, MatrixOrder.Append); //scale
                g.TranslateTransform(0, 0, MatrixOrder.Append); //pan
                g.DrawImage(UI.hpimage, 0, 0); //draw
                                               //画字
                g.ResetTransform();
                g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                g.TranslateTransform(100, 12, MatrixOrder.Append); //pan
                Font font = new Font("微软雅黑", 16);
                Brush brush = new SolidBrush(Color.White);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                g.DrawString(PlaneFlakes[0].hp.ToString() + "/" + PlaneFlakes[0].maxhp.ToString(), font, brush, PointF.Empty);
            }
            //UI得分
            g.ResetTransform();
            //g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.TranslateTransform(5, 40, MatrixOrder.Append); //pan
            g.ScaleTransform(1, 1, MatrixOrder.Append); //scale
            g.TranslateTransform(0, 0, MatrixOrder.Append); //pan
            g.DrawImage(UI.Score, 0, 0); //draw
                                         //得分
            g.ResetTransform();
            //g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.TranslateTransform(50, 33, MatrixOrder.Append); //pan
            Font fo = new Font("微软雅黑", 20);
            Brush bru = new SolidBrush(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.DrawString(":" + Player.Score.ToString(), fo, bru, PointF.Empty);
            //最高得分
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.TranslateTransform(15, 71, MatrixOrder.Append); //pan
            fo = new Font("微软雅黑", 20);
            bru = new SolidBrush(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.DrawString("MaxScroe:" + Player.MaxScore, fo, bru, PointF.Empty);
            //画炸弹
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform(1, (float)1.1, MatrixOrder.Append); //scale
            g.TranslateTransform(16, 102, MatrixOrder.Append); //pan
            g.DrawImage(UI.Boom, 0, 0); //draw
                                        //显示炸弹数量
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.TranslateTransform(32, 96, MatrixOrder.Append); //pan
            fo = new Font("微软雅黑", 16);
            bru = new SolidBrush(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.DrawString(":" + Player.BoomCount.ToString(), fo, bru, PointF.Empty);
            try
            {
                if (DB == true)
                {
                    //画框
                    g.ResetTransform();
                    g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                    g.ScaleTransform(1, (float)1.1, MatrixOrder.Append); //scale
                    g.TranslateTransform(300, 5, MatrixOrder.Append); //pan
                    g.DrawImage(UI.hpimage2, 0, 0); //draw
                                                    //画条
                    for (int i = 0; i < EnemyFlakes.Count; i++)
                    {
                        //标记BOSS在enemyflakes中的索引位置
                        if (EnemyFlakes[i].IsBoss == true)
                        {
                            //记录
                            BossID = i;
                            break;
                        }
                    }
                    if (EnemyFlakes[BossID].hp > 0)
                    {
                        g.ResetTransform();
                        g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                        g.ScaleTransform((float)EnemyFlakes[BossID].hp / (float)EnemyFlakes[BossID].maxhp, 1, MatrixOrder.Append); //scale
                        g.TranslateTransform(296, 0, MatrixOrder.Append); //pan
                        g.DrawImage(UI.hpimage, 0, 0); //draw
                                                       //画字
                        g.ResetTransform();
                        g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                        g.TranslateTransform(305, 12, MatrixOrder.Append); //pan
                        Font font = new Font("微软雅黑", 16);
                        Brush brush = new SolidBrush(Color.White);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                        g.DrawString("BOSS生命值：" + EnemyFlakes[BossID].hp.ToString() + "/" + EnemyFlakes[BossID].maxhp, font, brush, PointF.Empty);
                    }
                }
            }
            catch
            { }
        }
        private void DrawBackground(Graphics g)
        {
            // 不好用的画背景
            //*仅在暂停时播放背景
            UI.BGY++;
            if (UI.BGY >= 600)
            {
                UI.BGY = 0;
            }
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform(1, (float)1, MatrixOrder.Append); //scale
            g.TranslateTransform(UI.BGX, UI.BGY, MatrixOrder.Append); //pan
            g.DrawImage(UI.BG, 0, 0); //draw
                                      /*UI.BGY2--;
                                      if (UI.BGY2 <=600)
                                      {
                                          UI.BGY2 = 1200;
                                      }*/
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform(1, (float)1, MatrixOrder.Append); //scale
            g.TranslateTransform(0, -600 + UI.BGY, MatrixOrder.Append); //pan
            g.DrawImage(UI.BG, 0, 0); //draw
        }
        private void DrawStopCanves(Graphics g)
        { //绘制音量图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.5, (float)0.5, MatrixOrder.Append); //scale
            g.TranslateTransform(730, 15, MatrixOrder.Append); //pan
            g.DrawImage(UI.SoundImage, 0, 0); //draw
                                              //绘制鼠标图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.5, (float)0.5, MatrixOrder.Append); //scale
            g.TranslateTransform(670, 20, MatrixOrder.Append); //pan
            g.DrawImage(UI.Mouse, 0, 0); //draw
                                         //-------------
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan                    
            g.TranslateTransform(100, 140, MatrixOrder.Append); //pan
            Font font = new Font("微软雅黑", 60);
            Brush brush = new SolidBrush(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.DrawString("游戏暂停中...", font, brush, PointF.Empty);
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan                    
            g.TranslateTransform(100, 260, MatrixOrder.Append); //pan
            font = new Font("微软雅黑", 60);
            brush = new SolidBrush(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.DrawString("按下空格键继续...", font, brush, PointF.Empty);
        }
        private void DrawStartCanves(Graphics g)
        {
            //绘制背景
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.75, (float)0.75, MatrixOrder.Append); //scale
            g.TranslateTransform(0, 0, MatrixOrder.Append); //pan
            g.DrawImage(UI.BackG, 0, 0); //draw
                                         //绘制金币图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.5, (float)0.5, MatrixOrder.Append); //scale
            g.TranslateTransform(10, 50, MatrixOrder.Append); //pan
            g.DrawImage(UI.gold, 0, 0); //draw
                                        //显示金币数量
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.TranslateTransform(40, 55, MatrixOrder.Append); //pan
            Font fo = new Font("微软雅黑", 16);
            Brush bru = new SolidBrush(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.DrawString(":" + Player.Gold.ToString(), fo, bru, PointF.Empty);
            //绘制音量图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.5, (float)0.5, MatrixOrder.Append); //scale
            g.TranslateTransform(730, 15, MatrixOrder.Append); //pan
            g.DrawImage(UI.SoundImage, 0, 0); //draw
                                              //绘制开始按钮
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform(1, (float)1.1, MatrixOrder.Append); //scale
            g.TranslateTransform(240, 200, MatrixOrder.Append); //pan
            g.DrawImage(UI.PlayBtn, 0, 0); //draw
                                           //绘制鼠标图标
            g.ResetTransform();
            g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
            g.ScaleTransform((float)0.5, (float)0.5, MatrixOrder.Append); //scale
            g.TranslateTransform(670, 20, MatrixOrder.Append); //pan
            g.DrawImage(UI.Mouse, 0, 0);
            //draw
            //Drawtest(g);
            //绘制其他按钮
            /*
                                         g.ResetTransform();
                                         g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                                         g.ScaleTransform(1, (float)1.1, MatrixOrder.Append); //scale
                                         g.TranslateTransform(280, 325, MatrixOrder.Append); //pan
                                         g.DrawImage(UI.ShopBtn, 0, 0); //draw
            */
        }
        #endregion

        #region Form event
        private void Form1_Load(object sender, EventArgs e)
        {
            Shop shop = new Shop();
            shop.Show();
            //判断是否有新的更新日志
            StreamReader sR = File.OpenText(Application.StartupPath.ToString() + "\\" + "更新日志.txt");
            string sss = sR.ReadToEnd();
            sR.Close();
            if (sss != "")
            {
                UpdateBox ub = new UpdateBox();
                ub.Show();
                //显示并清空
            }

            Form = this;

            splayer = new SoundPlayer();

            LoadXml();
            GameMusic = Application.StartupPath.ToString() + "\\" + "Resources\\Sound\\1.wav";

            if (Game.Music == true)
            {
                splayer.Stop();
                splayer.SoundLocation = GameMusic;
                splayer.PlayLooping();
                UI.SoundImage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\soundon.png");
            }
            else
            {
                UI.SoundImage = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\UI\\soundoff.png");
                splayer.Stop();
            }
            //在游戏画布中创建玩家的飞机

            try
            {
                Plane s = new Plane();
                Random rd = new Random();
                s.X = 400;
                s.Y = 300;
                s.reg = 180;
                Player.PlaneID = 2;
                s.Size = 32;
                s.deviation = 8;
                s.Scale = 3;
                s.MoveX = 16;
                s.MoveY = 16;
                if (Player.MaxScore > 4000)
                {
                    s.WeaponID = 201;
                }
                else
                {
                    s.WeaponID = 1;
                }
                try
                {
                    s.image = Image.FromFile(Application.StartupPath.ToString() + "\\" + "Resources\\Plane\\" + Player.PlaneID + ".png");

                }
                catch
                {
                }
                PlaneFlakes.Add(s);
            }
            catch
            {

            }
            //
        }
        /// <summary>
        /// 主绘制函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics myBuffer = currentContext.Allocate(e.Graphics, e.ClipRectangle);
            Graphics g = myBuffer.Graphics;
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.HighSpeed;

            if (Canves == 0)
            {
                // 不好用的画背景
                //*仅在暂停时播放背景
                UI.BGY++;
                if (UI.BGY >= 600)
                {
                    UI.BGY = 0;
                }
                g.ResetTransform();
                g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                g.ScaleTransform(1, (float)1, MatrixOrder.Append); //scale
                g.TranslateTransform(UI.BGX, UI.BGY, MatrixOrder.Append); //pan
                g.DrawImage(UI.BG, 0, 0); //draw
                /*UI.BGY2--;
                if (UI.BGY2 <=600)
                {
                    UI.BGY2 = 1200;
                }*/
                g.ResetTransform();
                g.TranslateTransform(-16, -16, MatrixOrder.Append); //pan
                g.ScaleTransform(1, (float)1, MatrixOrder.Append); //scale
                g.TranslateTransform(0, -600 + UI.BGY, MatrixOrder.Append); //pan
                g.DrawImage(UI.BG, 0, 0); //draw
            }


            if (Canves == 1)
            {

                //绘制背景
                DrawBackground(g);
                //绘制玩家的飞机
                DrawPlane(g);
                //绘制敌人的飞机
                DrawEnemy(g);
                //绘制激光
                DrawLight(g);
                //绘制子弹
                DrawBullet(g);
                //绘制特效
                DrawEffect(g);
                //绘制UI
                DrawUI(g);

            }
            //暂停界面
            if (Canves == 0)
            {
                DrawStopCanves(g);
            }
            //游戏开始界面
            if (Canves == 2)
            {
                DrawStartCanves(g);
            }
            myBuffer.Render(e.Graphics);
            g.Dispose();
            myBuffer.Dispose();
        }
        //Player control
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Canves != 1)
                {
                    return;
                }
                //Trace.WriteLine(e.KeyCode.ToString());
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    Player.KeyIsDown[1] = true;
                    Player.KeyIsDown[3] = false;
                }
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    Player.KeyIsDown[3] = true;
                    Player.KeyIsDown[1] = false;
                }
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                {
                    Player.KeyIsDown[2] = true;
                    Player.KeyIsDown[4] = false;
                }
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                {
                    Player.KeyIsDown[4] = true;
                    Player.KeyIsDown[2] = false;
                }
                if (e.KeyCode == Keys.ShiftKey)
                {
                    Trace.WriteLine("按下shift");
                    Player.KeyIsDown[5] = true;
                }
                PlaneMove();
            }
            catch
            { }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Canves != 1)
                {
                    return;
                }
                //Trace.WriteLine("KeyUp:"+e.KeyCode.ToString());
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    Player.KeyIsDown[1] = false;
                }
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    Player.KeyIsDown[3] = false;
                }
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                {
                    Player.KeyIsDown[2] = false;
                }
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                {
                    Player.KeyIsDown[4] = false;
                }
                if (e.KeyCode == Keys.ShiftKey)
                {
                    Player.KeyIsDown[5] = false;
                }
                PlaneMove();
            }
            catch { }
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (GameOver == true)
            {
                //Trace.WriteLine(e.KeyChar.ToString());
                if ((int)e.KeyChar == 32 && Canves != 2)
                {
                    Canves = 2;
                    GameCenterTimer.Enabled = true;
                    return;
                }
            }
            if ((int)e.KeyChar == 32 && Canves == 1)
            {
                if (Player.BoomCount > 0)
                {

                    if (Game.Music == true)
                    {
                        SoundPlay(Application.StartupPath.ToString() + "\\" + "Resources\\Sound\\Game\\Boom.wav");
                    }
                    CreateEffect(3, 320, 150);
                    Boom();
                }
            }
            if ((int)e.KeyChar == 32 && Canves == 0)
            {
                GameStop(false);
            }
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            /*if (Canves != 2 && PlaneFlakes[0].hp > 0)
            {
                GameStop(false);
                if (Game.Music == true)
                {
                    splayer.SoundLocation = GameMusic;
                    splayer.PlayLooping();
                }
            }*/
        }
        private void Form1_LostFocus(object sender, EventArgs e)
        {
            /*if (Canves != 2 && PlaneFlakes[0].hp > 0)
            {
                GameStop(true);
                if (Game.Music == true)
                {
                    splayer.Stop();
                }
            }*/
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //dv.Dispose();
            //结束后存档
            WriteXML();

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            GameStop(true);
            Canves = 2;
        }
        #endregion

        #region xml区块----玩家存档
        //Save&Load
        /// <summary>
        /// 保存数据-玩家属性
        /// </summary>
        /// <param name="PlayerScore">玩家最高分</param>
        /// <param name="PlayerPlaneID">玩家飞机编号</param>
        /// <param name="PlayerGold">玩家金币</param>
        /// <param name="PlayerID">玩家编号</param>
        public void WriteXML()
        {
            //假若文件不存在则新建一个xml文件
            string FileName = Application.StartupPath.ToString() + "\\"+"PlayerState.xml";
            if (!File.Exists(FileName))
            {
                FileStream fs1 = new FileStream(Application.StartupPath.ToString() + "\\" + "PlayerState.xml", FileMode.Create, FileAccess.Write);//创建写入文件               
                //System.IO.File.SetAttributes("PlayerState.xml", FileAttributes.Hidden);
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + "\n<PlayerState>" + "\n</PlayerState>");//开始写入值
                sw.Close();
                fs1.Close();
                //初始化XML文档操作类
                XmlDocument myDoc = new XmlDocument();
                //加载XML文件
                myDoc.Load(FileName);
                //添加元素--玩家分数
                XmlElement ele = myDoc.CreateElement("PlayerScore");
                XmlText text = myDoc.CreateTextNode(Player.MaxScore.ToString());

                //添加元素--玩家当前飞机编号
                XmlElement ele1 = myDoc.CreateElement("PlayerPlaneID");
                XmlText text1 = myDoc.CreateTextNode(Player.PlaneID.ToString());

                //添加元素--玩家金币数量
                XmlElement ele2 = myDoc.CreateElement("PlayerGold");
                XmlText text2 = myDoc.CreateTextNode(Player.Gold.ToString());
                //添加元素--玩家编号
                XmlElement ele3 = myDoc.CreateElement("PlayerID");
                XmlText text3 = myDoc.CreateTextNode("0");
                //添加元素--玩家编号
                XmlElement ele4 = myDoc.CreateElement("PlayerExp");
                XmlText text4 = myDoc.CreateTextNode(Player.Exp.ToString());
                //添加元素--是否开启音效
                XmlElement ele5 = myDoc.CreateElement("Music");
                XmlText text5 = myDoc.CreateTextNode("0");
                //添加元素--是否启用鼠标控制模式
                XmlElement ele6 = myDoc.CreateElement("Mouse");
                XmlText text6 = myDoc.CreateTextNode("0");
                //添加节点 Player要对应我们xml文件中的节点名字
                XmlNode newElem = myDoc.CreateNode("element", "Player", "");

                //在节点中添加元素
                newElem.AppendChild(ele3);
                newElem.LastChild.AppendChild(text3);
                newElem.AppendChild(ele);
                newElem.LastChild.AppendChild(text);
                newElem.AppendChild(ele1);
                newElem.LastChild.AppendChild(text1);
                newElem.AppendChild(ele2);
                newElem.LastChild.AppendChild(text2);
                newElem.AppendChild(ele4);
                newElem.LastChild.AppendChild(text4);
                newElem.AppendChild(ele5);
                newElem.LastChild.AppendChild(text5);
                newElem.AppendChild(ele6);
                newElem.LastChild.AppendChild(text6);

                //将节点添加到文档中
                XmlElement root = myDoc.DocumentElement;
                root.AppendChild(newElem);

                //保存
                myDoc.Save(FileName);
            }
             else
            {
                //初始化XML文档操作类
                XmlDocument myDoc = new XmlDocument();
                //加载XML文件
                myDoc.Load(FileName);

                //搜索指定的节点
                System.Xml.XmlNodeList nodes = myDoc.SelectNodes("//Player");

                if (nodes != null)
                {
                    foreach (System.Xml.XmlNode xn in nodes)
                    {
                        if (xn.SelectSingleNode("PlayerID").InnerText == "0")
                        {
                            string s = "PlayerPlaneID";
                            System.Xml.XmlNodeList n = xn.SelectNodes(s);
                            Trace.WriteLine(n.Count);
                            if (n.Count > 0)
                            {
                                xn.SelectSingleNode(s).InnerText = Player.PlaneID.ToString();
                            }
                            else
                            {
                                XmlElement ele1 = myDoc.CreateElement(s);
                                XmlText text1 = myDoc.CreateTextNode("2");
                                xn.AppendChild(ele1);
                                xn.LastChild.AppendChild(text1);
                            }
                            
                            s = "PlayerGold";
                            n = xn.SelectNodes(s);
                            if (n.Count > 0)
                            {
                                xn.SelectSingleNode(s).InnerText = Player.Gold.ToString();
                            }
                            else
                            {
                                XmlElement ele1 = myDoc.CreateElement(s);
                                XmlText text1 = myDoc.CreateTextNode("0");
                                xn.AppendChild(ele1);
                                xn.LastChild.AppendChild(text1);
                            }
                            s = "PlayerScore";
                            n = xn.SelectNodes(s);
                            if (n.Count > 0)
                            {
                                xn.SelectSingleNode(s).InnerText = Player.MaxScore.ToString();
                            }
                            else
                            {
                                XmlElement ele1 = myDoc.CreateElement(s);
                                XmlText text1 = myDoc.CreateTextNode("0");
                                xn.AppendChild(ele1);
                                xn.LastChild.AppendChild(text1);
                            }
                            s = "PlayerExp";
                            n = xn.SelectNodes(s);
                            if (n.Count > 0)
                            {
                                xn.SelectSingleNode(s).InnerText = Player.Exp.ToString();
                            }
                            else
                            {
                                XmlElement ele1 = myDoc.CreateElement(s);
                                XmlText text1 = myDoc.CreateTextNode("0");
                                xn.AppendChild(ele1);
                                xn.LastChild.AppendChild(text1);
                            }
                            s = "Music";
                            n = xn.SelectNodes(s);
                            if (n.Count > 0)
                            {

                                if (Game.Music == false)
                                {

                                    xn.SelectSingleNode(s).InnerText = "0";
                                }
                                else
                                {
                                    xn.SelectSingleNode(s).InnerText = "1";
                                }
                            }
                            else
                            {
                                XmlElement ele1 = myDoc.CreateElement(s);
                                XmlText text1 = myDoc.CreateTextNode("0");
                                xn.AppendChild(ele1);
                                xn.LastChild.AppendChild(text1);
                            }
                            s = "Mouse";
                            n = xn.SelectNodes(s);
                            if (n.Count > 0)
                            {
                                if (Game.UseMouse == false)
                                {

                                    xn.SelectSingleNode(s).InnerText = "0";
                                }
                                else
                                {
                                    xn.SelectSingleNode(s).InnerText = "1";
                                }
                            }
                            else
                            {
                                XmlElement ele1 = myDoc.CreateElement(s);
                                XmlText text1 = myDoc.CreateTextNode("0");
                                xn.AppendChild(ele1);
                                xn.LastChild.AppendChild(text1);
                            }
                            /*假设没有节点测试*/
                            /*s = "PlayerPlane";
                            n = xn.SelectNodes(s);
                            if (n.Count > 0)
                            {
                                //xn.SelectSingleNode(s).InnerText = Player.PlaneID.ToString();

                            }
                            else
                            {
                                XmlElement ele1 = myDoc.CreateElement(s);
                                XmlText text1 = myDoc.CreateTextNode("123");
                                xn.AppendChild(ele1);
                                xn.LastChild.AppendChild(text1);
                            }
                            */
                            //---------------------
                        }

                    }
                }
                //保存
                myDoc.Save(FileName);
            }
            
        }
        /// <summary>
        /// 获取玩家存档数据
        /// </summary>
        public void LoadXml()
        {
            //假若文件不存在则新建一个xml文件
            if (!File.Exists(Application.StartupPath.ToString() + "\\" + "PlayerState.xml"))
            {
                Player.Gold = 0;
                Player.Exp = 0;
                Player.PlaneID = 1;
                Player.MaxScore = 0;
                WriteXML();
            }

            //初始化XML文档操作类
            XmlDocument myDoc = new XmlDocument();
            //加载XML文件
            myDoc.Load(Application.StartupPath.ToString()  + "\\PlayerState.xml");

            //搜索指定的节点
            System.Xml.XmlNodeList nodes = myDoc.SelectNodes("//Player");

            if (nodes != null)
            {
                foreach (System.Xml.XmlNode xn in nodes)
                {
                    
                    string s = "PlayerPlaneID";
                    System.Xml.XmlNodeList n = xn.SelectNodes(s);
                    if (n.Count > 0)
                    {
                        Player.PlaneID = Convert.ToInt32(xn.SelectSingleNode("PlayerPlaneID").InnerText);
                    }
                    else
                    {
                        XmlElement ele1 = myDoc.CreateElement(s);
                        XmlText text1 = myDoc.CreateTextNode("2");
                        xn.AppendChild(ele1);
                        xn.LastChild.AppendChild(text1);
                    }
                    s = "PlayerExp";
                    n = xn.SelectNodes(s);
                    if (n.Count > 0)
                    {
                        Player.Exp = Convert.ToInt32(xn.SelectSingleNode("PlayerExp").InnerText);
                    }
                    else
                    {
                        XmlElement ele1 = myDoc.CreateElement(s);
                        XmlText text1 = myDoc.CreateTextNode("0");
                        xn.AppendChild(ele1);
                        xn.LastChild.AppendChild(text1);
                    }
                    s = "PlayerGold";
                    n = xn.SelectNodes(s);
                    if (n.Count > 0)
                    {
                        Player.Gold = Convert.ToInt32(xn.SelectSingleNode("PlayerGold").InnerText);
                    }
                    else
                    {
                        XmlElement ele1 = myDoc.CreateElement(s);
                        XmlText text1 = myDoc.CreateTextNode("0");
                        xn.AppendChild(ele1);
                        xn.LastChild.AppendChild(text1);
                    }
                    s = "PlayerScore";
                    n = xn.SelectNodes(s);
                    if (n.Count > 0)
                    {
                        Player.MaxScore = Convert.ToInt32(xn.SelectSingleNode("PlayerScore").InnerText);
                    }
                    else
                    {
                        XmlElement ele1 = myDoc.CreateElement(s);
                        XmlText text1 = myDoc.CreateTextNode("0");
                        xn.AppendChild(ele1);
                        xn.LastChild.AppendChild(text1);
                    }
                    s = "Music";
                    n = xn.SelectNodes(s);
                    if (n.Count > 0)
                    {

                        if (xn.SelectSingleNode(s).InnerText == "0")
                        {
                            Game.Music = false;
                            UI.SoundImage = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\soundoff.png");
                            splayer.Stop();
                        }
                        else
                        {
                            Game.Music = true;
                            UI.SoundImage = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\soundon.png");
                            GameMusic = Application.StartupPath.ToString()+"\\"+"Resources\\Sound\\1.wav";
                            splayer.SoundLocation = GameMusic;
                            splayer.PlayLooping();
                        }
                    }
                    else
                    {
                        XmlElement ele1 = myDoc.CreateElement(s);
                        XmlText text1 = myDoc.CreateTextNode("0");
                        xn.AppendChild(ele1);
                        xn.LastChild.AppendChild(text1);
                    }
                    s = "Mouse";
                    n = xn.SelectNodes(s);
                    if (n.Count > 0)
                    {

                        if (xn.SelectSingleNode(s).InnerText == "0")
                        {
                            Game.UseMouse = false;
                            UI.Mouse = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\mouseoff.png");
                            splayer.SoundLocation = GameMusic;
                            splayer.PlayLooping();
                        }
                        else
                        {
                            Game.UseMouse = true;
                            UI.Mouse = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\mouseon.png");
                            splayer.Stop();
                        }
                    }
                    else
                    {
                        XmlElement ele1 = myDoc.CreateElement(s);
                        XmlText text1 = myDoc.CreateTextNode("0");
                        xn.AppendChild(ele1);
                        xn.LastChild.AppendChild(text1);
                    }
                    myDoc.Save("PlayerState.xml");
                    
                }
            }
        }
        /// <summary>
        /// ？？？
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #endregion

        #region 界面相关 + 控制
        //UI&Control
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                //play 图标的变换-按下鼠标变暗730+68/2 ,15+64/2 768,47
 
                if (e.X > 710 && e.X < 748 && e.Y > 15 && e.Y < 47)
                {
                    //Trace.WriteLine("音量按钮被点击");
                Device dv = new Device();
                dv.SetCooperativeLevel(this, CooperativeLevel.Priority);
                BufferDescription buffer = new BufferDescription();
                buffer.GlobalFocus = true;
                buffer.ControlVolume = true;
                buffer.ControlPan = true;
                SecondaryBuffer buf = new SecondaryBuffer(Application.StartupPath.ToString()+"\\"+"Resources\\Sound\\UI\\btn.wav", buffer, dv);
                buf.Play(0, BufferPlayFlags.Default);
                    if (Game.Music == false)
                    {
                        //Trace.WriteLine("true");
                        Game.Music = true;
                        UI.SoundImage = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\soundon.png");
                        splayer.SoundLocation = GameMusic;
                        splayer.PlayLooping();
                    }
                    else
                    {
                        //Trace.WriteLine("false");
                        Game.Music = false;
                        UI.SoundImage = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\soundoff.png");
                        splayer.Stop();
                    }
                }

                if (e.X > 690 && e.X < 720 && e.Y > 15 && e.Y < 47)
                {

                    //Trace.WriteLine("暂停按钮被点击");
                    if (Canves == 1)
                    {
                        Device dv = new Device();
                        dv.SetCooperativeLevel(this, CooperativeLevel.Priority);
                        BufferDescription buffer = new BufferDescription();
                        buffer.GlobalFocus = true;
                        buffer.ControlVolume = true;
                        buffer.ControlPan = true;
                        SecondaryBuffer buf = new SecondaryBuffer(Application.StartupPath.ToString()+"\\"+"Resources\\Sound\\UI\\btn.wav", buffer, dv);
                        buf.Play(0, BufferPlayFlags.Default);
                        GameStop(true);
                    }

                }
                if (e.X > 650 && e.X < 690 && e.Y > 15 && e.Y < 47)
                {

                    //Trace.WriteLine("鼠标按钮被点击");
                    
                    {
                        Device dv = new Device();
                        dv.SetCooperativeLevel(this, CooperativeLevel.Priority);
                        BufferDescription buffer = new BufferDescription();
                        buffer.GlobalFocus = true;
                        buffer.ControlVolume = true;
                        buffer.ControlPan = true;
                        SecondaryBuffer buf = new SecondaryBuffer(Application.StartupPath.ToString()+"\\"+"Resources\\Sound\\UI\\btn.wav", buffer, dv);
                        buf.Play(0, BufferPlayFlags.Default);
                        if (Game.UseMouse == true)
                        {
                            Game.UseMouse = false;
                            UI.Mouse = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\mouseoff.png");
                        }
                        else
                        {
                            Game.UseMouse = true;
                            UI.Mouse = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\mouseon.png");
                        }
                    }

                }
            }
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //play 图标的变换-移入变亮 移出变暗
            if (e.X > 220 && e.X < 500 && e.Y > 200 && e.Y < 289 && Canves == 2)
                {
                    if (UI.PlayBtnPath != Application.StartupPath.ToString()+"\\"+"Resources\\UI\\btn_play_1.png")
                    UI.PlayBtn = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\btn_play_0.png");
                }
                else
                {
                    UI.PlayBtn = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\btn_play_2.png");
                }
            //控制飞机移动
            Game.MouseX = e.X - PlaneFlakes[0].Size/2;
            Game.MouseY = e.Y - PlaneFlakes[0].Size/2;
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                //play 图标的变换-按下鼠标变暗
                if (e.X > 220 && e.X < 500 && e.Y > 200 && e.Y < 289 && Canves == 2)
                {
                    UI.PlayBtn = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\btn_play_1.png");
                }
                /*else
                {
                    UI.PlayBtn = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\btn_play_0.png");
                }*/
            }
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                //SoundPlay("btn.mp3");
                
                //play 图标的变换-松开鼠标变亮
                if (e.X > 220 && e.X < 500 && e.Y > 200 && e.Y < 289&&Canves==2)
                {
                    try
                    {
                        Device dv = new Device();
                        
                        dv.SetCooperativeLevel(this, CooperativeLevel.Priority);
                        //Microsoft.DirectX.DirectSound.Buffer buff = new Microsoft.DirectX.DirectSound.Buffer("btn"+".wav", dv);
                        //SecondaryBuffer buf = new SecondaryBuffer(Application.StartupPath.ToString()+"\\"+"Resources\\Sound\\UI\\btn.wav", dv);
                        //buf.Play(0, BufferPlayFlags.Default);
                        BufferDescription buffer = new BufferDescription();
                        buffer.GlobalFocus = true;
                        buffer.ControlVolume = true;
                        buffer.ControlPan = true;
                        SecondaryBuffer buf = new SecondaryBuffer(Application.StartupPath.ToString()+"\\"+"Resources\\Sound\\UI\\btn.wav",buffer, dv);
                        buf.Play(0, BufferPlayFlags.Default);
                    }
                    catch(SoundException se)
                    {
                        MessageBox.Show(se.ErrorString);
                    }
                    if (GameOver == true)
                    {
                        //Trace.WriteLine("点击play按钮");
                        //SoundPlay(@"Resources\Sound\UI\btn.wav");
                            GameOver = false;
                            GameStart();
                            Canves = 1;
                            Player.KeyIsDown[1] = false;
                            Player.KeyIsDown[2] = false;
                            Player.KeyIsDown[3] = false;
                            Player.KeyIsDown[4] = false;
                    }
                    else
                    {
                        //SoundPlay(@"Resources\Sound\UI\btn.wav");
                        UI.PlayBtn = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\btn_play_0.png");
                        //play按钮被点击松开  游戏开始
                        GameStop(false);
                        GameOver = false;
                        GameStart();
                        Canves = 1;
                    }
                }
                /*else
                {
                    UI.PlayBtn = Image.FromFile(Application.StartupPath.ToString()+"\\"+"Resources\\UI\\btn_play_0.png");
                }*/
            }
        }
        #endregion
    }
}
