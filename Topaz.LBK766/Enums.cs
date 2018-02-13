namespace Topaz.LBK766
{
    public static class Enums
    {
        public enum LcdWriteStringEnum
        {
            Clear = 0,
            Complement = 1,
            WriteOpaque = 2,
            WriteTransparent = 3
        }

        public enum LcdRefreshModeEnum
        {
            Clear = 0,
            Complement = 1,
            WriteOpaque = 2,
            WriteTransparent = 4
        }

        public enum DestinationEnum
        {
            Foreground = 0,
            Background = 1
        }

        public enum TabletStateEnum
        {
            Disabled = 0,
            Capture = 1
        }

        public enum ClearSigEnum
        {
            InsideSigWindow = 0,
            OutsideSigWindow = 1
        }
    }
}