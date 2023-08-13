using Meadow.Foundation.Graphics;

namespace DefConBadge2023
{
    public class Pong
    {
        public bool IsRunning = false;

        int frameCount;

        int player1X, player1Y;
        int player2X, player2Y;

        int ballX, ballY;

        int ballSpeedX, ballSpeedY;

        int paddleWidth = 8;
        int paddleHeight = 32;

        int ballRadius = 8;

        MicroGraphics graphics;

        public Pong(MicroGraphics graphics)
        {
            this.graphics = graphics;
        }

        public void Reset()
        {
            player1X = 10;
            player2X = 320 - 10 - paddleWidth;

            ballX = 160;

            IsRunning = true;
        }



    }
}
