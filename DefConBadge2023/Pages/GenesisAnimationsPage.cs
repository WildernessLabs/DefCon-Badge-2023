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
        Point initialStreetsOfRage2AxelBufferDrawLocation;
        Point currentStreetsOfRage2AxelBufferDrawLocation;
        Point initialSonicCdSonicTapBufferDrawLocation;
        Point currentSonicCdSonicTapBufferDrawLocation;
        Point initialTjearlEarlWalkBufferDrawLocation;
        Point currentTjearlEarlWalkBufferDrawLocation;

        public class JpegAnimation
        {
            const string JpegExtension = ".jpg";
            public int FrameCount { get; private set; }
            public string ResourcePrefix { get; private set; }
            public IPixelBuffer[] Frames { get; private set; }
            public Size FrameSize { get; private set; }

            public Size FrameMovement { get; private set; }

            public JpegAnimation(int frameCount, string resourcePrefix) : this(frameCount, resourcePrefix, Size.Empty) { }
            public JpegAnimation(int frameCount, string resourcePrefix, Size frameMovement)
            {
                FrameCount = frameCount;
                ResourcePrefix = resourcePrefix;
                Frames = new IPixelBuffer[frameCount];
                FrameMovement = frameMovement;
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
            currentSonicCdSonicTapBufferDrawLocation = initialSonicCdSonicTapBufferDrawLocation;
            currentStreetsOfRage2AxelBufferDrawLocation = initialStreetsOfRage2AxelBufferDrawLocation;
            currentTjearlEarlWalkBufferDrawLocation = initialTjearlEarlWalkBufferDrawLocation;
            var windowSize = new Size(graphics.Width, graphics.Height);
            var sonicCdSonicTapSize = sonicCdSonicTapAnimation.FrameSize;
            var streetsOfRage2AxelSize = streetsOfRage2AxelAnimation.FrameSize;
            var tjearlEarlWalkSize = tjearlEarlWalkAnimation.FrameSize;

            _ = Task.Run(() =>
            {
                while (IsUpdating)
                {
                    graphics.Clear();

                    graphics.DrawBuffer(currentStreetsOfRage2AxelBufferDrawLocation.X, currentStreetsOfRage2AxelBufferDrawLocation.Y, streetsOfRage2AxelAnimation.Frames[currentAnimationFrame % streetsOfRage2AxelAnimation.FrameCount]);
                    currentStreetsOfRage2AxelBufferDrawLocation.X += streetsOfRage2AxelAnimation.FrameMovement.Width;
                    currentStreetsOfRage2AxelBufferDrawLocation.Y += streetsOfRage2AxelAnimation.FrameMovement.Height;
                    if (!IsSpriteInBounds(ref windowSize, ref currentStreetsOfRage2AxelBufferDrawLocation, ref streetsOfRage2AxelSize))
                    {
                        currentStreetsOfRage2AxelBufferDrawLocation = initialStreetsOfRage2AxelBufferDrawLocation;
                    }
                    graphics.DrawBuffer(currentSonicCdSonicTapBufferDrawLocation.X, currentSonicCdSonicTapBufferDrawLocation.Y, sonicCdSonicTapAnimation.Frames[currentAnimationFrame % sonicCdSonicTapAnimation.FrameCount]);
                    currentSonicCdSonicTapBufferDrawLocation.X += sonicCdSonicTapAnimation.FrameMovement.Width;
                    currentSonicCdSonicTapBufferDrawLocation.Y += sonicCdSonicTapAnimation.FrameMovement.Height;
                    graphics.DrawBuffer(currentTjearlEarlWalkBufferDrawLocation.X, currentTjearlEarlWalkBufferDrawLocation.Y, tjearlEarlWalkAnimation.Frames[currentAnimationFrame % tjearlEarlWalkAnimation.FrameCount]);
                    currentTjearlEarlWalkBufferDrawLocation.X += tjearlEarlWalkAnimation.FrameMovement.Width;
                    currentTjearlEarlWalkBufferDrawLocation.Y += tjearlEarlWalkAnimation.FrameMovement.Height;
                    if (!IsSpriteInBounds(ref windowSize, ref currentTjearlEarlWalkBufferDrawLocation, ref tjearlEarlWalkSize))
                    {
                        currentTjearlEarlWalkBufferDrawLocation = initialTjearlEarlWalkBufferDrawLocation;
                    }

                    graphics.Show();
                    currentAnimationFrame += 1;
                    Thread.Sleep(animationFrameDelay);
                }
            });
        }

        /// <summary>
        /// Determine if any corners of a rectangular sprite are within the provided window.
        /// </summary>
        bool IsSpriteInBounds(ref Size window, ref Point spriteLocation, ref Size spriteSize)
        {
            Point[] spriteCorners = new Point[4];
            spriteCorners[0] = spriteLocation;
            spriteCorners[1] = new Point(spriteLocation.X + spriteSize.Width, spriteLocation.Y);
            spriteCorners[2] = new Point(spriteLocation.X + spriteSize.Width, spriteLocation.Y + spriteSize.Height);
            spriteCorners[3] = new Point(spriteLocation.X, spriteLocation.Y + spriteSize.Height);
            return IsCoordinateInBounds(ref window, ref spriteCorners[0])
                || IsCoordinateInBounds(ref window, ref spriteCorners[1])
                || IsCoordinateInBounds(ref window, ref spriteCorners[2])
                || IsCoordinateInBounds(ref window, ref spriteCorners[3]);
        }
        bool IsCoordinateInBounds(ref Size window, ref Point location)
        {
            if (location.X < 0 || location.Y < 0) { return false; }

            if (location.X > window.Width || location.Y > window.Height) { return false; }

            return true;
        }

        public void StopUpdating()
        {
            IsUpdating = false;
            currentAnimationFrame = 0;
        }

        public void Init(IProjectLabHardware hardware)
        {
            // HACK: Probably some clean-up needed elsewhere, but GC.Collect prevents memory issues.
            GC.Collect();
            streetsOfRage2AxelAnimation = new JpegAnimation(7, "DefConBadge2023.sprites_sor2.axel-uppercut-", new Size(-3, 0));
            streetsOfRage2AxelAnimation.Init();
            sonicCdSonicTapAnimation = new JpegAnimation(4, "DefConBadge2023.sprites_soniccd.sonic-tap-");
            sonicCdSonicTapAnimation.Init();
            tjearlEarlWalkAnimation = new JpegAnimation(6, "DefConBadge2023.sprites_tjearl.tjearl-tjwalk-", new Size(-5, 5));
            tjearlEarlWalkAnimation.Init();
            GC.Collect();

            //PLv3 screen: 320x240
            initialStreetsOfRage2AxelBufferDrawLocation = new Point(100, 15);
            initialSonicCdSonicTapBufferDrawLocation = new Point(hardware.Display.Width - sonicCdSonicTapAnimation.FrameSize.Width - 25, hardware.Display.Height - sonicCdSonicTapAnimation.FrameSize.Height - 15);
            initialTjearlEarlWalkBufferDrawLocation = new Point(hardware.Display.Width - tjearlEarlWalkAnimation.FrameSize.Width, 0 - tjearlEarlWalkAnimation.FrameSize.Height);
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