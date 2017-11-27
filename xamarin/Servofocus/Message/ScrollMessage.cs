using ServoSharp;

namespace Servofocus
{
    public class ScrollMessage
    {
        public int Dx { get; }
        public int Dy { get; }
        public uint X { get; }
        public uint Y { get; }
        public ScrollState State { get; }

        public ScrollMessage(int dx, int dy, uint x, uint y, ScrollState state)
        {
            Dx = dx;
            Dy = dy;
            X = x;
            Y = y;
            State = state;
        }
    }
}