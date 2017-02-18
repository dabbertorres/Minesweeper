namespace Minesweeper
{
    /// <summary>
    /// Convenience data type for passing coordinates around
    /// </summary>
    public struct Coordinate
    {
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x;
        public int y;
    }
}
