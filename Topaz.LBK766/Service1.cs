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
        private SigPlusNET sigPlusNET;
        private const short BUTTON_CLEAR = 2;
        private const short BUTTON_ACCEPT = 3;

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
            sigPlusNET = new SigPlusNET();
            sigPlusNET.CreateControl();
            sigPlusNET.PenUp += OnPenUp;

            // Enables tablet to access the COM or USB port to capture signatures or not
            //  State-setting to 1 enables the tablet to capture signatures as above, setting to 0 disables signature capture.
            sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Capture);
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Clear, 0, 0, 320, 240); //Refresh entire LCD
            sigPlusNET.ClearTablet(); //clears the SigPlus object

            //adds the hotpots on lcd
            sigPlusNET.KeyPadAddHotSpot(BUTTON_CLEAR, 1, 32, 155, 33, 13);
            sigPlusNET.KeyPadAddHotSpot(BUTTON_ACCEPT, 1, 258, 155, 20, 13);

            var img4x5 = new Bitmap(Resources.img4x5_new);
            sigPlusNET.SetLCDCaptureMode((int) LCDCaptureModes.LCDCapInk); //Sets up LCD to retain text/graphics/ink
            sigPlusNET.LCDSendGraphic((int) Enums.DestinationEnum.Background, (byte) Enums.LcdWriteStringEnum.WriteOpaque, 0, 0, img4x5); //load bmp into background memory for display on lcd

            //bring stored background image to foreground
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, 0, 0, 320, 240);

            sigPlusNET.LCDSetWindow(3, 178, 309, 51); //Permits only the section on lcd to ink
            sigPlusNET.SetSigWindow(1, 12, 180, 318, 55); //permits ink only in the section specified in sigplus object
            sigPlusNET.SetLCDCaptureMode((int) LCDCaptureModes.LCDCapInk);
            Application.Run();
        }

        protected override void OnStop()
        {
            //Reset Hardware
            sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Clear, 0, 0, 320, 240);
            sigPlusNET.LCDSetWindow(0, 0, 320, 240);
            sigPlusNET.SetSigWindow(1, 0, 0, 320, 240);
            sigPlusNET.KeyPadClearHotSpotList();
            sigPlusNET.SetLCDCaptureMode(1);
            sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Disabled);
            Application.ExitThread();
        }

        private void OnPenUp(object sender, EventArgs e)
        {
            if ( sigPlusNET.KeyPadQueryHotSpot(BUTTON_CLEAR) > 0 ) //clear chosen
            {
                sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
                sigPlusNET.ClearTablet();
                sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Complement, 32, 153, 37, 15); //invert px at CLEAR so user knows its been tapped
                Thread.Sleep(300);
                sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, 0, 150, 320, 90); //refresh lcd with background ONLY at bottom of LCD
                sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
            }

            if ( sigPlusNET.KeyPadQueryHotSpot(BUTTON_ACCEPT) > 0 ) //OK chosen
            {
                sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
                sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Complement, 250, 153, 34, 15); //invert px so user knows its been tapped
                Thread.Sleep(300);
                sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Complement, 250, 153, 34, 15);

                if ( sigPlusNET.NumberOfTabletPoints() > 0 ) //if there is a signature
                {
                    sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.Clear, 0, 0, 320, 240);
                    sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Disabled);

                    sigPlusNET.SetImageXSize(500);
                    sigPlusNET.SetImageYSize(150);
                    sigPlusNET.SetJustifyMode(5);
                    var image = sigPlusNET.GetSigImage();
                    image.Save(GetFileNameAndPath(), ImageFormat.Jpeg);

                    sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Capture);
                    sigPlusNET.LCDWriteString(0, 2, 63, 180, new Font("Arial", 10.0F, FontStyle.Regular), "Thank You For Signing!" + Environment.NewLine + "Signature saved to: " + Environment.NewLine + GetFileNameAndPath());
                    Thread.Sleep(4500);

                    sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, 0, 150, 320, 90); //refresh lcd with background ONLY at bottom of LCD
                    sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
                    //bring stored background image to foreground
                    sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, 0, 0, 320, 240);
                    sigPlusNET.SetTabletState((int) Enums.TabletStateEnum.Disabled);
                }
                else
                {
                    sigPlusNET.LCDWriteString(0, 2, 46, 186, new Font("Arial", 12.0F, FontStyle.Regular), "Please Sign Before Continuing...");
                    Thread.Sleep(2000);
                    sigPlusNET.LCDRefresh((byte) Enums.LcdRefreshModeEnum.WriteOpaque, 0, 180, 320, 59);
                    sigPlusNET.SetLCDCaptureMode(2);
                }
            }

            sigPlusNET.ClearSigWindow((short) Enums.ClearSigEnum.OutsideSigWindow);
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