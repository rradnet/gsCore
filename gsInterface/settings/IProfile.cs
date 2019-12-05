namespace gs.interfaces
{
    public interface IProfile
    {
        string ManufacturerName { get; }
        string ModelIdentifier { get; }
        string ProfileName { get; }

        double MachineBedSizeXMM { get; }
        double MachineBedSizeYMM { get; }
        double MachineBedSizeZMM { get; }

        double MachineBedOffsetX { get; }
        double MachineBedOffsetY { get; }

        IProfile Clone();
    }
}
