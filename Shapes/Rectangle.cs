using ConsoleEngine;

namespace craw
{
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
                    else
                        pixelChar = ' ';

                    Pixels[pixelCount++] = new Pixel(x, y, pixelChar, (short)color);
                }
            }

            return this;
        }
    }
}