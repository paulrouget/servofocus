namespace Servofocus
{
    public class ClickMessage
    {
        public uint X { get; }
        public uint Y { get; }

        public ClickMessage(uint x, uint y)
        {
            X = x;
            Y = y;
        }
    }
}