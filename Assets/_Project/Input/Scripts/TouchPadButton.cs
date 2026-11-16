namespace BlockDiggerz.Input
{
    /// <summary>
    /// Buttons of the on-screen <see cref="TouchPadDevice"/>. The value is the bit of the button in
    /// <see cref="TouchPadState.Buttons"/>.
    /// </summary>
    public enum TouchPadButton
    {
        MoveLeft = 0,
        MoveRight = 1,
        HardDrop = 2,
        RotateClockwise = 3,
        RotateCounterClockwise = 4,
        StorePiece = 5,
        Pause = 6
    }
}
