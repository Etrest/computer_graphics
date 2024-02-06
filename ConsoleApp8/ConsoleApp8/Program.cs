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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            int[,] currentPolygon = polygons[currentPolygonIndex];
            DrawPolygon(graphics, currentPolygon, Color.Blue);
        }

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
