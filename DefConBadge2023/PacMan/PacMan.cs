using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace DefConBadge2023
{
    public class PacMan
    {
        public bool IsRunning = false;

        int pacmanX, pacmanY;
        int ghostX, ghostY;

        int updateAmount = 8;

        IPixelBuffer pacman, pacman2;
        IPixelBuffer ghost, ghost2;

        MicroGraphics graphics;

        public PacMan(MicroGraphics graphics)
        {
            LoadImages();
            this.graphics = graphics;
        }

        public void Reset()
        {
            pacmanX = 0;
            pacmanY = 92;
            ghostX = -120;
            ghostY = 92;

            IsRunning = true;
        }

        Rect updateRect = new Rect(0, 92, 320, 158);

        bool pac1 = true;

        public void Update()
        {
            graphics.Clear();

            graphics.DrawRectangle(updateRect.Left, updateRect.Top, updateRect.Width, updateRect.Height, Color.Black, true);

            graphics.DrawBuffer(pacmanX, pacmanY, pac1?pacman:pacman2);
            

            graphics.DrawBuffer(ghostX, ghostY, pac1?ghost:ghost2);
            pac1 = !pac1;

            graphics.Show();

            pacmanX += updateAmount;
            ghostX += updateAmount;

            if(ghostX > 320)
            {
                IsRunning = false;
            }
        }

        void LoadImages()
        {
            pacman = MeadowApp.LoadImage("mspacman2-56.jpg");
            pacman2 = MeadowApp.LoadImage("mspacman3-56.jpg");
            ghost = MeadowApp.LoadImage("redghost-56.jpg");
            ghost2 = MeadowApp.LoadImage("redghost2-56.jpg");
        }
    }
}