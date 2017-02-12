namespace Minesweeper
{
    /// <summary>
    /// Simple data type that holds information about one location in a mine field
    /// </summary>
    public struct Cell
    {
        /// <summary>
        /// Represents the danger status of this Cell
        /// </summary>
        public bool IsMine
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the number of neighbors containing a mine
        /// <summary>
        public int NeighboringMines
        {
            get;
            set;
        }

        /// <summary>
        /// Represents whether or not the player has planted a flag on this Cell
        /// </summary>
        public bool Flagged
        {
            get;
            set;
        }

        /// <summary>
        /// Represents whether or not the player has cleared this Cell as not containing a mine
        /// <summary>
        public bool Cleared
        {
            get;
            set;
        }
    }
}
