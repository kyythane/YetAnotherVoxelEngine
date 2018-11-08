using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SunsetIsland.Common
{
    public static class Colors
    {
        //565
        public const ushort HoneyDew = 0xF7FE;
        public const ushort Beige = 0xF7BB;
        public const ushort Smoke = 0xF7BE;
        public const ushort Ivory = 0xFFFE;
        public const ushort White = 0xFFFF;
        public const ushort LightGrey = 0xD69A;
        public const ushort MediumGrey = 0xAD55;
        public const ushort Grey = 0x8410;
        public const ushort DarkGrey = 0x6B4D;
        public const ushort ChaarcoalGrey = 0x4A4A;
        public const ushort LightSlate = 0x7453;
        public const ushort Slate = 0x7412;
        public const ushort DarkSlate = 0x2A69;
        public const ushort Silver = 0xC618;
        public const ushort Gainsboro = 0xDEFB;
        public const ushort Black = 0;
        public const ushort Red = 0xE0C3;
        public const ushort PinkishRed = 0xD512;
        public const ushort OrangeyRed = 0xF9C3;
        public const ushort Rouge = 0xA166;
        public const ushort Wine = 0x7927;
        public const ushort StrongPink = 0xF96F;
        public const ushort BubbleGumPink = 0xFCFB;
        public const ushort PinkPurple = 0xE0FE;
        public const ushort HotMagenta = 0xE078;
        public const ushort Magenta = 0xC8EF;
        public const ushort WarmPurple = 0x996F;
        public const ushort Burgandy = 0x4003;
        public const ushort NavyBlue = 0x008A;
        public const ushort BluePurple = 0x481D;
        public const ushort MediumBlue = 0x2B59;
        public const ushort Blue = 0x18DC;
        public const ushort Azure = 0x053D;
        public const ushort RobinsEgg = 0x6F5F;
        public const ushort BlueGreen = 0x0D13;
        public const ushort DarkAqua = 0x2B2D;
        public const ushort DarkForestGreen = 0x01A3;
        public const ushort GreyishPurple = 0x8BD4;
        public const ushort LightPeriwinkle = 0xB61F;
        public const ushort GreyishGreen = 0xADF3;
        public const ushort Green = 0x1F23;
        public const ushort Brown = 0x59C3;
        public const ushort Sienna = 0xA285;
        public const ushort Umber = 0xAB20;
        public const ushort DullOrange = 0xCC69;
        public const ushort Orange = 0xFD20;
        public const ushort DarkOrange = 0xFC60;
        public const ushort YellowishOrange = 0xF546;
        public const ushort Yellowish = 0xF74B;
        public const ushort Yellow = 0xFFE0;
        public const ushort DullYellow = 0xEF00;
        public const ushort Gold = 0xFEA0;
        public const ushort PeaSoup = 0x9CA0;
        public const ushort MudGreen = 0x5300;
        public const ushort Mint = 0x176E;
        public const ushort KellyGreen = 0x14A7;
        public const ushort MossyGreen = 0x6C44;
        public const ushort ToxicGreen = 0x5702;
        public const ushort BrightTeal = 0x0FF9;
        public const ushort Sapphire = 0x3995;
        public const ushort PaleViolet = 0xBD5F;
        public const ushort RosePink = 0xFCB3;
        public const ushort Topaz = 0x3618;
        public const ushort MudBrown = 0x6A64;
        public const ushort Tan = 0xD5B1;

        public static Dictionary<string, ushort> NamedColors = new Dictionary<string, ushort>
        {
            {"Azure", Azure},
            {"Beige", Beige},
            {"Black", Black},
            {"Blue", Blue},
            {"Blue-Green", BlueGreen},
            {"Blue-Purple", BluePurple},
            {"Bright Teal", BrightTeal},
            {"Brown", Brown},
            {"Bubblegum Pink", BubbleGumPink},
            {"Burgandy", Burgandy},
            {"Chaarcoal Grey", ChaarcoalGrey},
            {"Dark Aqua", DarkAqua},
            {"Dark Forest Green", DarkForestGreen},
            {"Dark Grey", DarkGrey},
            {"Dark Orange", DarkOrange},
            {"Dark Slate", DarkSlate},
            {"Dull Orange", DullOrange},
            {"Dull Yellow", DullYellow},
            {"Gainsboro", Gainsboro},
            {"Gold", Gold},
            {"Green", Green},
            {"Grey", Grey},
            {"Greyish-Green", GreyishGreen},
            {"Greyish-Purple", GreyishPurple},
            {"Honey Dew", HoneyDew},
            {"Hot Magenta", HotMagenta},
            {"Ivory", Ivory},
            {"Kelly Green", KellyGreen},
            {"Light Grey", LightGrey},
            {"Light Periwinkle", LightPeriwinkle},
            {"Light Slate", LightSlate},
            {"Magenta", Magenta},
            {"Medium Blue", MediumBlue},
            {"Medium Grey", MediumGrey},
            {"Mint", Mint},
            {"Mossy Green", MossyGreen},
            {"Mud Brown", MudBrown},
            {"Mud Green", MudGreen},
            {"Navy Blue", NavyBlue},
            {"Orange", Orange},
            {"Orangey-Red", OrangeyRed},
            {"Pale Violet", PaleViolet},
            {"Pea Soup", PeaSoup},
            {"Pinkish-Red", PinkishRed},
            {"Pink-Purple", PinkPurple},
            {"Red", Red},
            {"Robin's Egg", RobinsEgg},
            {"Rose Pink", RosePink},
            {"Rouge", Rouge},
            {"Sapphire", Sapphire},
            {"Sienna", Sienna},
            {"Silver", Silver},
            {"Slate", Slate},
            {"Smoke", Smoke},
            {"Strong Pink", StrongPink},
            {"Tan", Tan},
            {"Topaz", Topaz},
            {"Toxic Green", ToxicGreen},
            {"Umber", Umber},
            {"Warm Purple", WarmPurple},
            {"White", White},
            {"Wine", Wine},
            {"Yellow", Yellow},
            {"Yellowish", Yellowish},
            {"Yellowish-Orange", YellowishOrange}
        };

        public static Dictionary<ushort, string> NameLookup = new Dictionary<ushort, string>();

        public static ushort[] Palette =
        {
            Azure,
            Beige,
            Black,
            Blue,
            BlueGreen,
            BluePurple,
            BrightTeal,
            Brown,
            BubbleGumPink,
            Burgandy,
            ChaarcoalGrey,
            DarkAqua,
            DarkForestGreen,
            DarkGrey,
            DarkOrange,
            DarkSlate,
            DullOrange,
            DullYellow,
            Gainsboro,
            Gold,
            Green,
            Grey,
            GreyishGreen,
            GreyishPurple,
            HoneyDew,
            HotMagenta,
            Ivory,
            KellyGreen,
            LightGrey,
            LightPeriwinkle,
            LightSlate,
            Magenta,
            MediumBlue,
            MediumGrey,
            Mint,
            MossyGreen,
            MudBrown,
            MudGreen,
            NavyBlue,
            Orange,
            OrangeyRed,
            PaleViolet,
            PeaSoup,
            PinkishRed,
            PinkPurple,
            Red,
            RobinsEgg,
            RosePink,
            Rouge,
            Sapphire,
            Sienna,
            Silver,
            Slate,
            Smoke,
            StrongPink,
            Tan,
            Topaz,
            ToxicGreen,
            Umber,
            WarmPurple,
            White,
            Wine,
            Yellow,
            Yellowish,
            YellowishOrange
        };

        public static void Initiailize()
        {
            Array.Sort(Palette);
            foreach (var namedColor in NamedColors)
                NameLookup[namedColor.Value] = namedColor.Key;
        }

        public static ushort NextColor(this ushort current)
        {
            var paletteColor = current.ToPaletteColor();
            var index = Array.IndexOf(Palette, paletteColor);
            return Palette[(index + 1) % Palette.Length];
        }

        public static ushort PreviousColor(this ushort current)
        {
            var paletteColor = current.ToPaletteColor();
            var index = Array.IndexOf(Palette, paletteColor);
            return Palette[index == 0 ? Palette.Length - 1 : index - 1];
        }

        public static ushort ToPaletteColor(this ushort current)
        {
            return Palette.ClosestTo(current);
        }

        public static ushort Convert(this Color32 color)
        {
            var r = (color.r >> 3) & 31;
            var g = (color.g >> 2) & 63;
            var b = (color.b >> 3) & 31;
            return (ushort) ((r << 11) | (g << 5) | b);
        }

        public static ushort Convert(this Color color)
        {
            return ((Color32) color).Convert();
        }

        public static ushort ConvertColor(this Vector3 color)
        {
            var r = ((uint) color.x >> 3) & 31;
            var g = ((uint) color.y >> 2) & 63;
            var b = ((uint) color.z >> 3) & 31;
            return (ushort) ((r << 11) | (g << 5) | b);
        }

        public static Color32 Convert32(this ushort color)
        {
            var red = ((color >> 11) & 31) << 3;
            var green = ((color >> 5) & 63) << 2;
            var blue = (color & 31) << 3;
            return new Color32((byte) red, (byte) green, (byte) blue, 255);
        }

        public static Vector3 ConvertVector3(this ushort color)
        {
            var red = ((color >> 11) & 31) << 3;
            var green = ((color >> 5) & 63) << 2;
            var blue = (color & 31) << 3;
            return new Vector3(red, green, blue);
        }

        public static Color Convert(this ushort color)
        {
            var red = ((color >> 11) & 31);
            var green = ((color >> 5) & 63);
            var blue = (color & 31);
            return new Color(red / 31f, green / 63f, blue / 31f);
        }

        //If this method gets used a lot convert it to straight math instead of chained functions
        public static ushort Multiply(this ushort light, ref ushort color)
        {
            var floatColor = color.Convert();
            return light.Convert().Multiply(ref floatColor).Convert();
        }

        public static Color32 Multiply(this Color32 light, ref Color32 color32)
        {
            var color = (Color) color32;
            return ((Color) light).Multiply(ref color);
        }

        public static Color Multiply(this Color light, ref Color color)
        {
            var r = Mathf.Clamp01(light.r * color.r);
            var g = Mathf.Clamp01(light.g * color.g);
            var b = Mathf.Clamp01(light.b * color.b);
            return new Color(r, g, b);
        }
    }
}