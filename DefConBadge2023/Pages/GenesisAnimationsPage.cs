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
        public bool IsUpdating = false;

        JpegAnimation streetsOfRage2AxelAnimation;
        JpegAnimation sonicCdSonicTapAnimation;
        JpegAnimation tjearlEarlWalkAnimation;

        TimeSpan animationFrameDelay = TimeSpan.FromMilliseconds(0);
        int currentAnimationFrame = 0;
        int streetsOfRage2AxelBufferDrawLocationX;
        int streetsOfRage2AxelBufferDrawLocationY;
        int sonicCdSonicTapBufferDrawLocationX;
        int sonicCdSonicTapBufferDrawLocationY;
        int tjearlEarlWalkBufferDrawLocationX;
        int tjearlEarlWalkBufferDrawLocationY;

        public class JpegAnimation
        {
            const string JpegExtension = ".jpg";
            public int FrameCount { get; private set; }
            public string ResourcePrefix { get; private set; }
            public IPixelBuffer[] Frames { get; private set; }
            public Size FrameSize { get; private set; }
            public JpegAnimation(int frameCount, string resourcePrefix)
            {
                FrameCount = frameCount;
                ResourcePrefix = resourcePrefix;
                Frames = new IPixelBuffer[frameCount];
            }

            public void Init()
            {
                foreach (int i in Enumerable.Range(0, FrameCount))
                {
                    Frames[i] = MeadowApp.LoadImage(ResourcePrefix + i + JpegExtension);
                }
                if (Frames[0] != null)
                {
                    FrameSize = new Size(Frames[0].Width, Frames[0].Height);
                }
            }
        }

        public void StartUpdating(IProjectLabHardware config, MicroGraphics graphics)
        {
            IsUpdating = true;

            while (IsUpdating)
            {
                graphics.Clear();

                graphics.DrawBuffer(streetsOfRage2AxelBufferDrawLocationX, streetsOfRage2AxelBufferDrawLocationY, streetsOfRage2AxelAnimation.Frames[currentAnimationFrame % streetsOfRage2AxelAnimation.FrameCount]);
                graphics.DrawBuffer(sonicCdSonicTapBufferDrawLocationX, sonicCdSonicTapBufferDrawLocationY, sonicCdSonicTapAnimation.Frames[currentAnimationFrame % sonicCdSonicTapAnimation.FrameCount]);
                graphics.DrawBuffer(tjearlEarlWalkBufferDrawLocationX, tjearlEarlWalkBufferDrawLocationY, tjearlEarlWalkAnimation.Frames[currentAnimationFrame % tjearlEarlWalkAnimation.FrameCount]);

                graphics.Show();
                currentAnimationFrame += 1;
                Thread.Sleep(animationFrameDelay);
            }
        }

        public void StopUpdating()
        {
            IsUpdating = false;
            Resolver.Log.Info($"Counted {currentAnimationFrame} frames");
            currentAnimationFrame = 0;
        }

        public void Init(IProjectLabHardware hardware)
        {
            // HACK: Probably some clean-up needed elsewhere, but GC.Collect prevents memory issues.
            GC.Collect();
            streetsOfRage2AxelAnimation = new JpegAnimation(7, "DefConBadge2023.sprites_sor2.axel-uppercut-");
            streetsOfRage2AxelAnimation.Init();
            sonicCdSonicTapAnimation = new JpegAnimation(4, "DefConBadge2023.sprites_soniccd.sonic-tap-");
            sonicCdSonicTapAnimation.Init();
            tjearlEarlWalkAnimation = new JpegAnimation(6, "DefConBadge2023.sprites_tjearl.tjearl-tjwalk-");
            tjearlEarlWalkAnimation.Init();
            GC.Collect();

            //PLv3 screen: 320x240
            streetsOfRage2AxelBufferDrawLocationX = 25;
            streetsOfRage2AxelBufferDrawLocationY = 15;
            sonicCdSonicTapBufferDrawLocationX = (hardware.Display.Width - sonicCdSonicTapAnimation.FrameSize.Width - 25);
            sonicCdSonicTapBufferDrawLocationY = (hardware.Display.Height - sonicCdSonicTapAnimation.FrameSize.Height - 15);
            tjearlEarlWalkBufferDrawLocationX = (hardware.Display.Width - tjearlEarlWalkAnimation.FrameSize.Width - 25);
            tjearlEarlWalkBufferDrawLocationY = 15;
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