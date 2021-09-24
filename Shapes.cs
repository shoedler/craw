using System;
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

    class ShapeFunctions 
    {
        public static Pixel[] Line(Coord A, Coord B, EConsolePixelColor color)
        {
            int dist = (int)Math.Ceiling(A.DistanceTo(B));

            Pixel[] pixels = new Pixel[dist + 1];

            if (dist == 0)
                return pixels;

            int x0 = A.X; int x1 = B.X;
            int y0 = A.Y; int y1 = B.Y;

            double angle = A.AngleTo(B);

            char line = '─';
            if (angle >=  22.5 && angle <=  67.5 || angle < -112.5 && angle >  -157.5) line = '╲';
            if (angle >=  67.5 && angle <= 112.5 || angle <= -67.5 && angle >= -112.5) line = '│';
            if (angle <= -22.5 && angle >= -67.5 || angle >= 112.5 && angle <=  157.5) line = '╱';


            // https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
            int dx =  Math.Abs(x1-x0);
            int sx = x0<x1 ? 1 : -1;
            int dy = -Math.Abs(y1-y0);
            int sy = y0<y1 ? 1 : -1;
            int err = dx+dy;  
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

    class Triangle : Shape
    {
        private Coord Middle;
        public Triangle(Coord start) : base(start) { }
        
        public override bool NextState(Coord stop)
        {
            if (State == EShapeState.End)
                return true;

            if (State == EShapeState.State1)
                State = EShapeState.End;

            if (State == EShapeState.Begin)
                State = EShapeState.State1;

            return false;
        }

        public override Shape Update(Coord current, EConsolePixelColor color)
        {
            if (State == EShapeState.Begin)
            {
                Middle = current;
                Pixels = ShapeFunctions.Line(Start, current, color);
            }
            else if (State == EShapeState.State1)
            {
                Pixel[] line1 = ShapeFunctions.Line(Start,   Middle, color);
                Pixel[] line2 = ShapeFunctions.Line(current, Middle, color);
                Pixel[] line3 = ShapeFunctions.Line(current, Start,  color);

                Pixels = new Pixel[line1.Length + line2.Length + line3.Length + 3];

                for (int i = 0; i < line1.Length; i++) Pixels[i] = line1[i];
                for (int i = 0; i < line2.Length; i++) Pixels[i + line1.Length] = line2[i];
                for (int i = 0; i < line3.Length; i++) Pixels[i + line1.Length + line2.Length] = line3[i];
            }

            return this;
        }
    }

    class Rectangle : Shape
    {
        public Rectangle(Coord start) : base(start) { }

        public override bool NextState(Coord stop)
        {
            if (State == EShapeState.End)
                return true;

            State = EShapeState.End;
            return false;
        }

        public override Shape Update(Coord current, EConsolePixelColor color)
        {
            // Find top left and bottom right coordinates of the current Rectangle (Start to stop)
            short xLeft = Start.X < current.X ? Start.X : current.X;
            short xRight = current.X > Start.X ? current.X : Start.X;
            short yTop = Start.Y < current.Y ? Start.Y : current.Y;
            short yBottom = current.Y > Start.Y ? current.Y : Start.Y;

            int width = xRight - xLeft;
            int height = yBottom - yTop;

            if (width < 1 || height < 1) 
                return null;

            Pixels = new Pixel[(width + 1) * (height + 1)];

            int pixelCount = 0;
            char pixelChar = ' ';

            for (short x = xLeft; x <= xRight; x++)
            {
                for (short y = yTop; y <= yBottom; y++)
                {
                    if (x == xLeft)
                        pixelChar = (y == yTop) ? '┌' : (y == yBottom) ? '└' : '│';
                    else if (x == xRight)
                        pixelChar = (y == yTop) ? '┐' : (y == yBottom) ? '┘' : '│';
                    else if (y == yTop || y == yBottom)
                        pixelChar = '─';

                    Pixels[pixelCount++] = new Pixel(x, y, pixelChar, (short)color);
                }
            }

            return this;
        }
    }
}
