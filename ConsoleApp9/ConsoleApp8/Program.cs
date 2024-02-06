using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ConsoleApp8
{
    class Program : Form
    {
        private List<int[,]> polygons;
        private int currentPolygonIndex;
        private Timer timer;
        private int timerInterval = 100; // Интервал в миллисекундах

        private Timer clippingTimer;
        private List<int[,]> clippedPolygons;
        private int currentClippedPolygonIndex;
        private int[,] clippingWindow1 = { { 0, 0 }, { 100, 0 }, { 100, 50 }, { 0, 50 } };
        private int[,] clippingWindow2 = { { 0, 35 }, { 85, 35 }, { 85, 15 }, { 0, 15 } };

        

        public Program()
        {
            polygons = new List<int[,]>();

            int[,] initialPolygon = { { 0, 0 }, { 100, 0 }, { 50, 100 } }; // Исходный многоугольник (треугольник)
            int[,] finalPolygon = { { 0, 0 }, { 100, 0 }, { 100, 100 }, { 0, 100 }, { 50, 50 }, { 100, 100 } }; // Конечный многоугольник (шестиугольник)
            int steps = 30; // Количество шагов анимации

            polygons = MorphPolygons(initialPolygon, finalPolygon, steps, 4, "PR2");

            currentPolygonIndex = 0;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            timer = new Timer();
            timer.Interval = timerInterval;
            timer.Tick += Timer_Tick;
            timer.Start();


            clippedPolygons = ClipPolygons(polygons, clippingWindow1);
            clippedPolygons = ClipPolygons(clippedPolygons, clippingWindow2);

            currentClippedPolygonIndex = 0;

            clippingTimer = new Timer();
            clippingTimer.Interval = timerInterval;
            clippingTimer.Tick += ClippingTimer_Tick;
            clippingTimer.Start();
        }
        private void ClippingTimer_Tick(object sender, EventArgs e)
        {
            currentClippedPolygonIndex++;
            if (currentClippedPolygonIndex >= clippedPolygons.Count)
            {
                currentClippedPolygonIndex = 0;
            }

            Invalidate(); // Перерисовываем окно
        }

        private List<int[,]> ClipPolygons(List<int[,]> polygons, int[,] window)
        {

            List<int[,]> clippedPolygons = new List<int[,]>();

            foreach (int[,] polygon in polygons)
            {
                List<int[]> clippedPolygon = new List<int[]>();

                for (int i = 0; i < polygon.GetLength(0); i++)
                {
                    int x1 = polygon[i, 0];
                    int y1 = polygon[i, 1];
                    int x2 = polygon[(i + 1) % polygon.GetLength(0), 0];
                    int y2 = polygon[(i + 1) % polygon.GetLength(0), 1];

                    // Классифицируем каждую точку относительно окна
                    int code1 = ComputeOutCode(x1, y1, window);
                    int code2 = ComputeOutCode(x2, y2, window);

                    if ((code1 | code2) == 0)
                    {
                        // Оба конца внутри окна; отрезок полностью видим
                        clippedPolygon.Add(new int[] { x1, y1 });
                        clippedPolygon.Add(new int[] { x2, y2 });
                    }
                    else if ((code1 & code2) == 0)
                    {
                        // Отрезок пересекает границу окна
                        if (code1 != 0)
                        {
                            // Находим точку пересечения
                            int[] intersection = ComputeIntersection(x1, y1, x2, y2, window, code1);
                            clippedPolygon.Add(intersection);
                            clippedPolygon.Add(new int[] { x2, y2 });
                        }
                        else
                        {
                            // Находим точку пересечения
                            int[] intersection = ComputeIntersection(x1, y1, x2, y2, window, code2);
                            clippedPolygon.Add(new int[] { x1, y1 });
                            clippedPolygon.Add(intersection);
                        }
                    }
                    // В противном случае отрезок полностью вне окна и игнорируется
                }

                if (clippedPolygon.Count > 0)
                {
                    int[,] clippedPolygonArray = new int[clippedPolygon.Count, 2];
                    for (int i = 0; i < clippedPolygon.Count; i++)
                    {
                        clippedPolygonArray[i, 0] = clippedPolygon[i][0];
                        clippedPolygonArray[i, 1] = clippedPolygon[i][1];
                    }
                    clippedPolygons.Add(clippedPolygonArray);
                }
            }

            return clippedPolygons;
        }
        private int ComputeOutCode(int x, int y, int[,] window)
        {
            int code = 0;

            if (x < window[0, 0])
                code |= 1; // Слева
            else if (x > window[2, 0])
                code |= 2; // Справа
            if (y < window[0, 1])
                code |= 4; // Сверху
            else if (y > window[2, 1])
                code |= 8; // Снизу

            return code;
        }

        private int[] ComputeIntersection(int x1, int y1, int x2, int y2, int[,] window, int outcode)
        {
            if ((outcode & 1) != 0) // Слева
            {
                int x = window[0, 0];
                int y = y1 + (y2 - y1) * (x - x1) / (x2 - x1);
                return new int[] { x, y };
            }
            else if ((outcode & 2) != 0) // Справа
            {
                int x = window[2, 0];
                int y = y1 + (y2 - y1) * (x - x1) / (x2 - x1);
                return new int[] { x, y };
            }
            else if ((outcode & 4) != 0) // Сверху
            {
                int y = window[0, 1];
                int x = x1 + (x2 - x1) * (y - y1) / (y2 - y1);
                return new int[] { x, y };
            }
            else if ((outcode & 8) != 0) // Снизу
            {
                int y = window[2, 1];
                int x = x1 + (x2 - x1) * (y - y1) / (y2 - y1);
                return new int[] { x, y };
            }

            return null; // Не должно сюда попасть
        }














        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            int[,] currentClippedPolygon = clippedPolygons[currentClippedPolygonIndex];
            DrawPolygon(graphics, currentClippedPolygon, Color.Blue);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            currentPolygonIndex++;
            if (currentPolygonIndex >= polygons.Count)
            {
                currentPolygonIndex = 0;
            }

            Invalidate(); // Перерисовываем окно
        }

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    base.OnPaint(e);

        //    Graphics graphics = e.Graphics;

        //    int[,] currentPolygon = polygons[currentPolygonIndex];
        //    DrawPolygon(graphics, currentPolygon, Color.Blue);
        //}

        private void DrawPolygon(Graphics graphics, int[,] polygon, Color color)
        {
            int pointsCount = polygon.GetLength(0);
            Point[] points = new Point[pointsCount];

            for (int i = 0; i < pointsCount; i++)
            {
                int x = polygon[i, 0];
                int y = polygon[i, 1];
                points[i] = new Point(x, y);
            }

            Brush brush = new SolidBrush(color);
            graphics.FillPolygon(brush, points);
        }

        //private List<int[,]> MorphPolygons(int[,] initialPolygon, int[,] finalPolygon, int steps, int connectionScheme, string barrier)
        //{
        //    int initialPointsCount = initialPolygon.GetLength(0);
        //    int finalPointsCount = finalPolygon.GetLength(0);
        //    int maxPointsCount = Math.Max(initialPointsCount, finalPointsCount);

        //    List<int[,]> intermediatePolygons = new List<int[,]>();

        //    for (int step = 0; step <= steps; step++)
        //    {
        //        double interpolation = (double)step / steps;

        //        int[,] intermediatePolygon = new int[maxPointsCount, 2];

        //        for (int i = 0; i < maxPointsCount; i++)
        //        {
        //            int initialX = initialPolygon[i % initialPointsCount, 0];
        //            int initialY = initialPolygon[i % initialPointsCount, 1];
        //            int finalX = finalPolygon[i % finalPointsCount, 0];
        //            int finalY = finalPolygon[i % finalPointsCount, 1];

        //            int interpolatedX = (int)(initialX + interpolation * (finalX - initialX));
        //            int interpolatedY = (int)(initialY + interpolation * (finalY - initialY));

        //            intermediatePolygon[i, 0] = interpolatedX;
        //            intermediatePolygon[i, 1] = interpolatedY;
        //        }

        //        intermediatePolygons.Add(intermediatePolygon);
        //    }

        //    return intermediatePolygons;
        //}

        private List<int[,]> MorphPolygons(int[,] initialPolygon, int[,] finalPolygon, int steps, int connectionScheme, string barrier)
        {
            int initialPointsCount = initialPolygon.GetLength(0);
            int finalPointsCount = finalPolygon.GetLength(0);

            // Если количество вершин не совпадает, добавляем дополнительные вершины
            if (initialPointsCount < finalPointsCount)
            {
                int[,] newInitialPolygon = new int[finalPointsCount, 2];
                for (int i = 0; i < finalPointsCount; i++)
                {
                    newInitialPolygon[i, 0] = initialPolygon[i % initialPointsCount, 0];
                    newInitialPolygon[i, 1] = initialPolygon[i % initialPointsCount, 1];
                }
                initialPolygon = newInitialPolygon;
            }
            else if (finalPointsCount < initialPointsCount)
            {
                int[,] newFinalPolygon = new int[initialPointsCount, 2];
                for (int i = 0; i < initialPointsCount; i++)
                {
                    newFinalPolygon[i, 0] = finalPolygon[i % finalPointsCount, 0];
                    newFinalPolygon[i, 1] = finalPolygon[i % finalPointsCount, 1];
                }
                finalPolygon = newFinalPolygon;
            }

            int maxPointsCount = Math.Max(initialPolygon.GetLength(0), finalPolygon.GetLength(0));

            List<int[,]> intermediatePolygons = new List<int[,]>();

            for (int step = 0; step <= steps; step++)
            {
                double interpolation = (double)step / steps;

                int[,] intermediatePolygon = new int[maxPointsCount, 2];

                for (int i = 0; i < maxPointsCount; i++)
                {
                    int initialX = initialPolygon[i, 0];
                    int initialY = initialPolygon[i, 1];
                    int finalX = finalPolygon[i, 0];
                    int finalY = finalPolygon[i, 1];

                    int interpolatedX = (int)(initialX + interpolation * (finalX - initialX));
                    int interpolatedY = (int)(initialY + interpolation * (finalY - initialY));

                    intermediatePolygon[i, 0] = interpolatedX;
                    intermediatePolygon[i, 1] = interpolatedY;
                }

                intermediatePolygons.Add(intermediatePolygon);
            }

            return intermediatePolygons;
        }


        public static int[] AddAdditionalPoints(int[] polygon, int count)
        {
            List<int> polygonList = new List<int>(polygon);

            for (int i = 0; i < count * 2; i++)
            {
                polygonList.Add(0); // Добавляем дополнительные точки со значением 0
            }

            return polygonList.ToArray();
        }

        public static void Main()
        {
            Application.Run(new Program());
        }


    }
}

