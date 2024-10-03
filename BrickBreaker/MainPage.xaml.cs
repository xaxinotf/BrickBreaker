using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickBreaker
{
    public partial class MainPage : ContentPage
    {
        double screenWidth, screenHeight;
        double ballX, ballY;
        double ballSpeedX, ballSpeedY;

        double paddleX;

        int score = 0;
        int lives = 3;
        int highScore = 0;
        Random random = new Random();

        List<BoxView> bricks = new List<BoxView>();
        List<Ellipse> balls = new List<Ellipse>();
        List<double> ballSpeedsX = new List<double>();
        List<double> ballSpeedsY = new List<double>();

        public MainPage()
        {
            InitializeComponent();
            this.SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, EventArgs e)
        {
            screenWidth = this.Width;
            screenHeight = this.Height;
            HighScoreLabel.Text = $"Рекорд: {highScore}";
        }

        private void SetupGame()
        {
            score = 0;
            lives = 3;
            ScoreLabel.Text = $"Рахунок: {score}";
            LivesLabel.Text = $"Життя: {lives}";

            // Видалення всіх м'ячів з попередньої гри
            foreach (var ball in balls.ToList())
            {
                GameLayout.Children.Remove(ball);
            }
            balls.Clear();
            ballSpeedsX.Clear();
            ballSpeedsY.Clear();

            // Створення початкового м'яча
            AddBall();

            paddleX = (screenWidth - Paddle.WidthRequest) / 2;
            AbsoluteLayout.SetLayoutBounds(Paddle, new Rect(paddleX, screenHeight - 50, Paddle.WidthRequest, Paddle.HeightRequest));

            CreateBricks();
            ResetBonuses();
        }

        private void AddBall()
        {
            // Додаємо новий м'яч на екран
            var ball = new Ellipse
            {
                Fill = Ball.Fill,
                WidthRequest = Ball.WidthRequest,
                HeightRequest = Ball.HeightRequest
            };

            balls.Add(ball);
            ballX = screenWidth / 2;
            ballY = screenHeight / 2;

            double speedX = random.Next(4, 7) * (random.Next(0, 2) == 0 ? 1 : -1);
            double speedY = random.Next(4, 7) * (random.Next(0, 2) == 0 ? 1 : -1);

            ballSpeedsX.Add(speedX);
            ballSpeedsY.Add(speedY);

            AbsoluteLayout.SetLayoutBounds(ball, new Rect(ballX, ballY, ball.WidthRequest, ball.HeightRequest));
            GameLayout.Children.Add(ball);
        }

        private void CreateBricks()
        {
            foreach (var brick in bricks)
            {
                GameLayout.Children.Remove(brick);
            }
            bricks.Clear();

            double brickWidth = screenWidth / 10;
            double brickHeight = 20;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 10 - i; j++)
                {
                    var brick = new BoxView
                    {
                        Color = Color.FromRgb(255 - (i * 20), (i * 40), (j * 30)),
                        WidthRequest = brickWidth - 2,
                        HeightRequest = brickHeight - 2
                    };
                    AbsoluteLayout.SetLayoutBounds(brick, new Rect(j * brickWidth + (i * (brickWidth / 2)), i * brickHeight + 1, brick.WidthRequest, brick.HeightRequest));
                    AbsoluteLayout.SetLayoutFlags(brick, AbsoluteLayoutFlags.None);
                    GameLayout.Children.Add(brick);
                    bricks.Add(brick);
                }
            }
        }

        private void ResetBonuses()
        {
            BonusLife.IsVisible = false;
            BonusMultiBall.IsVisible = false;
            BonusPaddleExpand.IsVisible = false;
            Paddle.WidthRequest = 100; // Повертаємо ширину ракетки до початкової
        }

        private bool GameLoop()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                var ball = balls[i];
                ballX = AbsoluteLayout.GetLayoutBounds(ball).X;
                ballY = AbsoluteLayout.GetLayoutBounds(ball).Y;

                ballX += ballSpeedsX[i];
                ballY += ballSpeedsY[i];

                if (ballX <= 0 || ballX + ball.WidthRequest >= screenWidth)
                {
                    ballSpeedsX[i] = -ballSpeedsX[i];
                }

                if (ballY <= 0)
                {
                    ballSpeedsY[i] = -ballSpeedsY[i];
                }

                Rect paddleRect = new Rect(paddleX, screenHeight - 50, Paddle.WidthRequest, Paddle.HeightRequest);
                Rect ballRect = new Rect(ballX, ballY, ball.WidthRequest, ball.HeightRequest);

                if (ballRect.IntersectsWith(paddleRect))
                {
                    ballSpeedsY[i] = -ballSpeedsY[i];
                }

                foreach (var brick in bricks.ToList())
                {
                    Rect brickRect = AbsoluteLayout.GetLayoutBounds(brick);

                    if (ballRect.IntersectsWith(brickRect))
                    {
                        ballSpeedsY[i] = -ballSpeedsY[i];
                        GameLayout.Children.Remove(brick);
                        bricks.Remove(brick);

                        if (random.NextDouble() < 0.3)
                        {
                            CreateBonus(brickRect);
                        }

                        score += 10;
                        ScoreLabel.Text = $"Рахунок: {score}";
                        break;
                    }
                }

                AbsoluteLayout.SetLayoutBounds(ball, new Rect(ballX, ballY, ball.WidthRequest, ball.HeightRequest));
            }

            CheckBonusCollision(BonusLife, "Life");
            CheckBonusCollision(BonusMultiBall, "MultiBall");
            CheckBonusCollision(BonusPaddleExpand, "PaddleExpand");

            if (balls.Count == 0 || balls.All(b => AbsoluteLayout.GetLayoutBounds(b).Y > screenHeight))
            {
                lives--;
                LivesLabel.Text = $"Життя: {lives}";

                if (lives == 0)
                {
                    EndGame("Ви програли!");
                    return false;
                }
                else
                {
                    SetupGame();
                }
            }

            if (bricks.Count == 0)
            {
                EndGame("Вітаємо! Ви виграли!");
                return false;
            }

            return true;
        }

        private void EndGame(string message)
        {
            DisplayAlert("Гра закінчена", message, "OK");
            StartMenu.IsVisible = true;
            GameLayout.IsVisible = false;
            ScorePanel.IsVisible = false;
        }

        private void CreateBonus(Rect brickRect)
        {
            int bonusType = random.Next(0, 3);
            BoxView bonus = null;

            switch (bonusType)
            {
                case 0:
                    bonus = BonusLife;
                    break;
                case 1:
                    bonus = BonusMultiBall;
                    break;
                case 2:
                    bonus = BonusPaddleExpand;
                    break;
            }

            if (bonus != null)
            {
                AbsoluteLayout.SetLayoutBounds(bonus, new Rect(brickRect.X, brickRect.Y, bonus.WidthRequest, bonus.HeightRequest));
                bonus.IsVisible = true;
            }
        }

        private void CheckBonusCollision(BoxView bonus, string bonusType)
        {
            if (bonus.IsVisible)
            {
                Rect bonusRect = AbsoluteLayout.GetLayoutBounds(bonus);
                Rect paddleRect = new Rect(paddleX, screenHeight - 50, Paddle.WidthRequest, Paddle.HeightRequest);

                if (bonusRect.IntersectsWith(paddleRect))
                {
                    ApplyBonus(bonusType);
                    bonus.IsVisible = false;
                }
                else if (bonusRect.Y > screenHeight)
                {
                    bonus.IsVisible = false;
                }
                else
                {
                    AbsoluteLayout.SetLayoutBounds(bonus, new Rect(bonusRect.X, bonusRect.Y + 5, bonusRect.Width, bonusRect.Height)); // Рух бонусу вниз
                }
            }
        }

        private void ApplyBonus(string bonusType)
        {
            switch (bonusType)
            {
                case "Life":
                    lives++;
                    LivesLabel.Text = $"Життя: {lives}";
                    break;
                case "MultiBall":
                    AddBall();
                    break;
                case "PaddleExpand":
                    Paddle.WidthRequest *= 1.5;
                    break;
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    double newX = paddleX + e.TotalX;

                    if (newX < 0)
                        newX = 0;
                    if (newX + Paddle.WidthRequest > screenWidth)
                        newX = screenWidth - Paddle.WidthRequest;

                    paddleX = newX;
                    AbsoluteLayout.SetLayoutBounds(Paddle, new Rect(paddleX, screenHeight - 50, Paddle.WidthRequest, Paddle.HeightRequest));
                    break;
            }
        }

        private void OnEasyButtonClicked(object sender, EventArgs e)
        {
            StartMenu.IsVisible = false;
            GameLayout.IsVisible = true;
            ScorePanel.IsVisible = true;
            SetupGame();
            ballSpeedX = 4;
            ballSpeedY = -4;
            Device.StartTimer(TimeSpan.FromMilliseconds(16), GameLoop);
        }

        private void OnMediumButtonClicked(object sender, EventArgs e)
        {
            StartMenu.IsVisible = false;
            GameLayout.IsVisible = true;
            ScorePanel.IsVisible = true;
            SetupGame();
            ballSpeedX = 6;
            ballSpeedY = -6;
            Device.StartTimer(TimeSpan.FromMilliseconds(16), GameLoop);
        }

        private void OnHardButtonClicked(object sender, EventArgs e)
        {
            StartMenu.IsVisible = false;
            GameLayout.IsVisible = true;
            ScorePanel.IsVisible = true;
            SetupGame();
            ballSpeedX = 8;
            ballSpeedY = -8;
            Device.StartTimer(TimeSpan.FromMilliseconds(16), GameLoop);
        }

        private void OnRetryButtonClicked(object sender, EventArgs e)
        {
            StartMenu.IsVisible = true;
            GameLayout.IsVisible = false;
            ScorePanel.IsVisible = false;
        }
    }
}
