using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.mikroBUS.Displays;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using SimpleJpegDecoder;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DefConBadge2023
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        IBadgePage currentPage;

        IBadgePage[] pages = new IBadgePage[]
        {
            new GenesisAnimationsPage(),
            new EnvironmentPage(),
            //new GraphPage(),
            //new WiFiTrackerPage(),
        };

        IPixelBuffer defcon1;
        IPixelBuffer defcon2;
        IPixelBuffer defcon3;
        //IPixelBuffer defcon4;

        public override async Task Run()
        {
            while (true)
            {
                DrawSplash(defcon1);
                await Task.Delay(4000).ConfigureAwait(false);

                currentPage = pages[0];
                currentPage.StartUpdating(projLab, graphics);
                await Task.Delay(10000).ConfigureAwait(false);
                currentPage.StopUpdating();

                DrawSplash(defcon2);
                await Task.Delay(2500).ConfigureAwait(false);

                currentPage = pages[1];
                currentPage.StartUpdating(projLab, graphics);
                await Task.Delay(20000).ConfigureAwait(false);
                currentPage.StopUpdating();

                DrawSplash(defcon3);
                await Task.Delay(2500).ConfigureAwait(false);

                //DrawSplash(defcon4);
                //await Task.Delay(2500).ConfigureAwait(false);
            }
        }

        public override Task Initialize()
        {
            //foreach (var x in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            //{
            //    Resolver.Log.Info($"{x}");
            //}

            Resolver.Log.Info("Initialize hardware...");

            projLab = ProjectLab.Create();
            graphics = new MicroGraphics(projLab.Display)
            {
                IgnoreOutOfBoundsPixels = true,
            };

            projLab.LeftButton.Clicked += ButtonLeft_Clicked;
            projLab.RightButton.Clicked += ButtonRight_Clicked;
            
            projLab.DownButton.Clicked += ButtonDown_Clicked;
            projLab.UpButton.Clicked += ButtonUp_Clicked;

            projLab.RgbLed.SetColor(Color.Green);

            foreach (var page in pages)
            {
                page.Init(projLab);
            }
            currentPage = pages[0];

            graphics.Clear();
            graphics.DrawText(0, 0, "Initializing ...", Color.White);
            graphics.Show();

            defcon1 = LoadImage("DefConBadge2023.defcon31-1.jpg");
            defcon2 = LoadImage("DefConBadge2023.defcon31-2.jpg");
            defcon3 = LoadImage("DefConBadge2023.defcon31-3.jpg");
            //defcon4 = LoadImage("DefConBadge2023.defcon31-4.jpg");

            return Task.CompletedTask;
        }

        void DrawSplash(IPixelBuffer buffer)
        {
            graphics.Clear();

            graphics.DrawBuffer(0, 0, buffer);

            graphics.Show();
        }

        public static IPixelBuffer LoadImage(string name)
        {
            byte[] jpgData;
            try
            {
                jpgData = LoadResource(name);
            }
            catch (Exception)
            {
                Resolver.Log.Error($"Failed to load resource: {name}");
                throw;
            }

            Resolver.Log.Info($"Loaded {jpgData.Length} bytes, decoding jpeg ...");

            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(jpgData);

            Resolver.Log.Info($"Size: {jpg.Length} bytes; {decoder.Width}x{decoder.Height} pixels");

            return new BufferRgb888(decoder.Width, decoder.Height, jpg).ConvertPixelBuffer<BufferRgb565>();
        }
        public static byte[] LoadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        private void ButtonUp_Clicked(object sender, EventArgs e)
        {
            currentPage.Up();
        }

        private void ButtonDown_Clicked(object sender, EventArgs e)
        {
            currentPage.Down();
        }

        private void ButtonRight_Clicked(object sender, EventArgs e)
        {
            currentPage.Right();
        }

        private void ButtonLeft_Clicked(object sender, EventArgs e)
        {
            currentPage.Left();
        }
    }
}