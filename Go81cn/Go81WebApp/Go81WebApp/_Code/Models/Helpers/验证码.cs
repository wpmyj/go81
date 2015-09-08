using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MongoDB
{
    public class 验证码
    {
        public string Code { get; private set; }
        public Image Image { get; private set; }
        public 验证码()
        {
            Generate();
        }
        public void Generate()
        {
            Code = GenerateCode();
            Image = GenerateImage(Code);
        }
        private string GenerateCode(int length = 4)//生成随机数
        {
            string Code = "";
            Random rd = new Random();
            string[] Codestr = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            for (int i = 0; i < length; i++)
            {
                Code += Codestr[rd.Next(35)];
            }
            return Code;
        }
        private Image GenerateImage(string code)//生成验证码图片
        {
            Random rd = new Random();
            Bitmap img = new Bitmap(code.Length * 16 + 16, 35);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.Clear(Color.White);
                Font CodeFont = new Font("Arial", 16, FontStyle.Bold | FontStyle.Italic);
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, img.Width, 20), Color.Green, Color.Blue, 30);
                for (int i = 0; i < 25; i++)//画噪线
                {
                    int x1 = rd.Next(img.Width);
                    int y1 = rd.Next(img.Height);
                    int x2 = rd.Next(img.Width);
                    int y2 = rd.Next(img.Height);
                    g.DrawLine(new Pen(Color.LightGreen), x1, y1, x2, y2);
                }
                g.DrawString(code, CodeFont, brush, new Point(6, 5));
                for (int num = 0; num < 100; num++)//画噪点
                {
                    int x = rd.Next(img.Width);
                    int y = rd.Next(img.Height);
                    img.SetPixel(x, y, Color.FromArgb(rd.Next()));
                }
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, img.Width - 1, img.Height - 1);//画边线
                return img;
            }
        }
    }
}