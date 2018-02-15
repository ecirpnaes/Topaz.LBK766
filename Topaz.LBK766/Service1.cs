#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Topaz.LBK766.Properties;

#endregion

namespace Topaz.LBK766
{
    public partial class SigCapture : ServiceBase
    {
        private SigPlusNET2 sigPlusNET;
        private const short BUTTON_CLEAR = 2;
        private const short BUTTON_ACCEPT = 3;
        private Rectangle EntireWindow;
        private Rectangle SigWindow;
        private Rectangle InkableLcdWindow;
        private Rectangle InkableSigWindow;
        private Rectangle ClearButton;
        private Rectangle AcceptButton;
        private Font _fontRegular;
        private Font _fontLarger;

        public SigCapture()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var thread = new Thread(StartService);
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void StartService()
        {
            EntireWindow = new Rectangle(0, 0, 320, 240);
            SigWindow = new Rectangle(3, 150, 320, 90);
            InkableLcdWindow = new Rectangle(3, 178, 309, 51);
            InkableSigWindow = new Rectangle(5, 180, 318, 55);
            ClearButton = new Rectangle(32, 155, 33, 13);
            AcceptButton = new Rectangle(254, 155, 33, 13);
            _fontRegular = new Font("Arial", 10.0F, FontStyle.Regular);
            _fontLarger = new Font("Arial", 12.0F, FontStyle.Regular);

            sigPlusNET = new SigPlusNET2();
            sigPlusNET.CreateControl();
            sigPlusNET.PenUp += OnPenUp;

            sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Capture); // Enables tablet to access the COM or USB port to capture signatures or not
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Clear, EntireWindow); //Refresh entire LCD
            sigPlusNET.ClearTablet(); // Clear everything out of memory

            // add button hotpots on lcd
            sigPlusNET.KeyPadAddHotSpot(BUTTON_CLEAR, (short) Enums.CoordinateEnum.Lcd, ClearButton);
            sigPlusNET.KeyPadAddHotSpot(BUTTON_ACCEPT, (short) Enums.CoordinateEnum.Lcd, AcceptButton);

            // Set up LCD to retain text/graphics/ink
            sigPlusNET.SetLCDCaptureMode((int) LCDCaptureModes.LCDCapInk);

            // load bmp into background memory for display on lcd
            sigPlusNET.LCDSendGraphic((int) Enums.DestinationEnum.Background, (byte) Enums.LcdWriteStringEnum.WriteOpaque, 0, 0, new Bitmap(Resources.imgOverlay));
            //sigPlusNET.LCDSendGraphic((int)Enums.DestinationEnum.Background, (byte)Enums.LcdWriteStringEnum.WriteOpaque, 0, 0, new Bitmap(Resources.imgThanks));

            // bring stored background image to foreground
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, EntireWindow);

            // Define the Inkable sections, and set the table to capture mode
            sigPlusNET.LCDSetWindow(InkableLcdWindow);
            sigPlusNET.SetSigWindow((short) Enums.CoordinateEnum.Lcd, InkableSigWindow);
            sigPlusNET.SetLCDCaptureMode((int) LCDCaptureModes.LCDCapInk);

            Application.Run();
        }

        protected override void OnStop()
        {
            sigPlusNET.ClearTablet(); // Clear everything out of memory
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Clear, EntireWindow);
            Application.ExitThread();
        }

        private void OnPenUp(object sender, EventArgs e)
        {
            if ( ButtonWasPressed(BUTTON_CLEAR) )
            {
                ShowButtonPressed(ClearButton);
                sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
                sigPlusNET.ClearTablet();

                sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, SigWindow);
                sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
            }

            if ( ButtonWasPressed(BUTTON_ACCEPT) )
            {
                ShowButtonPressed(AcceptButton);
                sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);

                if ( HasSignature() )
                {
                    try
                    {
                        sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Clear, EntireWindow);
                        sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Disabled);

                        // Save the image
                        sigPlusNET.SetImageXSize(500);
                        sigPlusNET.SetImageYSize(150);
                        sigPlusNET.SetJustifyMode(5);
                        var image = sigPlusNET.GetSigImage();
                        image.Save(GetFileNameAndPath(), ImageFormat.Jpeg);
                        sigPlusNET.SetJustifyMode(0);

                        // Set the tablet back to capture state and show the uer the thank you image
                        sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Capture);                        
                        sigPlusNET.LCDSendGraphic((int)Enums.DestinationEnum.Foreground, (byte)Enums.LcdWriteStringEnum.WriteOpaque, 0, 0, new Bitmap(Resources.imgThanks));
                        Thread.Sleep(4500);

                        ResetTabletForSignature();
                    }
                    catch (Exception ex)
                    {
                        // Set the tablet back to capture state and send a response string to the user.
                        sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Capture);
                        sigPlusNET.LCDWriteString(0, 2, 10, 10, _fontRegular, "Error: " + ex.Message);
                        Thread.Sleep(9000);

                        ResetTabletForSignature();
                    }
                }
                else
                {
                    sigPlusNET.LCDWriteString(0, 2, 46, 186, _fontLarger, "Please Sign Before Continuing...");
                    Thread.Sleep(2000);
                    sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, SigWindow);
                    sigPlusNET.SetLCDCaptureMode((int) LCDCaptureModes.LCDCapInk);
                }
            }

            sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
        }

        private void ResetTabletForSignature()
        {
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, SigWindow); //refresh lcd with background ONLY at bottom of LCD
            sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);

            //bring stored background image to foreground
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, EntireWindow);
            sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Capture);

            // clear cached image from background memory
            sigPlusNET.ClearTablet();
        }

        private bool ButtonWasPressed(short buttonKey)
        {
            return sigPlusNET.KeyPadQueryHotSpot(buttonKey) > 0;
        }

        public bool HasSignature()
        {
            return sigPlusNET.NumberOfTabletPoints() > 0;
        }

        private void ShowButtonPressed(Rectangle button)
        {
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Complement, button); //invert px at button location so user knows its been tapped
            Thread.Sleep(300);
        }

        private static string GetFileNameAndPath()
        {
            var folder = Util.GetValueFromConfigFile(Util.KEY_IMAGE_SAVE_PATH);
            if ( !folder.EndsWith("\\") ) folder += "\\";
            if ( !Directory.Exists(folder) ) Directory.CreateDirectory(folder);
            return folder + Util.GetValueFromConfigFile(Util.IMAGE_SAVE_FILENAME);
        }
    }
}