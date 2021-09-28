using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ConsoleEngine
{
    /// <summary>
    /// Enumerates all possible values for `Attributes`, which define a Console color
    /// </summary>
    public enum EConsolePixelColor
    {
        Black = 0,
        DarkBlue = 1,
        DarkGreen = 2,
        SkyBlue = 3,
        Red = 4,
        Violet = 5,
        Orange = 6,
        LightGray = 7,
        Gray = 8,
        RoyalBlue = 9,
        Green = 10,
        Turquoise = 11,
        Salmon = 12,
        Magenta = 13,
        LightYellow = 14,
        White = 15,
    }

    /// <summary>
    /// Defines a Cartesian coordinate structure with a sequential Memorylayout.
    /// Required for the p/invoke <code>WriteConsoleOutput</code> call.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X;
        public short Y;

        /// <summary>
        /// Constructs a new Coord
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public Coord(short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Calculates the linear distance between this and <code>Coord B </code>
        /// </summary>
        /// <param name="B"></param>
        /// <returns>Distance to point B</returns>
        public double DistanceTo(Coord B) 
        {
            return Math.Sqrt( 
                Math.Pow(B.X - X, 2d) 
                + 
                Math.Pow(B.Y - Y, 2d) 
            );
        }

        /// <summary>
        /// Returns angle between this and <code>Coord B</code> in degrees.
        /// </summary>
        /// <param name="B"></param>
        /// <returns>Angle in degrees</returns>
        public double AngleTo(Coord B)
        {
            double radians = Math.Atan2(B.Y - Y, B.X - X);
            return radians * 180 / Math.PI;
        }
    };

    /// <summary>
    /// Defines a Character Structure, utilizing byte-overlay to store 
    /// Unicode and ASCII Characters
    /// </summary> 
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CharUnion
    {
        [FieldOffset(0)] public char UnicodeChar;
        [FieldOffset(0)] public byte AsciiChar;
    }

    /// <summary>
    /// Defines a Console "Pixel" Structure. Consists of 4 Bytes - The first two for the <code>CharUnion</code>
    /// Character, the second two for the Attributes. Required for the p/invoke <code>WriteConsoleOutput</code> call.
    /// </summary> 
    /// 
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CharItem
    {
        [FieldOffset(0)] public CharUnion Char;
        [FieldOffset(2)] public short Attributes;

        /// <summary>
        /// Property to directly access the Unicode Character
        /// </summary>
        /// <value>A Unicode character</value>
        public char Character
        {
            get { return Char.UnicodeChar; }
            set { Char.UnicodeChar = value; }
        }

        /// <summary>
        /// Property to directly access the EConsolePixelColor Attribute
        /// </summary>
        /// <value>A Color (Casts to short)</value>
        public EConsolePixelColor Color
        {
            get { return (EConsolePixelColor)Attributes; }
            set { Attributes = (short)value; }
        }

        /// <summary>
        /// Sets this to the desired values
        /// </summary>
        /// <param name="unicode">A Unicode Character</param>
        /// <param name="color">A Color (Casts to short)</param>
        public void Set(char unicode, EConsolePixelColor color)
        {
            Character = unicode;
            Color = color;
        }
    }

    /// <summary>
    /// A set of shorts, which define the Write Region within the Console Window.
    /// Values must fit within the current Console Window size.
    /// </summary>
    /// <remarks>
    /// - Left &amp; Top are usually set to 0.<br/>
    /// - Right is usually set to the Width of the Console Window.<br/>
    /// - Bottom is usually set to the Height of the Console Window.<br/>
    /// </remarks> 
    [StructLayout(LayoutKind.Sequential)]
    public struct WriteRegion
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    /// <summary>
    /// Describes a single Frame, displayed within the Console Window.
    /// </summary>
    class ConsoleFrame
    {
        public CharItem[] Buf;
        protected short Width = 100;
        protected short Height = 50;

        /// <summary>
        /// Constructs a new <code>ConsoleFrame</code>.
        /// </summary>
        /// <param name="width">Width value, which is <= current Console Window Width.</param>
        /// <param name="height">Height value, which is <= current Console Window Height.</param>
        public ConsoleFrame(short width, short height)
        {
            Width = width;
            Height = height;
            Buf = new CharItem[Width * Height];
        }

        /// <summary>
        /// Inserts string <code>s</code>, beginning at position <code>x</code>, <code>y</code>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="s"></param>
        /// <param name="color"></param>
        /// <param name="vertical">Toggle horizontal / vertical insertion</param>
        public void InsertString(int x, int y, string s, EConsolePixelColor color = EConsolePixelColor.White, bool vertical = false)
        {
            char[] chars = s.ToCharArray();


            if (vertical) 
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    if (y + i >= Height) break; // Exit if overflow
                    this[x, y + i].Set(chars[i], color);
                }
            }
            else 
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    if (x + i >= Width) break; // Exit if overflow
                    this[x + i, y].Set(chars[i], color);
                }
            }
        }

        /// <summary>
        /// Array Indexer for Cartesian Coordinates
        /// </summary>
        /// <value>Returns a reference to a CharItem</value>
        public ref CharItem this[int x, int y]
        {
            get { return ref Buf[(x - 1) + Width * (y - 1)]; }
        }

        /// <summary>
        /// Array Indexer for <code>Coord</code> structures
        /// </summary>
        /// <value>Returns a reference to a CharItem</value>
        public ref CharItem this[Coord coord]
        {
            get { return ref Buf[((short)coord.Y - 1) * Width + ((short)coord.X - 1)]; }
        }
    }

    /// <summary>
    /// Main Console Adapter class. It retrieves a File Handle to the stdout ($CONOUT)
    /// Using the <code>WriteConsoleOutput</code> p/invoke Command, it then writes a Buffer or Frame to 
    /// the Console.
    /// Basically, it emulates <code>fprintf()</code>
    /// </summary>
    class ConsoleEngine
    {
        /// <summary>
        /// P/invoke to GetStdHandle
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/console/getstdhandle"/>
        /// <param name="nStdHandle">Handle Value, -10 = stdin, -11 = stdout, -12 = stderr</param>
        /// <returns>Handle to STD stream</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle GetStdHandle(int nStdHandle);

        /// <summary>
        /// P/invoke to WriteConsoleOutput
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/console/writeconsoleoutput"/>
        /// <param name="hConsoleOutput">Handle to STD stream</param>
        /// <param name="lpBuffer">Char Buffer to write to the console</param>
        /// <param name="dwBufferSize">Coordinate with the Width and Height of the Buffer</param>
        /// <param name="dwBufferCoord">Coordinate to offset the Buffer Top / Left</param>
        /// <param name="lpWriteRegion">Reference to a WriteRegion</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharItem[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref WriteRegion lpWriteRegion);

        public short Width = 100;
        public short Height = 50;
        private SafeFileHandle Handle;
        private WriteRegion Region;

        /// <summary>
        /// Constructs a new <code>ConsoleEngine</code>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ConsoleEngine(short width, short height)
        {
            Width = width;
            Height = height;
            Handle = GetStdHandle(-11);

            if (Handle.IsInvalid)
                throw new Exception("Invalid Handle to $CONOUT - is a console instance running?");

            Region = new WriteRegion()
            {
                Left = 0,
                Top = 0,
                Right = Width,
                Bottom = Height
            };
        }

        /// <summary>
        /// Creates a new <code>ConsoleFrame</code> with the Console Engines Width / Height Dimensions
        /// </summary>
        /// <returns>A new ConsoleFrame</returns>
        public ConsoleFrame CreateFrame()
        {
            return new ConsoleFrame(Width, Height);
        }

        /// <summary>
        /// Writes a <code>ConsoleFrame</code> to the Console
        /// </summary>
        /// <param name="frame">The <code>ConsoleFrame</code> to write</param>
        /// <returns>Nonzero, if successful. Zero, if it fails </returns>
        public bool Write(ConsoleFrame frame)
        {
            return WriteConsoleOutput(Handle, frame.Buf,
                new Coord() { X = Width, Y = Height },
                new Coord() { X = 0, Y = 0 },
                ref Region);
        }
    }
}
