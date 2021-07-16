using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEngine;

namespace craw
{
    abstract class PlotRecipe
    {
        public List<PlotInstruction> Instructions;
        public abstract List<PlotInstruction> Bake();
    }


    class PlotInstruction
    {
        public Coord Coord;
        public CharItem Data;

        public PlotInstruction(short x, short y, int unicode, short attr = 15)
        {
            Coord = new Coord(x, y);
            Data = new CharItem()
            {
                Char = 
                {
                    UnicodeChar = (char)unicode
                },
                Attributes = attr
            };
        }

        public PlotInstruction(short x, short y, byte ascii, short attr = 15)
        {
            Coord = new Coord(x, y);
            Data = new CharItem()
            {
                Char =
                {
                    AsciiChar = ascii
                },
                Attributes = attr
            };
        }

        public static List<PlotInstruction> FromString(string s, short x, short y, short attr = 15)
        {
            List<PlotInstruction> instructions = new List<PlotInstruction>();
            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                instructions.Add(new PlotInstruction((short)(x + i), y, chars[i], attr));
            }

            return instructions;
        }
    }

    class CrawController
    {
        public ConsoleEngine.ConsoleEngine engine;

        public bool ShouldRun = false;
        private List<PlotRecipe> Recipes = new List<PlotRecipe>();

        private Coord CursorPos;
        private char CursorChar = '⌂'; //˟˹

        public CrawController()
        {
            short width = (short)(150);
            short height = (short)(50);

            CursorPos = new Coord((short)(width / 2), (short)(height / 2));

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            Console.CursorVisible = false;


            engine = new ConsoleEngine.ConsoleEngine(width, height);
        }

        public void MoveCursor(short xOffset, short yOffset)
        {
            bool outsideXBounds = CursorPos.X + xOffset < 1 || CursorPos.X + xOffset > engine.Width - 1;
            bool outsideYBounds = CursorPos.Y + yOffset < 1 || CursorPos.Y + yOffset > engine.Height - 1;
            if (!outsideXBounds && !outsideYBounds)
            {
                CursorPos.X += xOffset;
                CursorPos.Y += yOffset;
            }
        }

        public List<PlotInstruction> GetUI()
        {
            //  COMBAK: Active Color Switcher (Key pgup / pgdwn) & display in GUI
            string[] data = new string[5]
            {
                 "╔ᴄᴚᴀᴡ════════════════════════════════════╗",// ϽЯ∆ꟿ ꞆꞦѦꟿ ₡ƦȺШ ᴄᴚᴀᴡ
                $"║                                        ║",
                 "╚════════════════════════════════════════╝",
                "ϽЯᴧ₡₳∆ꞦꟿꭗףּɄШΔѦȺƦ",
                "↕↔←↑→↓∞"
            };

            List<PlotInstruction> instructions = new List<PlotInstruction>();
            for (int i = 0; i < 5; i++)
            {
                instructions.AddRange(PlotInstruction.FromString(data[i], 1, (short)(i + 1)));
            }
            return instructions;
        }

        public void Run()
        {
            while (ShouldRun)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        var action = Console.ReadKey();
                        Execute(action);
                    }

                    Draw();
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


        private void Execute(ConsoleKeyInfo action)
        {
            switch (action.Key)
            {
                case ConsoleKey.LeftArrow:  MoveCursor(-1,  0); break;
                case ConsoleKey.RightArrow: MoveCursor( 1,  0); break;
                case ConsoleKey.UpArrow:    MoveCursor( 0, -1); break;
                case ConsoleKey.DownArrow:  MoveCursor( 0,  1); break;
                default: break;
            }
        }

        private void Draw()
        {
            CharItem[] frame = engine.GetEmptyBuf();
            var uiLayer = new List<PlotInstruction>();

            uiLayer.AddRange(GetUI());

            // Fill frame buffer with Data from all Recipes - implicitly prioritizing the
            // higher indices
            Recipes.ForEach(recipe =>
            {
                recipe.Instructions.ForEach(instruction =>
                {
                    short index = engine.GetBufIndex(instruction.Coord);
                    frame[index] = instruction.Data;
                });
            });

            // Fill frame buffer with Data from UI Layer - prioritizing the UI Layer over the Data in 'Recipies'
            uiLayer.ForEach(instruction =>
            {
                short index = engine.GetBufIndex(instruction.Coord);
                frame[index] = instruction.Data;
            });

            // Draw Cursor
            short cursorIndex = engine.GetBufIndex(CursorPos);
            frame[cursorIndex].Char.UnicodeChar = CursorChar;
            frame[cursorIndex].Attributes = 15;

            engine.WriteBuf(frame);
        }
    }
}
