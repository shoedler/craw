using ConsoleEngine;

namespace craw
{
class Line : Shape
    {
        public Line(Coord start) : base(start) { }

        public override bool NextState(Coord stop)
        {
            if (State == EShapeState.End)
                return true;

            State = EShapeState.End;
            return false;
        }

        public override Shape Update(Coord current, EConsolePixelColor color)
        {
            Pixels = ShapeFunctions.Line(Start, current, color);
            return this;
        }
    }
}