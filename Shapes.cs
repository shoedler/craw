using System.Collections.Generic;
using ConsoleEngine;

namespace craw
{
  abstract class Shape
  {
    public Coord Start;
    public Pixel[] Pixels;
    public abstract Shape Make(Coord stop, EConsolePixelColor color);

    public Shape(Coord start)
    {
      Start = start;
      Pixels = new Pixel[1];
      Pixels[0] = new Pixel(start.X, start.Y, 'X');
    }

    public void Put(ref ConsoleFrame frame)
    {
      for (int i = 0; i < Pixels.Length; i++)
      {
        Pixel instruction = Pixels[i];
        if (instruction != null)
        {
          frame[instruction.Coord].Set(instruction.Data.Character, instruction.Data.Color);
        }
      }
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
  }

  class Rectangle : Shape
  {
    public Rectangle(Coord start) : base(start)
    {
    }

    public override Shape Make(Coord stop, EConsolePixelColor color)
    {
      // Find top left and bottom right coordinates of the current Rectangle (Start to stop)
      short xLeft = Start.X < stop.X ? Start.X : stop.X;
      short xRight = stop.X > Start.X ? stop.X : Start.X;
      short yTop = Start.Y < stop.Y ? Start.Y : stop.Y;
      short yBottom = stop.Y > Start.Y ? stop.Y : Start.Y;

      short width = (short)(xRight - xLeft);
      short height = (short)(yBottom - yTop);

      if (width < 1 || height < 1) return null;

      Pixels = new Pixel[(width + 1) * (height + 1)];
      int commandCounter = 0;

      for (short x = xLeft; x <= xRight; x++)
      {
        for (short y = yTop; y <= yBottom; y++)
        {
          if (x == xLeft)
          {
            Pixels[commandCounter++] = (y == yTop)    ? new Pixel(x, y, /*48*/ 0x250c, (short)color)  /*  */
                                     : (y == yBottom) ? new Pixel(x, y, /*49*/ 0x2514, (short)color)  /* └ */
                                     :                  new Pixel(x, y, /*50*/ 0x2502, (short)color); /* │ */
          }
          else if (x == xRight)
          {
            Pixels[commandCounter++] = (y == yTop)    ? new Pixel(x, y, /*51*/ 0x2510, (short)color)  /* ┐ */
                                     : (y == yBottom) ? new Pixel(x, y, /*52*/ 0x2518, (short)color)  /* ┘ */
                                     :                  new Pixel(x, y, /*53*/ 0x2502, (short)color); /* │ */
          }
          else if (y == yTop || y == yBottom)
            Pixels[commandCounter++] = new Pixel(x, y, /*54*/ 0x2500, (short)color); /* ─ */
        }
      }

      return this;
    }
  }
}
