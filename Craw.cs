using System;
using System.Collections.Generic;
using System.Threading;
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
    private const char CursorChar = '◌'; //˟○◌⌂
    private const char SelectorChar = '⌂'; //◊∆¯^⌂
    private Shape ActiveShape = null;
    private EConsolePixelColor ActiveColor = EConsolePixelColor.White;

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

    private void PutUI(ref ConsoleFrame frame)
    {
      // ϽЯ∆ꟿ ϽЯ∆Ш ꞆꞦѦꟿ ₡ƦȺШ ᴄᴚᴀᴡ - ϽЯᴧ₡₳∆ꞦꟿꭗףּɄШΔѦȺƦ
      // ↕↔←↑→↓∞ ███████████████

      // Draw UI Outline
      frame.InsertString(1, 1, "╔ᴄᴚᴀᴡ══════════════════════════════════════════╗", EConsolePixelColor.White);
      frame.InsertString(1, 4, "╚══════════════════════════════════════════════╝", EConsolePixelColor.White);
      frame.InsertString(19, 2, "<Z>: Undo | <U>: Redo", EConsolePixelColor.White);
      frame.InsertString(19, 3, "<C>: Switch Color", EConsolePixelColor.White);

      frame[1, 2].Set('║', EConsolePixelColor.White);
      frame[1, 3].Set('║', EConsolePixelColor.White);
      frame[48, 2].Set('║', EConsolePixelColor.White);
      frame[48, 3].Set('║', EConsolePixelColor.White);

      // Draw Color swatches
      for (int i = 0; i < Enum.GetNames(typeof(EConsolePixelColor)).Length; i++)
        frame[2 + i, 2].Set('█', (EConsolePixelColor)i);

      // Draw Color Selector
      frame[2 + (int)ActiveColor, 3].Set(SelectorChar, EConsolePixelColor.White);

      // Draw Cursor
      frame[CursorPos].Set(CursorChar, EConsolePixelColor.White);
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
              case ConsoleKey.C: ChangeColor(); break;
              case ConsoleKey.Z: UndoShape(); break;
              case ConsoleKey.U: RedoShape(); break;
              default: break;
            }
          }

          Render();
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
        Shapes.Add(ActiveShape);
        ActiveShape = null;
      }
      else
      {
        ActiveShape = new Rectangle(CursorPos);
      }
    }

    private void ChangeColor()
    {
      int currentColor = (int)ActiveColor;
      currentColor += 1;
      currentColor %= Enum.GetNames(typeof(EConsolePixelColor)).Length;
      ActiveColor = (EConsolePixelColor)currentColor;
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

      // Fill frame buffer with Data from all Recipes - implicitly prioritizing the
      // higher indices
      Shapes.ForEach(recipe => recipe.Put(ref frame));

      // Fill frame buffer with Data from the Active shape
      if (ActiveShape != null)
      {
        ActiveShape.Make(CursorPos, ActiveColor); // Update Active Shape with current Cursor Position
        ActiveShape.Put(ref frame);
      }

      // Fill frame buffer with Data from UI Layer
      PutUI(ref frame);

      Engine.Write(frame);
    }
  }
}
