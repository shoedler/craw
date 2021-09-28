using System;
using System.Diagnostics;
using System.Collections.Generic;
using ConsoleEngine;

namespace craw
{
    class CrawController
    {
        public ConsoleEngine.ConsoleEngine Engine;
        public bool ShouldRun = false;
        private List<Shape> Shapes = new List<Shape>();
        private List<Shape> ShapeHistory = new List<Shape>();
        private Coord CursorPos;
        private Shape ActiveShape = null;
        private EConsolePixelColor ActiveColor = EConsolePixelColor.White;
        private EShapeKind ActiveShapeKind = EShapeKind.Rectangle;
        private float Fps = 0f;
        private double FrameTimeMs = 0;
        private bool ToggleGrid = false;
        // Config Values
        private const char CursorChar = '◌'; //˟○◌⌂
        private const char SelectorChar = '⌂'; //◊∆¯^⌂
        private const string UiTitle = "ᴄᴚᴀᴡ";
        private const EConsolePixelColor UiColor = EConsolePixelColor.White;

        // Combak
        // Title alternatives: ϽЯ∆ꟿ ϽЯ∆Ш ꞆꞦѦꟿ ₡ƦȺШ ᴄᴚᴀᴡ - ϽЯᴧ₡₳∆ꞦꟿꭗףּɄШΔѦȺƦ
        // Chars:              ↕↔←↑→↓∞ █
        // TODO: Implement key [x] -> x, followed by numbers and then enter moves the cursor to the given x pos
        // TODO: Implement key [y] -> y, followed by numbers and then enter moves the cursor to the given y pos
        // TODO: Implement
        
        public CrawController()
        {
            short width = (short)(150);
            short height = (short)(50);

            CursorPos = new Coord((short)(width / 2), (short)(height / 2));

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            Console.CursorVisible = false;

            Engine = new ConsoleEngine.ConsoleEngine(width, height);
        }

        public void MoveCursor(short xOffset, short yOffset)
        {
            bool outsideXBounds = CursorPos.X + xOffset < 1 || CursorPos.X + xOffset > Engine.Width - 1;
            bool outsideYBounds = CursorPos.Y + yOffset < 1 || CursorPos.Y + yOffset > Engine.Height - 1;
            if (!outsideXBounds && !outsideYBounds)
            {
                CursorPos.X += xOffset;
                CursorPos.Y += yOffset;
            }
        }

        private void PutGrid(ref ConsoleFrame frame)
        {
            for (int x = 1; x < Engine.Width; x++) 
            {
                for (int y = 1; y < Engine.Height; y++)
                {
                    if (x % 2 == 0)
                        frame[x, y].Set('─', EConsolePixelColor.Gray);
                    else
                        frame[x, y].Set('┼', EConsolePixelColor.Gray);
                }
            }
        }

        private void PutUI(ref ConsoleFrame frame)
        {
            int verticalHeight = 3;
            int colorCount = Enum.GetNames(typeof(EConsolePixelColor)).Length;
            int shapeKindCount = Enum.GetNames(typeof(EShapeKind)).Length;

            string horizontalBar = new string('═', Engine.Width - 3);
            string verticalBar = new string('║', verticalHeight);
            string verticalSpacer = '╤' + new string('│', verticalHeight) + '╧';

            // Draw main border
            frame.InsertString(1, 1, '╔' + horizontalBar + '╗', UiColor);
            frame.InsertString(1, verticalHeight + 2, '╚' + horizontalBar + '╝', UiColor);
            frame.InsertString(1, 2, verticalBar, UiColor, true);
            frame.InsertString(Engine.Width - 1, 2, verticalBar, UiColor, true);

            frame.InsertString(2, 1, UiTitle, UiColor);

            /*
             * From here on, the vertical sections are drawn from left to right.
             * `ySectionBegin` and `ySectionEnd` are incremented according to the width of the sections content
             */
            int ySectionBegin = 0;
            int ySectionEnd = 0;

            /*
             * Color Section
             */
            ySectionBegin = 2;
            ySectionEnd = ySectionBegin + colorCount + 2;

            // Draw Section title, Spacer and selector
            frame.InsertString(ySectionBegin, 2, "[C]olors", UiColor);
            frame.InsertString(ySectionEnd - 1, 1, verticalSpacer, UiColor, true);
            frame[ySectionBegin + (int)ActiveColor, 4].Set(SelectorChar, EConsolePixelColor.White);

            // Draw Color swatches
            for (int i = 0; i < colorCount; i++)
                frame[ySectionBegin + i, 3].Set('█', (EConsolePixelColor)i);

            /*
             * Shape Section
             */
            ySectionBegin = ySectionEnd + 1;
            ySectionEnd = ySectionBegin + 8 + 2; //TODO: Change '8' to shapeKindCount once there are enough implemented

            // Draw Section title, Spacer and selector
            frame.InsertString(ySectionBegin, 2, "[S]hapes", UiColor);
            frame.InsertString(ySectionEnd - 1, 1, verticalSpacer, UiColor, true);
            frame[ySectionBegin + (int)ActiveShapeKind, 4].Set(SelectorChar, EConsolePixelColor.White);
            
            // Draw Shape kinds
            for (int i = 0; i < shapeKindCount; i++) 
            {
                char s = (i == (int)EShapeKind.Rectangle) ? '□' :
                         (i == (int)EShapeKind.Triangle) ? '∆' :
                         (i == (int)EShapeKind.Line) ? '-' : 
                         (i == (int)EShapeKind.Circle) ? '○' :
                         '?';
                frame[ySectionBegin + i, 3].Set(s, UiColor);
            }

            /*
             * Keymapping Section
             */
            ySectionBegin = ySectionEnd + 1;
            ySectionEnd = ySectionBegin + 26 + 2;

            // Draw text and spacer 
            frame.InsertString(ySectionEnd - 1, 1, verticalSpacer, UiColor, true);
            frame.InsertString(ySectionBegin, 2, "[Z]: Undo / [U]: Redo", UiColor);
            frame.InsertString(ySectionBegin, 3, "[G]: Toggle grid");
            frame.InsertString(ySectionBegin, 4, "[Space]: Begin / End Shape");

            /* 
             * Info Section
             */
            ySectionBegin = ySectionEnd + 1;
            frame.InsertString(ySectionBegin, 2, "fps: " + Fps.ToString() , UiColor);
            frame.InsertString(ySectionBegin, 3, "frametime [ms]: " + ((float)FrameTimeMs).ToString() , UiColor);


            // Draw Cursor
            frame[CursorPos].Set(CursorChar, EConsolePixelColor.White);
            frame.InsertString(CursorPos.X + 1, CursorPos.Y + 1, CursorPos.X.ToString() + "/" + CursorPos.Y.ToString(), EConsolePixelColor.Gray);
        }

        public void Run()
        {
            Stopwatch sw = new Stopwatch();
            const int fpsFreqMs = 100; // Recalculate fps every `fpsFreqMs` miliseconds
            double fpsAccumulator = 0d;

            while (ShouldRun)
            {
                try
                {
                    sw.Restart();

                    if (Console.KeyAvailable)
                    {
                        var action = Console.ReadKey();
                        switch (action.Key)
                        {
                            case ConsoleKey.LeftArrow: MoveCursor(-1, 0); break;
                            case ConsoleKey.RightArrow: MoveCursor(1, 0); break;
                            case ConsoleKey.UpArrow: MoveCursor(0, -1); break;
                            case ConsoleKey.DownArrow: MoveCursor(0, 1); break;
                            case ConsoleKey.Spacebar: CursorAction(); break;
                            case ConsoleKey.C: ChangeColor(); break;
                            case ConsoleKey.S: ChangeShapeKind(); break;
                            case ConsoleKey.Z: UndoShape(); break;
                            case ConsoleKey.U: RedoShape(); break;
                            case ConsoleKey.G: ToggleGrid = !ToggleGrid; break;
                            default: break;
                        }
                    }

                    Render();

                    FrameTimeMs = sw.Elapsed.TotalMilliseconds;
                    fpsAccumulator += FrameTimeMs;

                    if (fpsAccumulator > fpsFreqMs) 
                    {
                        Fps = 1000 * (float) fpsAccumulator / (float) fpsFreqMs;
                        fpsAccumulator = 0;
                    }
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine("An Error occured during Runtime");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("\n");
                    Console.WriteLine("Press any key to restart...");
                    Console.ReadLine();
                }
            }
        }

        private void CursorAction()
        {
            // Create / Finish ActiveShape
            if (ActiveShape != null)
            {
                ActiveShape.NextState(CursorPos);
                if (ActiveShape.State == EShapeState.End)
                {
                    Shapes.Add(ActiveShape);
                    ActiveShape = null;
                }
            }
            else
            {
                ActiveShape = (ActiveShapeKind == EShapeKind.Triangle) ? new Triangle(CursorPos) :
                              (ActiveShapeKind == EShapeKind.Rectangle) ? new Rectangle(CursorPos) :
                              (ActiveShapeKind == EShapeKind.Circle) ? new Triangle(CursorPos) : // 😁
                              (ActiveShapeKind == EShapeKind.Line) ? new Line(CursorPos) :
                              new Rectangle(CursorPos);
            }
        }

        private void ChangeColor()
        {
            int currentColor = (int)ActiveColor;
            currentColor += 1;
            currentColor %= Enum.GetNames(typeof(EConsolePixelColor)).Length;
            ActiveColor = (EConsolePixelColor)currentColor;
        }

        private void ChangeShapeKind()
        {
            int currentShapeKind = (int)ActiveShapeKind;
            currentShapeKind++;
            currentShapeKind %= Enum.GetNames(typeof(EShapeKind)).Length;
            ActiveShapeKind = (EShapeKind)currentShapeKind;
        }

        private void UndoShape()
        {
            if (Shapes.Count > 0)
            {
                Shape s = Shapes[Shapes.Count - 1];
                Shapes.Remove(s);
                ShapeHistory.Add(s);
            }
        }

        private void RedoShape()
        {
            if (ShapeHistory.Count > 0)
            {
                Shape s = ShapeHistory[ShapeHistory.Count - 1];
                ShapeHistory.Remove(s);
                Shapes.Add(s);
            }
        }

        private void Render()
        {
            ConsoleFrame frame = Engine.CreateFrame();

            // Draw the grid first, if it's toggled on
            if (ToggleGrid)
            {
                PutGrid(ref frame);
            }

            // Fill frame buffer with Data from all Recipes - implicitly prioritizing the
            // higher indices
            Shapes.ForEach(recipe => recipe.Put(ref frame));

            // Fill frame buffer with Data from the Active shape
            if (ActiveShape != null)
            {
                ActiveShape.Update(CursorPos, ActiveColor); // Update Active Shape with current Cursor Position
                ActiveShape.Put(ref frame);
            }

            // Fill frame buffer with Data from UI Layer
            PutUI(ref frame);

            Engine.Write(frame);
        }
    }
}
