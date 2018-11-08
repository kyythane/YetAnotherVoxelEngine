namespace Assets.SunsetIsland.Common.Enums
{
    public enum LightChannel
    {
        Red = 0,
        Green,
        Blue,
        White, //White light is used to optimize lighting performance when RGB values are the same at the source.
    }
}