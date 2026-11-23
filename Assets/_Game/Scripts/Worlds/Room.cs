namespace Worlds
{
    public abstract class Room
    {
        public int Index { get; }

        protected Room(int index)
        {
            Index = index;
        }
    }
}
