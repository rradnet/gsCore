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

        double MachineBedOriginFactorX { get; }
        double MachineBedOriginFactorY { get; }

        IProfile Clone();
    }
}
