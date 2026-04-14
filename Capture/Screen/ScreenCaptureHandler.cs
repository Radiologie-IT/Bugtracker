using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Media;
using System.Windows.Forms;
using Bugtracker.Configuration;
using Bugtracker.Console;
using Bugtracker.Globals_and_Information;
using Bugtracker.Logging;
using Microsoft.VisualBasic.Devices;

namespace Bugtracker.Capture.Screen
{
    /// <summary>
    /// This class handles everthing related to 
    /// screenshot capture
    /// 
    /// TODO: Test with multiple screens
    /// </summary>
    public class ScreenCaptureHandler
    {
        private int CurrentNumberInSequence = 0;
        /// <summary>
        /// public constructor
        /// </summary>
        public ScreenCaptureHandler()
        {
            Logger.Log("ScreenCaptureHandler objet was created.", (LoggingSeverity)2);
        }


        public static event EventHandler TookScreenshot;

        /// <summary>
        /// Assamble screenshot image file name
        /// 
        /// screenshot_PCName_data_time.jpg
        /// f.e. PC01_0102021_081243.jpg
        /// for PC01 01.02.2021 08:12:43
        /// </summary>
        public string BuildScreenShotFileName()
        {
            return BuildScreenShotFileName("screenshot", Globals.SCREENSHOT_FILE_FORMAT);
        }

        /// <summary>
        /// Assamble screenshot image file name using custom prefix
        /// 
        /// %prefix%_PCName_data_time.jpg
        /// f.e. PC01_0102021_081243.jpg
        /// for PC01 01.02.2021 08:12:43
        /// </summary>
        public string BuildScreenShotFileName(String prefix)
        {
            return BuildScreenShotFileName(prefix, Globals.SCREENSHOT_FILE_FORMAT);
        }

        /// <summary>
        /// Assamble screenshot image file name using custom prefix and filetype
        /// 
        /// %prefix%_PCName_data_time.%filetype%
        /// f.e. PC01_0102021_081243.jpg
        /// for PC01 01.02.2021 08:12:43
        /// </summary>
        public string BuildScreenShotFileName(string prefix, string filetype)
        {
            // log info
            Logger.Log("Building filename for "+prefix+" file(s).", (LoggingSeverity)2);

            // start with hostname
            string filename = prefix + "_" + PCInfo.Hostname;

            // build date format
            DateTime dt = DateTime.Now; // Or whatever
            string date = dt.ToString("_dd-MM-yy_HH-mm-ss");

            // finished filename
            filename += date + filetype;
            Logger.Log("Finished filename: " + filename, (LoggingSeverity)2);

            // return filename
            return filename;
        }


        /// <summary>
        /// Start and Stop isnt a function that can be realized in bugtracker core
        /// </summary>
        public void StartScreenRecording()
        { 
            //TODO: To be continued.
        }

        /// <summary>
        /// Start and Stop isnt a function that can be realized in bugtracker core
        /// </summary>
        public void StopScreenRecording()
        {
            //TODO: To be continued.
        }

        /// <summary>
        /// This method is responsible for generating the screenshots for the current bugtrack in sequence
        /// </summary>
        public void GenerateScreenshotInSequence()
        {
            CurrentNumberInSequence++;
            BugtrackConsole.Print("Screenshot Number" + CurrentNumberInSequence + "captured.");
            GenerateScreenshot(RunningConfiguration.GetInstance().BugtrackerFolderName, true);
        }

        /// <summary>
        /// This method is responsible for generating the screenshots for the current bugtrack from a given bitmap
        /// </summary>
        /// <param name="bugtrackFolderName"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public string GenerateScreenshotFromBitmap(string bugtrackFolderName, Bitmap bitmap, string prefix="screenshot")
        {
            var screenShotFileName = BuildScreenShotFileName(prefix);
            bitmap.Save(bugtrackFolderName + @"\" + screenShotFileName, ImageFormat.Jpeg);

            Logger.Log("Screenshot generated and saved at '" + screenShotFileName + "'", (LoggingSeverity)2);

            TookScreenshot?.Invoke(null, null);

            return screenShotFileName;
        }

        /// <summary>
        /// This method is responsible for
        /// generating the screenshots for
        /// the current bugtrack
        /// </summary>
        /// <param name="bugtrackerDirectoryPath"></param>
        /// <returns>Screenshot Path</returns>
        public string GenerateScreenshot(string bugtrackerDirectoryPath, bool sequence = false)
        {
            CurrentNumberInSequence++;

            // log info
            Logger.Log("Generating screenshot now", (LoggingSeverity)2);

            Logger.Log($"Bugtracker Directory Path for Screenshot: {bugtrackerDirectoryPath}", LoggingSeverity.Info);

            // get file name for screenshot file
            string screenShotFileName = BuildScreenShotFileName();

            try
            {
                // Determine the size of the "virtual screen", which includes all monitors.
                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;
                Logger.Log("Detected eges (left, top, width, height) = (" + screenLeft + ", " + screenTop + ", " + screenWidth + ", " + screenHeight, (LoggingSeverity)2);

                // Create a bitmap of the appropriate size to receive the screenshot.
                using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                {
                    // Draw the screenshot into our bitmap.
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);

                        Icon cursor =  (Icon) Bugtracker.Properties.Resources.ico_cursor;;

                        Brush cursorBrush = new SolidBrush(Color.FromArgb(80, Color.Yellow));
                        Brush fontBrush = new SolidBrush(Color.FromArgb(100, Color.White));
                        Pen curosrPen = new Pen(cursorBrush, 25f);

                        g.DrawEllipse(curosrPen, Cursor.Position.X, Cursor.Position.Y, 80, 80);
                        
                        if(sequence)
                        {
                            Font fontCanvas = new Font("Arial", 200f);
                            g.DrawString("Schritt " + CurrentNumberInSequence, fontCanvas, fontBrush, 100, 100);
                        }
                    }

                    if (sequence)
                        bmp.Save(bugtrackerDirectoryPath + "\\" + "step_" + CurrentNumberInSequence + "_" + screenShotFileName, ImageFormat.Jpeg);
                    else
                        // Do something with the Bitmap here, like save it to a file:
                        bmp.Save(bugtrackerDirectoryPath + "\\" + screenShotFileName, ImageFormat.Jpeg);

                    TookScreenshot?.Invoke(null, null);
                }

                Logger.Log("Screenshot generated and saved at '" + screenShotFileName + "'", (LoggingSeverity)2);
            }
            catch (Exception e)
            {
                Logger.Log("There was an error taking screenshots of the clients monitors.", 0);
                Logger.Log(e.Message, 0);
            }

            return screenShotFileName;
        }

    }
}
