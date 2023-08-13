using Froggit;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.mikroBUS.Displays;
using Meadow.Gateway.WiFi;
using Meadow.Hardware;
using SimpleJpegDecoder;
using System;
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

        IPixelBuffer defcon1;
        IPixelBuffer defcon2;
        IPixelBuffer defcon3;
        IPixelBuffer defcon4;
        IPixelBuffer defcon5;
        IPixelBuffer defcon6;

        FrogItGame frogit;
        PacMan pacman;

        IWiFiNetworkAdapter wifi;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            projLab = ProjectLab.Create();
            ((Ili9341)projLab.Display).SpiBusSpeed = new Meadow.Units.Frequency(48, Meadow.Units.Frequency.UnitType.Megahertz);
            graphics = new MicroGraphics(projLab.Display);

            frogit = new FrogItGame();
            frogit.Init(graphics, new MicroAudio(projLab.Speaker));

            pacman = new PacMan(graphics);

            projLab.DownButton.Clicked += ButtonDown_Clicked;

            projLab.RgbLed.SetColor(Color.FromHex("#686EA0"));

            graphics.Clear();
            graphics.DrawText(0, 0, "Initializing hardware ...", Color.White);
            graphics.DrawText(0, 12, "Scanning WiFi ...", Color.White);
            graphics.DrawText(0, 24, "Scanning BlueTooth ...", Color.White);
            graphics.Show();

            defcon1 = LoadImage("defcon31-1.jpg");
            defcon2 = LoadImage("defcon31-2.jpg");
            defcon3 = LoadImage("defcon31-3.jpg");
            defcon4 = LoadImage("defcon31-banner.jpg");
            defcon5 = LoadImage("defcon31-5.jpg");
            defcon6 = LoadImage("defcon31-banner2.jpg");

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            // enumerate the public WiFi channels
            await ScanForAccessPoints(wifi);
            await Task.Delay(1000);

            await DrawLoadingScreen();
            _ = StartImageLoop();
        }

        async Task DrawLoadingScreen()
        {
            var color = Color.FromHex("#81C8BD");
            var font = new Font8x12();
            graphics.Clear();
            graphics.DrawRectangle(40, 60, 240, 120, color, true);
            graphics.DrawRectangle(42, 82, 236, 96, Color.Black, true);
            graphics.DrawText(160, 100, "Loading ...", color, ScaleFactor.X2, HorizontalAlignment.Center, font: font);
            graphics.DrawRectangle(52, 140, 216, 26, color, false);

            graphics.Show();

            for(int i = 4; i < 216; i+=4)
            {
                graphics.DrawRectangle(54, 142, i, 22, color, true);
                graphics.Show();
            }
        }

        async Task StartImageLoop()
        {
            await ScanForAccessPoints(wifi);
            await Task.Delay(8000);

            DrawBanner(defcon4, 32);
            await CountDown(1);

            DrawSplash(defcon1);
            await CountDown(9);

            DrawSplash(defcon2);
            await CountDown(9);

            DrawBanner(defcon6, 32);
            await CountDown(1);

            await DrawGlitch(defcon5);
            await CountDown(2);

            DrawSplash(defcon3);
            await CountDown(9);

            _ = PlayPacMan();
        }

        void DrawBanner(IPixelBuffer buffer, int speed)
        {
            for(int i = 0; i < buffer.Width - graphics.Width; i+=speed) 
            {
                graphics.DrawBuffer(0 - i, 0, buffer);
                graphics.Show();
            }
        }

        void DrawSplash(IPixelBuffer buffer)
        {
            graphics.DrawBuffer(0, 0, buffer);
            graphics.Show();
        }

        async Task DrawGlitch(IPixelBuffer buffer) 
        {
            graphics.DrawBuffer(0, 0, buffer);
            graphics.Show();

            await Task.Delay(1000);

            graphics.DrawBuffer(0, 0, buffer);
            graphics.Show();

            graphics.DrawBuffer(8, 0, buffer);
            graphics.Show();

            graphics.DrawBuffer(-8, 0, buffer);
            graphics.Show();

            graphics.DrawBuffer(0, 0, buffer);
            graphics.Show();

            graphics.DrawBuffer(4, 0, buffer);
            graphics.Show();

            await Task.Delay(1000);

            graphics.DrawBuffer(0, 0, buffer);
            graphics.Show();

            await Task.Delay(1000);
        }

        public static IPixelBuffer LoadImage(string name)
        {
            var jpgData = LoadResource(name);

            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(jpgData);

            Console.WriteLine($"Decoded {name}: {decoder.Width}x{decoder.Height}, {jpg.Length} bytes");

            return new BufferRgb888(decoder.Width, decoder.Height, jpg).ConvertPixelBuffer<BufferRgb565>();
        }

        private void ButtonDown_Clicked(object sender, EventArgs e)
        {
            if(pacman.IsRunning == false)
            {
                PlayPacMan();
            }

            return;

            if(frogit.IsPlaying == false)
            {
                PlayFrogger();
            }
        }

        public static byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"DefConBadge2023.{filename}";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using var ms = new MemoryStream();
        
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        Task PlayFrogger()
        {
            var t = new Task(() =>
            {
                frogit.Reset();
                while (frogit.IsPlaying)
                {
                    UpdateGame();
                    Thread.Sleep(0);
                }
                _ = StartImageLoop();
            }, TaskCreationOptions.LongRunning);

            t.Start();

            return t;
        }

        Task PlayPacMan()
        {
            var t = new Task(() =>
            {
                pacman.Reset();
                while (pacman.IsRunning)
                {
                    pacman.Update();
                    Thread.Sleep(0);
                }
                _ = StartImageLoop();
            }, TaskCreationOptions.LongRunning);

            t.Start();

            return t;
        }

        void UpdateGame()
        {
            if (projLab.LeftButton.State == true)
            {
                frogit.Left();
            }
            else if (projLab.RightButton.State == true)
            {
                frogit.Right();
            }
            else if (projLab.UpButton.State == true)
            {
                frogit.Up();
            }
            else if (projLab.DownButton.State == true)
            {
                frogit.Down();
            }

            frogit.Update();
        }

        /*
        public void SetAuxDisplay(int value)
        {
            graphics8x8.Clear();
            graphics8x8.DrawText(2, 2, $"{value}");
            graphics8x8.Show();
        }
        */

        async Task CountDown(int seconds)
        {
            for(int i = seconds; i > 0; i--)
            {
            //    SetAuxDisplay(i);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        bool IsAnimationPlaying()
        {
            if (frogit.IsPlaying || pacman.IsRunning)
                return true;

            return false;
        }

        async Task ScanForAccessPoints(IWiFiNetworkAdapter adapter)
        {
            graphics.Clear();

            Console.WriteLine("Getting list of access points.");
           
            int spacing = 12;
            int y = 0;

            var networks = await adapter.Scan();

            graphics.DrawText(0, 0, $"Found {networks.Count} access points...");

            if (networks.Count > 0)
            {
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                Console.WriteLine("|         Network Name             | RSSI |       BSSID       | Channel |");
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                foreach (WifiNetwork accessPoint in networks)
                {
                    Console.WriteLine($"| {accessPoint.Ssid,-32} | {accessPoint.SignalDbStrength,4} | {accessPoint.Bssid,17} |   {accessPoint.ChannelCenterFrequency,3}   |");

                    y += spacing;
                    graphics.DrawText(0, y, $"| {accessPoint.SignalDbStrength,4} | {accessPoint.Bssid,17} | {accessPoint.ChannelCenterFrequency,3} |");
                }
            }
            else
            {
                Console.WriteLine($"No access points detected.");
            }

            graphics.Show();
        }
    }
}