using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace Helpers.印章
{
    public class 圆形公章 : IDisposable
    {
        public float 半径 { get; set; } //= 100F;
        public float 实际绘制半径 { get; private set; }
        public Color 边框颜色 { get; set; } //= Color.Red;
        public float 边框宽度 { get; set; } //= 3F;
        public bool 中央五角星 { get; set; } //= true;
        public bool 八一五角星 { get; set; } //= true;
        public Color 中央五角星颜色 { get; set; } //= Color.Red;
        public float 单位签名分布角度 { get; set; } //= 230F;
        public bool 自动计算签名字体大小 { get; set; } //= true;
        public bool 自动计算单位签名分布角度 { get; set; } //= true;
        public bool 随机旋转 { get; set; } //= false;
        public int 旋转角度 { get; set; } //= 0;
        public byte 透明度 { get; set; } //= 225;
        public bool 辅助线 { get; set; } //= false;
        public bool 自动清理资源 { get; set; } //= true;
        public string Png文件路径 { get; set; }
        public string Emf文件路径 { get; set; }
        public 签章信息 单位签名 { get; set; }
        public 签章信息 业务签名 { get; set; }
        public class 签章信息
        {
            public string 文字 { get; set; }
            public string 字体名 { get; set; }
            public float 字体大小 { get; set; }
            public float 字体宽高比 { get; set; }
            public bool 粗体 { get; set; }
            public Color 颜色 { get; set; }
            public 签章信息()
            {
                文字 = "壹贰叁肆伍陆柒捌玖";
                字体名 = "华文中宋";
                字体大小 = float.NaN;
                字体宽高比 = 2.0F;
                粗体 = false;
                颜色 = Color.Red;
            }
        }
        public 圆形公章(bool autoGcCollect = false)
        {
            半径 = 100F;
            边框宽度 = 3F;
            中央五角星 = true;
            八一五角星 = false;
            单位签名分布角度 = 230F;
            自动计算签名字体大小 = true;
            自动计算单位签名分布角度 = true;
            透明度 = 225;
            辅助线 = false;
            随机旋转 = false;
            旋转角度 = 0;
            自动清理资源 = autoGcCollect;

            单位签名 = new 签章信息
            {
                文字 = "龘龘龘龘龘龘龘龘龘龘龘龘龘龘龘",
                字体大小 = 21F,
            };
            业务签名 = new 签章信息
            {
                文字 = "龘龘龘龘龘龘龘龘龘",
                字体大小 = 17F,
            };

            字体名 = "华文中宋";
            颜色 = Color.Red;
        }
        public string 字体名
        {
            set { 单位签名.字体名 = 业务签名.字体名 = value; }
            get { return 单位签名.字体名 == 业务签名.字体名 ? 单位签名.字体名 : 单位签名.字体名 + "|" + 业务签名.字体名; }
        }
        public Color? 颜色
        {
            set { 边框颜色 = 中央五角星颜色 = 单位签名.颜色 = 业务签名.颜色 = Color.FromArgb(透明度, value ?? Color.Red); }
            get { return 边框颜色 == 中央五角星颜色 && 中央五角星颜色 == 单位签名.颜色 && 单位签名.颜色 == 业务签名.颜色 ? 边框颜色 : (Color?) null; }
        }

        private const int _padding = 4;
        private float _unitWidth;
        private float _layoutAngel;
        private float _bmpRadius;
        private float _borderWidth;
        private float _angle;
        private Bitmap _bmp;
        private Graphics _gfx;
        private GraphicsPath _gp;
        public Bitmap 绘制结果
        {
            get
            {
                return (null != _bmp || (绘制结果信息.成功 == 绘制() && null != _bmp))
                    ? _bmp.Clone(new Rectangle(0, 0, _bmp.Width, _bmp.Height), _bmp.PixelFormat)
                    : null
                    ;
            }
        }
        private 绘制结果信息 绘制()
        {
            var res = 检查数据();
            if (绘制结果信息.成功 != res) return res;
            //计算参数
            实际绘制半径 = Math.Max(50, Math.Min(半径, 3000));
            _unitWidth = 实际绘制半径/100;
            _layoutAngel = 自动计算单位签名分布角度 || float.IsNaN(单位签名分布角度)
                ? 业务签名.文字.Length <= 7
                    ? 240F
                    : 业务签名.文字.Length <= 9
                        ? 240F - 10*(业务签名.文字.Length - 7)
                        : 220F
                : 单位签名分布角度
                ;
            _bmpRadius = _padding + 实际绘制半径;
            _borderWidth = 边框宽度*_unitWidth;
            //确定随机旋转角度
            if (0 != 旋转角度)
            {
                if (随机旋转)
                {
                    var rdm = new Random();
                    _angle = rdm.Next(-旋转角度, 旋转角度);
                }
                else _angle = 旋转角度;
            }
            else _angle = float.NaN;
            //创建 Bitmap 和 Graphics 对象等
            var w = (int) (_bmpRadius*2);
            if (null != _bmp) _bmp.Dispose();
            _bmp = new Bitmap(w, w);
            var gfx0 = Graphics.FromImage(_bmp);
            var hdc = IntPtr.Zero;
            Metafile emf = null;
            if (!string.IsNullOrWhiteSpace(Emf文件路径))
            {
                hdc = gfx0.GetHdc();
                emf = new Metafile(Emf文件路径, hdc);
                _gfx = Graphics.FromImage(emf);
            }
            else _gfx = gfx0;

            绘制印章();

            _gfx.Save();
            if (null != emf)
            {
                //回收 emf 资源
                _gfx.Dispose();
                emf.Dispose();
                gfx0.ReleaseHdc(hdc);
                gfx0.Dispose();
                //重新绘制到 _bmp
                _bmp.Dispose();
                _bmp = new Bitmap(w, w);
                _gfx = Graphics.FromImage(_bmp);
                绘制印章();
                _gfx.Save();
            }
            _gfx.Dispose();
            _gfx = null;
            保存Png图片();
            if (自动清理资源) GC.Collect();
            return res;
        }
        private void 绘制印章()
        {
            //准备绘制参数
            _gfx.SmoothingMode = SmoothingMode.HighQuality;
            _gfx.CompositingQuality = CompositingQuality.HighQuality;
            _gfx.CompositingMode = CompositingMode.SourceOver;
            _gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (null != _gp) _gp.Reset();
            else _gp = new GraphicsPath();
            //处理全局平移和随机旋转
            _gfx.TranslateTransform(_bmpRadius, _bmpRadius);
            if (!float.IsNaN(_angle)) _gfx.RotateTransform(_angle);
            //绘制开始
            if (辅助线)
            {
                _gfx.DrawLine(Pens.Blue, -_bmpRadius, 0, _bmpRadius, 0);
                _gfx.DrawLine(Pens.Blue, 0, -_bmpRadius, 0, _bmpRadius);
            }
            绘制圆形边框();
            绘制单位签名();
            绘制业务签名();
            绘制中央五角星();
        }
        private void 绘制圆形边框()
        {
            var r = 实际绘制半径 - _borderWidth / 2;
            _gp.AddEllipse(-r, -r, r * 2, r * 2);
            using (var pen = new Pen(边框颜色, _borderWidth))
                _gfx.DrawPath(pen, _gp);
        }
        private void 绘制单位签名()
        {
            using (var ff = new FontFamily(单位签名.字体名))
            using (var brush = new SolidBrush(单位签名.颜色))
            {
                var fz = 自动计算签名字体大小 || float.IsNaN(单位签名.字体大小)
                    ? 实际绘制半径*0.21F
                    : 单位签名.字体大小*_unitWidth
                    ;
                var offsetR = (float) (实际绘制半径 - _borderWidth*2F - fz/2*(单位签名.字体宽高比 + 0.2));
                var angle = _layoutAngel/(单位签名.文字.Length - 1);
                for (var i = 0; i < 单位签名.文字.Length; ++i)
                {
                    _gp.Reset();
                    _gp.AddString(单位签名.文字[i].ToString(), ff,
                        (int) (单位签名.粗体 ? FontStyle.Bold : FontStyle.Regular),
                        fz, new PointF(0, 0), new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                        });
                    if (辅助线)
                    {
                        var r = _gp.GetBounds();
                        _gp.AddRectangle(r);
                    }

                    var m = new Matrix();
                    m.Scale(1.0F, 单位签名.字体宽高比);
                    _gp.Transform(m);
                    m.Reset();

                    m.Translate(0, -offsetR);
                    _gp.Transform(m);
                    m.Reset();

                    m.Rotate(360 - (_layoutAngel/2) + (angle*i));
                    _gp.Transform(m);
                    m.Reset();

                    _gfx.FillPath(brush, _gp);
                }
            }
        }
        private void 绘制业务签名()
        {
            using(var ff = new FontFamily(业务签名.字体名))
            using(var brush = new SolidBrush(业务签名.颜色))
            {
                var fs = (int) (业务签名.粗体 ? FontStyle.Bold : FontStyle.Regular);
                var fz = 自动计算签名字体大小 || float.IsNaN(业务签名.字体大小)
                    ? 实际绘制半径*(业务签名.文字.Length <= 8 ? 0.170F : 业务签名.文字.Length <= 9 ? 0.158F : 0.146F)
                    : 业务签名.字体大小*_unitWidth
                    ;
                var pt = new PointF();
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                var sh0 = 0F;
                var offsetX = (业务签名.文字.Sum(c =>
                {
                    _gp.Reset();
                    _gp.AddString(c.ToString(), ff, fs, fz, pt, sf);
                    var r = _gp.GetBounds();
                    sh0 = Math.Min(sh0, r.Y);
                    return r.Width;
                }) + 业务签名.文字.Length - 1)/2;
                var offsetR = (float) Math.Sqrt(
                    Math.Pow(实际绘制半径 - _borderWidth, 2) - Math.Pow(offsetX, 2)
                    ) + sh0 - _borderWidth*1.5F;
                for (var i = 0; i < 业务签名.文字.Length; ++i)
                {
                    _gp.Reset();
                    _gp.AddString(业务签名.文字[i].ToString(), ff, fs, fz, pt, sf);
                    var r = _gp.GetBounds();
                    if (辅助线) _gp.AddRectangle(r);

                    var m = new Matrix();
                    m.Scale(1.0F, 业务签名.字体宽高比);
                    _gp.Transform(m);
                    m.Reset();

                    m.Translate(-offsetX + r.Width/2 + i, offsetR);
                    offsetX -= r.Width;
                    _gp.Transform(m);
                    m.Reset();

                    _gfx.FillPath(brush, _gp);
                }
            }
        }
        private void 绘制中央五角星()
        {
            if (!中央五角星) return;
            var r = 实际绘制半径*0.43F;

            const int pn = 5;
            const int ptn = pn*2;
            var pts = new PointF[ptn];
            var r2 = r*Math.Cos(2*Math.PI/pn)/Math.Cos(Math.PI/pn);
            for (var i = 0; i < pn; ++i)
            {
                var A = i*2*Math.PI/pn;
                var B = (i*2 + 1)*Math.PI/pn;
                pts[2*i] = new PointF((float) (r*Math.Sin(A)), (float) (-r*Math.Cos(A)));
                pts[2*i + 1] = new PointF((float) (r2*Math.Sin(B)), (float) (-r2*Math.Cos(B)));
            }
            _gp.Reset();
            _gp.AddPolygon(pts);

            if (八一五角星)
            {
                using (var pen = new Pen(中央五角星颜色, _unitWidth))
                    _gfx.DrawPath(pen, _gp);

                _gp.Reset();
                _gp.AddPolygon(pts);
                foreach(var p in 八一军徽文字.GetAe81Polygons(_unitWidth))
                    _gp.AddPolygon(p);

                var m = new Matrix();
                var scale = (r - 9*_unitWidth)/r;
                m.Scale(scale, scale);
                _gp.Transform(m);
            }

            using (var brush = new SolidBrush(中央五角星颜色))
                _gfx.FillPath(brush, _gp);
        }
        private void 保存Png图片()
        {
            if (null != _bmp && !string.IsNullOrWhiteSpace(Png文件路径))
                _bmp.Save(Png文件路径, ImageFormat.Png);
        }
        private 绘制结果信息 检查数据()
        {
            if (0 == 半径.CompareTo(0F)) return 绘制结果信息.错误_半径不能为0;
            if (string.IsNullOrWhiteSpace(单位签名.文字)) return 绘制结果信息.错误_单位签名文字不能为null或空白;
            if (string.IsNullOrWhiteSpace(单位签名.字体名)) return 绘制结果信息.错误_单位签名字体名不能为null或空白;
            if (0 == 单位签名.字体大小.CompareTo(0F)) return 绘制结果信息.错误_单位签名字体大小不能为0;
            if (0 == 单位签名.字体宽高比.CompareTo(0F)) return 绘制结果信息.错误_单位签名字体高宽比不能为0;
            if (string.IsNullOrWhiteSpace(业务签名.文字)) return 绘制结果信息.错误_业务签名文字不能为null或空白;
            if (string.IsNullOrWhiteSpace(业务签名.字体名)) return 绘制结果信息.错误_业务签名字体名不能为null或空白;
            if (0 == 业务签名.字体大小.CompareTo(0F)) return 绘制结果信息.错误_业务签名字体大小不能为0;
            if (0 == 业务签名.字体宽高比.CompareTo(0F)) return 绘制结果信息.错误_业务签名字体高宽比不能为0;
            return 绘制结果信息.成功;
        }
        public void Dispose()
        {
            try { if (null != _gp) _gp.Dispose(); } catch { }
            try { if (null != _gfx) _gfx.Dispose(); } catch { }
            try { if (null != _bmp) _bmp.Dispose(); } catch { }
            if (自动清理资源) GC.Collect();
        }
        public enum 绘制结果信息
        {
            成功 = 0,
            成功_错误 = 400,
            错误_半径不能为0,
            错误_单位签名文字不能为null或空白,
            错误_单位签名字体名不能为null或空白,
            错误_单位签名字体大小不能为0,
            错误_单位签名字体高宽比不能为0,
            错误_业务签名文字不能为null或空白,
            错误_业务签名字体名不能为null或空白,
            错误_业务签名字体大小不能为0,
            错误_业务签名字体高宽比不能为0,
        }
    }
    public static class 八一军徽文字
    {
        private static readonly string[] ae81PolygonsJson =
        {
            "[[-387.8909,-473.0468],[115.2341,-473.0468],[182.4216,-531.6405],[291.7966,-436.328],[238.6716,-338.6717],[235.5466,-319.1405],[233.9846,-300.3905],[233.2026,-280.8592],[233.2026,-262.1092],[234.7656,-244.1405],[236.3276,-225.3905],[238.6716,-207.4217],[242.5776,-189.453],[245.7026,-172.2655],[250.3906,-155.078],[255.8596,-138.6717],[260.5466,-122.2655],[266.0156,-106.6405],[272.2656,-91.79669],[278.5156,-76.953],[284.7656,-62.8905],[291.0156,-49.60919],[298.0466,-36.328],[304.2966,-24.60919],[310.5466,-12.8905],[316.7966,-2.734192],[323.0466,7.421997],[328.5156,16.797],[333.9846,24.6095],[343.3596,38.672],[350.3906,48.82831],[355.0776,55.07831],[357.4216,57.422],[219.9216,191.0158],[205.8596,175.3908],[192.5776,158.2033],[180.8596,140.2345],[169.1404,121.4845],[158.2029,102.7345],[148.8279,83.9845],[139.4529,63.672],[130.8591,43.3595],[123.0466,23.047],[116.0154,1.953308],[109.7654,-19.1405],[103.5154,-39.453],[98.0466,-60.54669],[92.57791,-80.85919],[88.6716,-100.3905],[84.76541,-120.703],[80.8591,-140.2342],[77.7341,-158.203],[75.39041,-176.1717],[73.0466,-194.1405],[71.4841,-209.7655],[69.14041,-225.3905],[68.3591,-240.2342],[66.7966,-252.7342],[65.2341,-275.3905],[64.45291,-293.3592],[64.45291,-307.4217],[-393.3596,-307.4217]]",
            "[[-326.9534,-194.9217],[-166.7971,-191.7967],[-167.5784,-178.5155],[-168.3596,-166.0155],[-169.9221,-153.5155],[-171.4846,-140.2342],[-173.8284,-127.7342],[-175.3909,-114.453],[-177.7346,-101.1717],[-180.0784,-87.8905],[-183.2034,-75.3905],[-186.3284,-62.10919],[-189.4534,-48.828],[-193.3596,-35.54669],[-197.2659,-23.04669],[-201.9534,-9.765503],[-206.6409,2.734497],[-211.3284,14.45331],[-216.0159,26.95331],[-221.4846,39.45331],[-227.7346,51.172],[-233.9846,62.89081],[-240.2346,73.82831],[-247.2659,84.76581],[-254.2971,94.922],[-262.1096,105.8595],[-269.9221,115.2345],[-278.5159,124.6095],[-287.8909,133.9845],[-296.4846,142.5783],[-306.6409,150.3908],[-316.7971,158.2033],[-326.9534,165.2345],[-337.8909,171.4845],[-376.9534,134.7658],[-373.8284,128.5158],[-369.9221,122.2658],[-367.5784,115.2345],[-364.4534,107.422],[-362.1096,100.3908],[-358.9846,91.797],[-356.6409,83.9845],[-354.2971,75.39081],[-351.9534,66.01581],[-350.3909,57.422],[-348.8284,48.047],[-346.4846,37.89081],[-343.3596,17.57831],[-341.0159,-3.515503],[-338.6721,-25.3905],[-336.3284,-48.04669],[-334.7659,-71.48419],[-332.4221,-95.703],[-330.8596,-119.9217],[-329.2971,-144.9217],[-327.7346,-169.9217]]",
            "[[-385.5471,390.2345],[183.9846,390.2345],[190.2346,389.4533],[196.4846,389.4533],[201.9526,388.672],[206.6406,387.8908],[212.1096,387.1095],[216.0156,386.3283],[219.9216,385.547],[223.8276,383.9845],[226.9526,382.422],[230.0776,380.8595],[233.2026,379.297],[234.7656,377.7345],[237.8906,375.3908],[239.4526,373.8283],[241.0156,371.4845],[242.5776,369.1408],[244.1406,366.0158],[244.9216,363.672],[245.7026,360.547],[248.0466,354.297],[249.6096,348.047],[251.9526,332.422],[255.8596,314.4533],[393.3596,385.547],[391.0156,391.0158],[386.3276,405.8595],[382.4216,415.2345],[377.7346,426.172],[372.2656,438.6725],[365.2346,451.1725],[361.3276,457.4225],[357.4216,463.6725],[353.5156,469.9225],[348.8276,476.1725],[344.1406,483.2035],[338.6716,488.6725],[333.9846,494.9225],[327.7346,500.3905],[322.2656,505.8595],[316.0156,510.5475],[309.7656,515.2345],[302.7346,519.9225],[295.7026,523.8285],[287.8906,526.9535],[280.0776,530.0785],[272.2656,531.6405],[-385.5471,531.6405]]",
        };
        private const double defaultAe81Scale = 0.0225D;
        private const double correctAe81offsetX = 36D;
        private const double correctAe81offsetY = 50D;
        private static double currentAe81Scale = defaultAe81Scale;
        private static IEnumerable<PointF[]> ae81Polygons;
        public static IEnumerable<PointF[]> GetAe81Polygons(double scale = 1.0D)
        {
            scale *= defaultAe81Scale;
            var eq = scale.CompareTo(currentAe81Scale);
            if (0 == eq && null != ae81Polygons) return ae81Polygons;
            if (0 != eq) currentAe81Scale = scale;
            ae81Polygons = ae81PolygonsJson
                .Select(JsonConvert.DeserializeObject<double[][]>)
                .Select(p => p.Select(f => new PointF
                {
                    X = (float) ((f[0] + correctAe81offsetX)*currentAe81Scale),
                    Y = (float) ((f[1] + correctAe81offsetY)*currentAe81Scale),
                }).ToArray())
                ;
            return ae81Polygons;
        }
    }
}
