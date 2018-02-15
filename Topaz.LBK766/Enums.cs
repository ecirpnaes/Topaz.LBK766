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
            Clear = 0, // display is cleared at the specified location
            Complement = 1, //complements display at the specified location
            WriteOpaque = 2, //transfers contents of the background memory to the LCD display, overwriting the content
            WriteTransparent = 4 //transfers contents of the background memory to the LCD display and combined in the contents of the LCD display
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

        public enum CoordinateEnum : short
        {
            Logical = 0,
            Lcd = 1
        }
    }
}