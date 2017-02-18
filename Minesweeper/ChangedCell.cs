namespace Minesweeper
{
    /// <summary>
    /// Represents a Cell that had its status changed
    /// </summary>
    /// <remarks>Where <see cref="Cell.Flagged"/> or <see cref="Cell.Cleared"/> changed</remarks>
    public struct ChangedCell
    {
        public ChangedCell(Coordinate coord, int mines)
        {
            coordinate = coord;
            neighboringMines = mines;
        }

        public Coordinate coordinate;
        public int neighboringMines;
    }
}
