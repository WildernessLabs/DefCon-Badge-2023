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

        IGraphicsDisplay mikroBusLedDisplay;
        MicroGraphics mikroBusLedDisplayGraphics;

        IBadgePage currentPage;

        IBadgePage[] pages;

        BufferRgb888 defcon1;
        BufferRgb888 defcon2;
        BufferRgb888 defcon3;
        BufferRgb888 defcon4;

        //Random rand = new Random();

        // TODO: Possibly worth making these part of some inherited system of hardware config classes?
        const bool useTertiaryDisplay = false;
        const bool useSecondaryDisplay = true;

        const float defaultBrightness = 0.2f;
        const float brightnessChangeAmount = 0.1f;
        const double lowestAmbientBrightnessThreshold = 40;
        const double highestAmbientBrightnessThreshold = 1500;
        float _tertiaryDisplayBrightness = defaultBrightness;
        float tertiaryDisplayBrightness {
            get { return _tertiaryDisplayBrightness; }
            set { _tertiaryDisplayBrightness = Math.Clamp(value, 0.0f, 1.0f); }
        }
        float AdjustDisplayBrightnessToAmbientBrightness()
        {
            float adjustedBrightness = defaultBrightness;
            if (projLab.LightSensor.Illuminance is { } light)
            {
                if (light is { } lightReading)
                {
                    adjustedBrightness = (float)Math.Clamp(lightReading.Lux, lowestAmbientBrightnessThreshold, highestAmbientBrightnessThreshold).Map(lowestAmbientBrightnessThreshold, highestAmbientBrightnessThreshold, 0.1, 1.0);
                    //Resolver.Log.Info($"Lux: {lightReading.Lux}, Adjusted: {adjustedBrightness}");
                }
            }
            return adjustedBrightness;
        }

        public override async Task Run()
        {
            ScrollBannerMessage(mikroBusLedDisplayGraphics);
            while (true)
            {
                DrawSplash(defcon1);
                await Task.Delay(10000).ConfigureAwait(false);

                currentPage.StartUpdating(projLab, graphics);
                await Task.Delay(20000).ConfigureAwait(false);
                currentPage.StopUpdating();

                DrawSplash(defcon2);
                await Task.Delay(3000).ConfigureAwait(false);

                currentPage.StartUpdating(projLab, graphics);
                await Task.Delay(20000).ConfigureAwait(false);
                currentPage.StopUpdating();

                DrawSplash(defcon3);
                await Task.Delay(3000).ConfigureAwait(false);

                DrawSplash(defcon4);
                await Task.Delay(3000).ConfigureAwait(false);
            }
        }

        private void ScrollBannerMessage(MicroGraphics graphics)
        {
            _ = Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                int maxCompleteCharsPerScreen = (int)Math.Floor(graphics.Width / (graphics.CurrentFont.Width + 1.0));
                var message = (new string(' ', maxCompleteCharsPerScreen + 1)) + "Meadow + DC31";
                Resolver.Log.Info(message);

                while (true)
                {
                    for (int x = 0; x < message.Length * (graphics.CurrentFont.Width + 1); x++)
                    {
                        if (stopwatch.ElapsedMilliseconds > 500)
                        {
                            //Resolver.Log.Info("Adjusting brightness");
                            tertiaryDisplayBrightness = AdjustDisplayBrightnessToAmbientBrightness();
                            stopwatch.Restart();
                        }
                        graphics.Clear();
                        graphics.DrawText(0 - x, 1, message, new Color(tertiaryDisplayBrightness, tertiaryDisplayBrightness, tertiaryDisplayBrightness));
                        graphics.Show();

                        Thread.Sleep(0);
                    }

                    Thread.Sleep(50);
                }
            }).ConfigureAwait(false);
        }

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize hardware...");

            projLab = ProjectLab.Create();
            graphics = new MicroGraphics(projLab.Display);

            mikroBusLedDisplay = new C16x9(projLab.MikroBus2.Pins.AN, projLab.MikroBus2.I2cBus);
            mikroBusLedDisplayGraphics = new MicroGraphics(mikroBusLedDisplay);

            projLab.LeftButton.Clicked += ButtonLeft_Clicked;
            projLab.RightButton.Clicked += ButtonRight_Clicked;
            
            projLab.DownButton.Clicked += ButtonDown_Clicked;
            projLab.UpButton.Clicked += ButtonUp_Clicked;

            projLab.RgbLed.SetColor(Color.Green);

            pages = new IBadgePage[]
            {
                new EnvironmentPage(),
                new GraphPage(),
                new WiFiTrackerPage(),
            };

            currentPage = pages[0];

            graphics.Clear();
            graphics.DrawText(0, 0, "Initializing ...", Color.White);
            graphics.Show();

            defcon1 = LoadImage("defcon31-1.jpg");
            defcon2 = LoadImage("defcon31-2.jpg");
            defcon3 = LoadImage("defcon31-3.jpg");
            defcon4 = LoadImage("defcon31-4.jpg");

            return Task.CompletedTask;
        }

        void DrawSplash(IPixelBuffer buffer)
        {
            graphics.Clear();

            graphics.DrawBuffer(0, 0, buffer);

            //Resolver.Log.Info("Jpeg show");

            graphics.Show();
        }

        BufferRgb888 LoadImage(string name)
        {
            var jpgData = LoadResource(name);

            Resolver.Log.Info($"Loaded {jpgData.Length} bytes, decoding jpeg ...");

            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(jpgData);

            Resolver.Log.Info($"Size: {jpg.Length} bytes; {decoder.Width}x{decoder.Height} pixels");

            return new BufferRgb888(decoder.Width, decoder.Height, jpg);
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
            tertiaryDisplayBrightness += brightnessChangeAmount;
            Resolver.Log.Info($"Brightness: {tertiaryDisplayBrightness}");
            currentPage.Right();
        }

        private void ButtonLeft_Clicked(object sender, EventArgs e)
        {
            tertiaryDisplayBrightness -= brightnessChangeAmount;
            Resolver.Log.Info($"Brightness: {tertiaryDisplayBrightness}");
            currentPage.Left();
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"DefConBadge2023.{filename}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}