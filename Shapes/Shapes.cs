using System;
using System.Collections.Generic;
using ConsoleEngine;

namespace craw
{
    class Pixel
    {
        public Coord Coord;
        public CharItem Data;

        public Pixel(short x, short y, int unicode, short attr = 15)
        {
            Coord = new Coord(x, y);
            Data = new CharItem()
            {
                Char = { UnicodeChar = (char)unicode },
                Attributes = attr
            };
        }
    }

    public enum EShapeKind
    {
        Rectangle = 0,
        Triangle = 1,
        Circle = 2,
        Line = 3,
    }

    public enum EShapeState
    {
        Begin  = 0,
        State1 = 1,
        State2 = 2,
        State3 = 3,
        State4 = 4,
        State5 = 5,
        End    = 98,
        Error  = 99,
    }

    abstract class Shape
    {
        public Coord Start;
        public Pixel[] Pixels;
        public EShapeState State = EShapeState.Begin;

        public Shape(Coord start)
        {
            Start = start;
            Pixels = new Pixel[1];
            Pixels[0] = new Pixel(start.X, start.Y, 'X');
        }

        public abstract Shape Update(Coord current, EConsolePixelColor color);
        public abstract bool NextState(Coord stop);

        public void Put(ref ConsoleFrame frame)
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                Pixel p = Pixels[i];
                if (p != null)
                    frame[p.Coord].Set(p.Data.Character, p.Data.Color);
            }
        }
    }

    static class ShapeFunctions 
    {
        public static char GetAngleChar(double angle)
        {
            if (angle > 180)
                angle -= 180;

            const double inc = 22.5;
            if (angle >=  inc && angle <=  90 - inc || angle < -135 + inc && angle > -180 + inc)      return '╲';
            if (angle <= -inc && angle >= -90 + inc || angle >= 135 - inc && angle <= 180 - inc)      return '╱';
            if (angle >= 90 - inc && angle <= 135 - inc || angle <= -90 + inc && angle >= -135 + inc) return '│';
            return '─';
        }

        public static Pixel[] Circle(Coord center, Coord perim, EConsolePixelColor color)
        {
            int dist = (int)Math.Ceiling(center.DistanceTo(perim));

            List<Pixel> pixels = new();

            if (dist < 2)
                return pixels.ToArray();

            double iIncrement = dist < 5 ? 0.3d : 0.1d;
            
            int x0 = center.X;
            int y0 = center.Y;

            for (double i = 0.0d; i < Math.PI * 2; i += iIncrement)
            {
                int x1 = (int)(x0 + dist * Math.Cos(i));

                // Use twice the x value since the cells are 2:1
                x1 += x1;
                x1 -= x0;

                int y1 = (int)(y0 + dist * Math.Sin(i));

                pixels.Add(new((short)x1, (short)y1, GetAngleChar(i * 180 / Math.PI), (short)color));
            }

            return pixels.ToArray();
        }


        public static Pixel[] Line(Coord A, Coord B, EConsolePixelColor color)
        {
            int dist = (int)Math.Ceiling(A.DistanceTo(B));

            Pixel[] pixels = new Pixel[dist + 1];

            if (dist == 0)
                return pixels;

            int x0 = A.X; int x1 = B.X;
            int y0 = A.Y; int y1 = B.Y;

            double angle = A.AngleTo(B);
            char line = GetAngleChar(angle);

            // https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
            int dx =  Math.Abs(x1 - x0);
            int dy = -Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx + dy;  
            int c = 0;

            while (true) 
            {
                pixels[c++] = new Pixel((short)x0, (short)y0, line, (short)color);

                if (x0 == x1 && y0 == y1) 
                    return pixels;
                    
                int e2 = 2*err;

                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}
