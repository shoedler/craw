using System;
using craw;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleEngine
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new CrawController();
            app.ShouldRun = true;
            app.Run();
            //short width = (short)(Console.LargestWindowWidth * 0.7);
            //short height = (short)(Console.LargestWindowHeight * 0.7);

            //Console.SetWindowSize(width, height);
            //Console.SetBufferSize(width, height);
            //ConsoleEngine ce = new ConsoleEngine(width, height);

            //#region Draw Test

            //CharItem[] canvas = ce.GetEmptyBuf();
            //Rectangle r = null;

            //Rectangle[] rectangles = new Rectangle[10];
            //int rectangleCounter = 0;

            //bool isDrawing = false;

            //for (; ; )
            //{
            //    var p = Console.GetCursorPosition();
            //    Coord cursorPos = new Coord((short)p.Left, (short)p.Top);

            //    if (Console.KeyAvailable)
            //    {
            //        ConsoleKeyInfo key = Console.ReadKey(true);

            //        switch (key.Key)
            //        {
            //            case ConsoleKey.Enter:
            //                if (!isDrawing)
            //                {
            //                    r = new Rectangle(cursorPos);
            //                }
            //                else
            //                {
            //                    if (r != null)
            //                        rectangles[rectangleCounter++] = r;
            //                }
            //                isDrawing = !isDrawing;
            //                break;
            //            case ConsoleKey.LeftArrow:
            //                Console.SetCursorPosition(p.Left - 1, p.Top);
            //                break;
            //            case ConsoleKey.RightArrow:
            //                Console.SetCursorPosition(p.Left + 1, p.Top);
            //                break;
            //            case ConsoleKey.DownArrow:
            //                Console.SetCursorPosition(p.Left, p.Top + 1);
            //                break;
            //            case ConsoleKey.UpArrow:
            //                Console.SetCursorPosition(p.Left, p.Top - 1);
            //                break;
            //        }
            //    }

            //    canvas = ce.GetEmptyBuf();

            //    // Draw currently drawing Rectangle
            //    if (isDrawing)
            //    {
            //        var cmds = r.Make(cursorPos);
            //        if (cmds != null)
            //        {
            //            for (int i = 0; i < cmds.Length; i++)
            //            {
            //                DrawCommand cmd = cmds[i];
            //                if (cmd != null && !cmd.Empty)
            //                {
            //                    short index = ce.GetBufIndex(cmd.Coord.X, cmd.Coord.Y);
            //                    canvas[index] = cmd.Data;
            //                }
            //            }
            //        }
            //    }

            //    // DraW "old" Rectangles
            //    for (int i = 0; i < rectangleCounter; i++)
            //    {
            //        Rectangle r2 = rectangles[i];
            //        for (int j = 0; j < r2.Blueprint.Length; j++)
            //        {
            //            DrawCommand cmd = r2.Blueprint[j];
            //            if (cmd != null && !cmd.Empty)
            //            {
            //                short index = ce.GetBufIndex(cmd.Coord.X, cmd.Coord.Y);
            //                canvas[index] = cmd.Data;
            //            }
            //        }
            //    }


            //    ce.WriteBuf(canvas);

            //}
            //#endregion
        }
    }

    class Rectangle
    {
        public Coord Start;
        public DrawCommand[] Blueprint;

        public Rectangle(Coord _start)
        {
            Start = _start;
            Blueprint = new DrawCommand[1];
        }

        public DrawCommand[] Make(Coord stop)
        {
            short xLeft =   Start.X < stop.X ? Start.X : stop.X;
            short xRight =  stop.X > Start.X ? stop.X : Start.X;
            short yTop =    Start.Y < stop.Y ? Start.Y : stop.Y;
            short yBottom = stop.Y > Start.Y ? stop.Y : Start.Y;

            short width =  (short)(xRight - xLeft);
            short height = (short)(yBottom - yTop);

            if (width < 2 || height < 2) return null;

            DrawCommand[] commands = new DrawCommand[(width + 1) * (height + 1)];
            int commandCounter = 0;

            for (short x = xLeft; x <= xRight; x++)
            {
                for (short y = yTop; y <= yBottom; y++)
                {
                    if (x == xLeft)
                    {
                        if (y == yTop)         commands[commandCounter++] = new DrawCommand(x, y, /*48*/ 0x250c); /* ┌ */
                        else if (y == yBottom) commands[commandCounter++] = new DrawCommand(x, y, /*49*/ 0x2514); /* └ */
                        else                   commands[commandCounter++] = new DrawCommand(x, y, /*50*/ 0x2502); /* │ */
                    }
                    else if (x == xRight)
                    {
                        if (y == yTop)         commands[commandCounter++] = new DrawCommand(x, y, /*51*/ 0x2510); /* ┐ */
                        else if (y == yBottom) commands[commandCounter++] = new DrawCommand(x, y, /*52*/ 0x2518); /* ┘ */
                        else                   commands[commandCounter++] = new DrawCommand(x, y, /*53*/ 0x2502); /* │ */
                    }
                    else if (y == yTop || y == yBottom)
                        commands[commandCounter++] = new DrawCommand(x, y, /*54*/ 0x2500); /* ─ */
                }
            }

            Blueprint = commands;
            return commands;
        }
    }

    class DrawCommand
    {
        public Coord Coord;
        public CharItem Data;
        public bool Empty = true;

        public DrawCommand(short x, short y, int unicode)
        {
            Empty = false;
            Coord = new Coord(x, y);
            Data = new CharItem()
            {
                Char = {
                    UnicodeChar = (char)unicode
                },
                Attributes = 5
            };
        }
    }
}