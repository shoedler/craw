using ConsoleEngine;

namespace craw
{
    class Circle : Shape
    {
        private Coord Middle;
        public Circle(Coord start) : base(start) { }

        public override bool NextState(Coord stop)
        {
            if (State == EShapeState.End)
                return true;

            if (State == EShapeState.Begin)
                State = EShapeState.End;

            return false;
        }

        public override Shape Update(Coord current, EConsolePixelColor color)
        {
            if (State == EShapeState.Begin)
            {
                Middle = current;
                Pixels = ShapeFunctions.Circle(Start, current, color);
            }

            return this;
        }
    }

}