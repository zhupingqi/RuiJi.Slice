using RuiJi.Slicer.Core;
using RuiJi.Slicer.Core.Slicer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuiJi.Slice.App
{
    public class StepDistance
    {
        public int Step { get; set; }

        public float D { get; set; }
    }

    public class RuiJiCNC
    {
        private class StepInfo
        {
            public int X { get; set; }

            public int Y { get; set; }

            public int Z { get; set; }

            public int Step { get; set; }

            public StepInfo()
            {
                this.X = -1;
                this.Y = -1;
            }

            public StepInfo(int x, int y, int z, int step = 1)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
                this.Step = step;
            }

            public override string ToString()
            {
                if (this.X == -1 || this.Y == -1)
                    return "n";

                if (Step > 1)
                    return $"{X},{Y},{Z},{Step}";

                return $"{X},{Y},{Z}";
            }

            public bool IsSamePosition(StepInfo info)
            {
                return X == info.X && Y == info.Y && X != -1;
            }
        }

        private static RuiJiCNC ruiJiCNC = null;

        Dictionary<float, List<SlicedPlane>> dict;

        Dictionary<float, Dictionary<int, float>> cache;

        private int keyIndex = 0;

        private SerialPort port;

        private Task task;

        private const float slice_angle = 1.8f;

        private readonly double MEARM_MIN_R = 2.0;
        private readonly double DEG_TO_RAD = Math.PI / 180;
        private readonly double RAD_TO_DEG = 180.0 / Math.PI;
        private readonly double MEARM_R = 22.0;

        private readonly double baseX = 15;
        private readonly double baseY = 20;
        private readonly double dis = 7;

        private int slices = Convert.ToInt32(360.0 / slice_angle);

        private ManualResetEvent idleResetEvent = new ManualResetEvent(false);

        private bool cnc_run = false;

        private bool cancelTask = false;

        private RuiJiCNC()
        {
            dict = new Dictionary<float, List<SlicedPlane>>();
            cache = new Dictionary<float, Dictionary<int, float>>();
        }

        public static RuiJiCNC Instance
        {
            get
            {
                if (ruiJiCNC == null)
                    ruiJiCNC = new RuiJiCNC();

                return ruiJiCNC;
            }
        }

        public void SetDict(Dictionary<float, List<SlicedPlane>> dict)
        {
            this.dict = dict;
        }

        public void Start(SerialPort port)
        {
            this.port = port;
            task = new Task(() =>
            {
                cancelTask = false;
                ruiJiCNC.Run();
            });

            task.Start();
        }

        public void Pause()
        {

        }

        public void Resume()
        {

        }

        public void Stop()
        {
            cancelTask = true;
        }

        private void Run()
        {
            port.WriteLine($"m:{baseX},{baseY}");
            CNCWaitOne();

            var k = (360.0 / slices) * (Math.PI / 180.0);

            cache.Clear();
            float minD = 10;
            float maxD = 0;

            while (keyIndex < this.dict.Keys.Count)
            {
                var key = this.dict.Keys.ElementAt(keyIndex++);
                var result = this.dict[key];
                var ds = new Dictionary<int, float>();
                cache.Add(key, ds);

                for (int i = 0; i < slices; i++)
                {
                    var a = k * i;
                    float lx = (float)(10000.0 * Math.Cos(a));
                    float ly = (float)(10000.0 * Math.Sin(a));

                    foreach (var sp in result)
                    {
                        foreach (var line in sp.Lines)
                        {
                            if (line.Lenght == 0)
                                continue;

                            var v1 = new Vector2(line.Start.X, line.Start.Z);
                            var v2 = new Vector2(line.End.X, line.End.Z);
                            var v3 = new Vector2();
                            var v4 = new Vector2(lx, ly);

                            var s = new Line(v1, v2);
                            var e = new Line(v3, v4);

                            if (s.K != e.K && juge(v1, v2, v3, v4))
                            {
                                var p = f(v1, v2, v3, v4);
                                var d = (float)Math.Round(Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2)) / 100.0, 2);

                                if (d < minD)
                                    minD = d;

                                if (d > maxD)
                                    maxD = d;

                                if (ds.ContainsKey(i))
                                {
                                    if (ds[i] < d)
                                        ds[i] = d;
                                }
                                else
                                {
                                    ds.Add(i, d);
                                }
                            }
                        }
                    }
                }
            }
            
            double current_dis = maxD;

            keyIndex = 0;
            var steps = M0(minD, maxD);

            File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + @"cnc.txt", steps.Select(m => m.ToString()).ToArray());

            port.WriteLine($"set_speed:1200");
            CNCWaitOne();

            foreach (var s in steps)
            {
                if (cancelTask)
                    return;

                var cmd = "ms:" + s.ToString();
                port.WriteLine(cmd);
                CNCWaitOne();
            }

            port.WriteLine($"m:{baseX},{baseY}");
        }

        private List<StepInfo> M0(double minD,double maxD)
        {
            var steps = new List<StepInfo>();
            double current_dis = maxD;

            while (current_dis >= minD)
            {
                keyIndex = 0;
                while (keyIndex < this.cache.Keys.Count)
                {
                    var key = this.cache.Keys.ElementAt(keyIndex++);
                    var ds = this.cache[key];

                    for (int i = 0; i < 200; i++)
                    {
                        if (!ds.ContainsKey(i))
                        {
                            var step = new StepInfo();
                            steps.Add(step);
                            continue;
                        }

                        //m:20,20,0,90
                        var x = baseX + Math.Round(key / 100.0, 2);
                        double y = 0;

                        if (current_dis > ds[i])
                        {
                            y = baseY - (dis - current_dis);
                        }
                        else
                        {
                            y = baseY - (dis - ds[i]);
                        }

                        var p = moveTo(x, y, 0, 90);
                        if (p != null)
                        {
                            var step = new StepInfo(p.Value.X, p.Value.Y, 1000);

                            var last = steps.LastOrDefault();

                            if (last != null && last.IsSamePosition(step))
                                last.Step++;
                            else
                                steps.Add(step);
                        }
                    }
                }

                current_dis = current_dis - 0.1;
            }

            return steps;
        }

        private List<StepInfo> M1(double minD, double maxD)
        {
            var steps = new List<StepInfo>();
            keyIndex = 0;

            while (keyIndex < this.cache.Keys.Count)
            {
                var key = this.cache.Keys.ElementAt(keyIndex++);
                var ds = this.cache[key];

                for (int i = 0; i < 200; i++)
                {
                    if (!ds.ContainsKey(i))
                    {
                        var step = new StepInfo();
                        steps.Add(step);
                        continue;
                    }

                    //m:20,20,0,90
                    var x = baseX + Math.Round(key / 100.0, 2);
                    double y = baseY - (dis - ds[i]);

                    var p = moveTo(x, y, 0, 90);
                    if (p != null)
                    {
                        var step = new StepInfo(p.Value.X, p.Value.Y, 1000);

                        var last = steps.LastOrDefault();

                        if (last != null && last.IsSamePosition(step))
                            last.Step++;
                        else
                            steps.Add(step);
                    }
                }
            }

            return steps;
        }

        private void CNCWaitOne()
        {
            cnc_run = true;
            idleResetEvent.Reset();
            idleResetEvent.WaitOne();
            cnc_run = false;
        }

        Point? moveTo(double x, double y, double z, double angle)
        {
            double za = angle + 90.0;

            //新x2坐标
            double x2 = x + MEARM_MIN_R * Math.Cos((angle - 180) * DEG_TO_RAD);
            double y2 = y + MEARM_MIN_R * Math.Sin((angle - 180) * DEG_TO_RAD);

            //新x1坐标
            //x0到x2距离
            double d = Math.Sqrt(Math.Pow(x2, 2.0) + Math.Pow(y2, 2.0));
            if (d / 2.0 / MEARM_R > 1 || y2 / d > 1)
                return null; // 无效坐标

            double c1 = Math.Acos((d / 2.0) / MEARM_R) * RAD_TO_DEG;
            double c2 = Math.Acos(y2 / d) * RAD_TO_DEG;

            if (x2 <= 0)
                c2 = Math.Abs(c2) + 90.0;
            else
                c2 = 90.0 - c2;

            //sprintf(msg, "d:%.2f,c1:%.2f,c2:%.2f", d, c1,c2);
            //Serial.println(msg);

            //新x轴角度
            double xa = c1 + c2;

            //新x1坐标
            double x1 = MEARM_R * Math.Cos(xa * DEG_TO_RAD);
            double y1 = MEARM_R * Math.Sin(xa * DEG_TO_RAD);

            //sprintf(msg, "new p1:%.2f,%.2f", x1, y1);
            //Serial.println(msg);

            double ya = xa + (180.0 - 2.0 * c1);

            /*char msg[100];
            sprintf(msg, "new angle xa:%.2f,ya:%.2f,za:%.2f,a_angle:%.2f", xa, ya, za,0.0);
            Serial.println(msg);
            Serial.println();*/

            Point p = new Point();
            p.X = getAngleStep(xa);
            p.Y = getAngleStep(ya);

            return p;
        }

        int getAngleStep(double angle)
        {
            double s = (210.0 - angle) / 3.0 * 100;
            return Convert.ToInt32(Math.Round(s));
        }

        public void SetEvent()
        {
            if (cnc_run)
                idleResetEvent.Set();
        }

        private double ff(Vector2 p1, Vector2 p2, Vector2 p3)//判断点p3是否在直线p1p2上
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;

            double temp = dx * (p3.Y - p1.Y) - dy * (p3.X - p1.X);

            return temp;
        }

        private bool juge(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)//判断线段p1p2,p3p4是否有交点
        {
            if (ff(p1, p2, p3) * ff(p1, p2, p4) <= 0 && ff(p3, p4, p1) * ff(p3, p4, p2) <= 0)
            {

                return true;
            }
            else
                return false;
        }

        public Vector2 f(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)//求两直线的交点
        {
            Vector2 result = new Vector2();

            double left, right;

            left = (p2.Y - p1.Y) * (p4.X - p3.X) - (p4.Y - p3.Y) * (p2.X - p1.X);

            right = (p3.Y - p1.Y) * (p2.X - p1.X) * (p4.X - p3.X) + (p2.Y - p1.Y) * (p4.X - p3.X) * p1.X - (p4.Y - p3.Y) * (p2.X - p1.X) * p3.X;

            result.X = (float)(right / left);

            left = (p2.X - p1.X) * (p4.Y - p3.Y) - (p4.X - p3.X) * (p2.Y - p1.Y);

            right = (p3.X - p1.X) * (p2.Y - p1.Y) * (p4.Y - p3.Y) + p1.Y * (p2.X - p1.X) * (p4.Y - p3.Y) - p3.Y * (p4.X - p3.X) * (p2.Y - p1.Y);

            result.Y = (float)(right / left);

            return result;

        }
    }
}
