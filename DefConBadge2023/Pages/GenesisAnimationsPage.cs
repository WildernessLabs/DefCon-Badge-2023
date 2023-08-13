using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Light;
using SimpleJpegDecoder;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DefConBadge2023
{
    public class GenesisAnimationsPage : IBadgePage
    {
        IProjectLabHardware config;

        MicroGraphics graphics;

        public bool IsUpdating = false;

        //320x240
        IPixelBuffer[] streetsOfRage2AxelAnimationImages = new IPixelBuffer[7];
        int streetsOfRage2AxelAnimationFrameWidth = 76;
        int streetsOfRage2AxelAnimationFrameHeight = 112;
        //IPixelBuffer[] sonicCdSonicTapAnimationImages = new IPixelBuffer[4];
        int sonicCdSonicTapAnimationFrameWidth = 30;
        int sonicCdSonicTapAnimationFrameHeight = 38;

        public GenesisAnimationsPage()
        {
        }

        //TimeSpan animationFrameDelay = TimeSpan.FromMilliseconds(50);
        int currentAnimationFrame = 0;

        public void StartUpdating(IProjectLabHardware config, MicroGraphics graphics)
        {
            this.config = config;
            this.graphics = graphics;

            IsUpdating = true;

            var streetsOfRage2AxelBufferDrawLocationX = 25 - (int)(streetsOfRage2AxelAnimationFrameWidth / 2);
            var streetsOfRage2AxelBufferDrawLocationY = 15 - (int)(streetsOfRage2AxelAnimationFrameHeight / 2);
            var sonicCdSonicTapBufferDrawLocationX = (graphics.Width - sonicCdSonicTapAnimationFrameWidth - 25);
            var sonicCdSonicTapBufferDrawLocationY = (graphics.Height - sonicCdSonicTapAnimationFrameHeight - 15);

            Task.Run(() =>
            {
                while (IsUpdating)
                {
                    graphics.Clear();

                    graphics.DrawBuffer(streetsOfRage2AxelBufferDrawLocationX, streetsOfRage2AxelBufferDrawLocationY, streetsOfRage2AxelAnimationImages[currentAnimationFrame % streetsOfRage2AxelAnimationImages.Length]);
                    //graphics.DrawBuffer(sonicCdSonicTapBufferDrawLocationX, sonicCdSonicTapBufferDrawLocationY, sonicCdSonicTapAnimationImages[currentAnimationFrame % sonicCdSonicTapAnimationImages.Length]);

                    graphics.Show();
                    currentAnimationFrame += 1;
                    Thread.Sleep(0);
                    //Thread.Sleep(animationFrameDelay);
                }
            });
        }

        public void StopUpdating()
        {
            IsUpdating = false;
            Resolver.Log.Info($"Counted {currentAnimationFrame} frames");
            currentAnimationFrame = 0;
        }

        public void Init(IProjectLabHardware hardware)
        {
            foreach (int i in Enumerable.Range(0, streetsOfRage2AxelAnimationImages.Length))
            {
                streetsOfRage2AxelAnimationImages[i] = LoadImage("DefConBadge2023.sprites_sor2.axel-uppercut-" + i + ".jpg");
            }
            //foreach (int i in Enumerable.Range(0, sonicCdSonicTapAnimationImages.Length))
            //{
            //    sonicCdSonicTapAnimationImages[i] = LoadImage("DefConBadge2023.sprites_soniccd.sonic-tap-" + i + ".jpg");
            //}
        }

        IPixelBuffer LoadImage(string name)
        {
            var jpgData = LoadResource(name);

            Resolver.Log.Info($"Loaded {jpgData.Length} bytes, decoding jpeg ...");

            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(jpgData);

            Resolver.Log.Info($"Size: {jpg.Length} bytes; {decoder.Width}x{decoder.Height} pixels");

            return new BufferRgb888(decoder.Width, decoder.Height, jpg).ConvertPixelBuffer<BufferRgb565>();
        }
        byte[] LoadResource(string resourceName)
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

        public void Reset()
        {
            Resolver.Log.Info("Resetting page...");
        }

        public void Down()
        {
        }

        public void Left()
        {
        }

        public void Right()
        {
        }

        public void Up()
        {
        }
    }
}