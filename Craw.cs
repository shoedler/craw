using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEngine;

namespace craw
{
  class CrawController
  {
    public ConsoleEngine.ConsoleEngine Engine;

    public bool ShouldRun = false;
    private List<Shape> Recipes = new List<Shape>();

    private Coord CursorPos;
    private char CursorChar = '⌂'; //˟˹
    private Shape ActiveShape = null;

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

    public List<Pixel> UI()
    {
      //  COMBAK: Active Color Switcher (Key pgup / pgdwn) & display in GUI
      // ϽЯ∆ꟿ ꞆꞦѦꟿ ₡ƦȺШ ᴄᴚᴀᴡ
      //"ϽЯᴧ₡₳∆ꞦꟿꭗףּɄШΔѦȺƦ",
      //"↕↔←↑→↓∞"
      // ███████████████

      ConsoleFrame frame = Engine.CreateFrame();

      frame[1, 2].Set('║', EConsolePixelColor.White);

      List<Pixel> pixels = new List<Pixel>();

      pixels.AddRange(Pixel.FromString("╔ᴄᴚᴀᴡ══════════════════════════════════════════╗", 1, 1));
      pixels.Add(new Pixel(1, 2, '║', 15));

      for (int i = 0; i < 16; i++)
        pixels.Add(new Pixel((short)(2 + i), 2, '█', (short)i));

      pixels.AddRange(Pixel.FromString("                               ║", 17, 2));
      pixels.AddRange(Pixel.FromString("╚══════════════════════════════════════════════╝", 1, 3));

      return pixels;
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
            switch (action.Key)
            {
              case ConsoleKey.LeftArrow: MoveCursor(-1, 0); break;
              case ConsoleKey.RightArrow: MoveCursor(1, 0); break;
              case ConsoleKey.UpArrow: MoveCursor(0, -1); break;
              case ConsoleKey.DownArrow: MoveCursor(0, 1); break;
              case ConsoleKey.Spacebar: CursorAction(); break;
              default: break;
            }
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

    private void CursorAction()
    {
      if (ActiveShape != null)
      {
        Recipes.Add(ActiveShape);
        ActiveShape = null;
      }
      else
      {
        ActiveShape = new Rectangle(CursorPos);
      }
    }

    private void Draw()
    {
      ConsoleFrame frame = Engine.CreateFrame();
      var uiLayer = new List<Pixel>();

      uiLayer.AddRange(UI());

      // Fill frame buffer with Data from all Recipes - implicitly prioritizing the
      // higher indices
      Recipes.ForEach(recipe =>
      {
        for (int i = 0; i < recipe.Pixels.Length; i++)
        {
          Pixel instruction = recipe.Pixels[i];
          if (instruction != null)
          {
            frame[instruction.Coord].Set(instruction.Data.Character, instruction.Data.Color);
          }
        }
      });

      // Fill frame buffer with Data from UI Layer - prioritizing the UI Layer over the Data in 'Recipies'
      uiLayer.ForEach(instruction =>
      {
        frame[instruction.Coord].Set(instruction.Data.Character, instruction.Data.Color);
      });

      // Fill frame buffer with Data from the Active shape
      if (ActiveShape != null)
      {
        for (int i = 0; i < ActiveShape.Pixels.Length; i++)
        {
          Pixel instruction = ActiveShape.Pixels[i];
          if (instruction != null)
          {
            frame[instruction.Coord].Set(instruction.Data.Character, instruction.Data.Color);
          }
        }

        // Update Active Shape with current Cursor Position
        ActiveShape.Make(CursorPos);
      }

      // Draw Cursor
      frame[CursorPos].Set(CursorChar, EConsolePixelColor.White);

      Engine.Write(frame);
    }
  }
}
