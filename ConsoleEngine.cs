using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ConsoleEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X;
        public short Y;

        public Coord(short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }
    };

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CharUnion
    {
        [FieldOffset(0)] public char UnicodeChar;
        [FieldOffset(0)] public byte AsciiChar;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CharItem
    {
        [FieldOffset(0)] public CharUnion Char;
        [FieldOffset(2)] public short Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WriteRegion
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    class ConsoleEngine
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle GetStdHandle(int nStdHandle);

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

        public CharItem[] GetEmptyBuf()
        {
            return new CharItem[Width * Height];
        }

        public bool WriteBuf(CharItem[] buf)
        {
            return WriteConsoleOutput(Handle, buf,
                new Coord() { X = Width, Y = Height },
                new Coord() { X = 0, Y = 0 },
                ref Region);
        }
        public bool WriteBuf(CharItem[] buf, WriteRegion region)
        {
            return WriteConsoleOutput(Handle, buf,
                new Coord() { X = Width, Y = Height },
                new Coord() { X = 0, Y = 0 },
                ref region);
        }

        public short GetBufIndex(short X, short Y)
        {
            return (short)((Y - 1) * Width + X);
        }
    }
}
