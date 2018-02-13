namespace Topaz.LBK766
{
    public class SigPlusNET2 : SigPlusNET
    {
        public void KeyPadAddHotSpot(short keyCode, short coordToUse, Rectangle rectangle)
        {
            base.KeyPadAddHotSpot(keyCode, coordToUse, (short) rectangle.XPos, (short) rectangle.YPos, (short) rectangle.XSize, (short) rectangle.YSize);
        }

        public bool LCDRefresh(byte mode, Rectangle rectangle)
        {
            return base.LCDRefresh(mode, rectangle.XPos, rectangle.YPos, rectangle.XSize, rectangle.YSize);
        }

        public bool LCDSetWindow(Rectangle rectangle)
        {
            return base.LCDSetWindow(rectangle.XPos, rectangle.YPos, rectangle.XSize, rectangle.YSize);
        }

        public void SetSigWindow(short coords, Rectangle rectangle)
        {
            base.SetSigWindow(coords, (short) rectangle.XPos, (short) rectangle.YPos, (short) rectangle.XSize, (short) rectangle.YSize);
        }
    }

    public class Rectangle
    {
        private int _xPos;
        private int _yPos;
        private int _xSize;
        private int _ySize;

        public Rectangle(int xPos, int yPos, int xSize, int ySize)
        {
            XPos = xPos;
            YPos = yPos;
            XSize = xSize;
            YSize = ySize;
        }

        public int XPos
        {
            get { return _xPos; }
            set { _xPos = value; }
        }

        public int YPos
        {
            get { return _yPos; }
            set { _yPos = value; }
        }

        public int XSize
        {
            get { return _xSize; }
            set { _xSize = value; }
        }

        public int YSize
        {
            get { return _ySize; }
            set { _ySize = value; }
        }
    }
}