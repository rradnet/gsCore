namespace gs.interfaces
{
    public interface IProfile
    {
        string ManufacturerName { get; set; }
        string ModelIdentifier { get; set; }
        string ProfileName { get; set; }
        string MaterialName { get; }

        double MachineBedSizeXMM { get; }
        double MachineBedSizeYMM { get; }
        double MachineBedSizeZMM { get; }

        double MachineBedOriginFactorX { get; }
        double MachineBedOriginFactorY { get; }

        IProfile Clone();
    }
}
