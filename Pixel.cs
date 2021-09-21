using System.Collections.Generic;
using ConsoleEngine;

namespace craw
{
    abstract class Shape
    {
        public Pixel[] Pixels;
        public abstract Shape Make(Coord stop);

        public Shape()
        {
            Pixels = new Pixel[1];
        }
    }

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

        public Pixel(short x, short y, byte ascii, short attr = 15)
        {
            Coord = new Coord(x, y);
            Data = new CharItem()
            {
                Char = { AsciiChar = ascii },
                Attributes = attr
            };
        }

        public static List<Pixel> FromString(string s, short x, short y, short attr = 15)
        {
            List<Pixel> pixels = new List<Pixel>();
            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                pixels.Add(new Pixel((short)(x + i), y, chars[i], attr));
            }

            return pixels;
        }
    }

    class Rectangle : Shape
    {
        public Coord Start;

        public Rectangle(Coord _start) : base()
        {
            Start = _start;
            Pixels[0] = new Pixel(_start.X, _start.Y, ' ');
        }

        public override Shape Make(Coord stop)
        {
            short xLeft   = Start.X < stop.X  ? Start.X : stop.X;
            short xRight  = stop.X  > Start.X ? stop.X  : Start.X;
            short yTop    = Start.Y < stop.Y  ? Start.Y : stop.Y;
            short yBottom = stop.Y  > Start.Y ? stop.Y  : Start.Y;

            short width =  (short)(xRight - xLeft);
            short height = (short)(yBottom - yTop);

            if (width < 2 || height < 2) return null;

            Pixel[] commands = new Pixel[(width + 1) * (height + 1)];
            int commandCounter = 0;

            for (short x = xLeft; x <= xRight; x++)
            {
                for (short y = yTop; y <= yBottom; y++)
                {
                    if (x == xLeft)
                    {
                        commands[commandCounter++] = (y == yTop)    ? new Pixel(x, y, /*48*/ 0x250c) /* ┌ */
                                                   : (y == yBottom) ? new Pixel(x, y, /*49*/ 0x2514) /* └ */
                                                   :                  new Pixel(x, y, /*50*/ 0x2502); /* │ */
                    }
                    else if (x == xRight)
                    {
                        commands[commandCounter++] = (y == yTop)    ? new Pixel(x, y, /*51*/ 0x2510) /* ┐ */
                                                   : (y == yBottom) ? new Pixel(x, y, /*52*/ 0x2518) /* ┘ */
                                                   :                  new Pixel(x, y, /*53*/ 0x2502); /* │ */
                    }
                    else if (y == yTop || y == yBottom)
                        commands[commandCounter++] = new Pixel(x, y, /*54*/ 0x2500); /* ─ */
                }
            }

            Pixels = commands;
            return this;
        }
    }
}
