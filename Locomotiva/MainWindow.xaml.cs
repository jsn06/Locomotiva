using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Locomotiva
{
    public partial class MainWindow : Window
    {
        private RotateTransform _wheel1Rotate;
        private RotateTransform _wheel2Rotate;
        private RotateTransform _wheel3Rotate;
        private RotateTransform _bielaRotate;
        private TranslateTransform _bielaTranslate;

        // Novas posições para 3 rodas
        private double _wheelRadius = 30;
        private double _wheel1CenterX = 150; // Canvas.Left=120 + 30
        private double _wheel1CenterY = 200; // Canvas.Top=170 + 30
        private double _wheel2CenterX = 250; // Canvas.Left=220 + 30
        private double _wheel2CenterY = 200;
        private double _wheel3CenterX = 350; // Canvas.Left=320 + 30
        private double _wheel3CenterY = 200;
        private double _bielaLength;

        private DoubleAnimation _wheelAnimation;
        private DispatcherTimer _timer;
        private DateTime _startTime;
        private DoubleAnimation _locomotiveAnimation;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Acessa os RotateTransform das rodas corretamente
            _wheel1Rotate = (RotateTransform)Wheel1.Template.FindName("WheelRotate", Wheel1);
            _wheel2Rotate = (RotateTransform)Wheel2.Template.FindName("WheelRotate", Wheel2);
            _wheel3Rotate = (RotateTransform)Wheel3.Template.FindName("WheelRotate", Wheel3);

            // Acessa os transforms da biela
            var tg = (TransformGroup)Biela.RenderTransform;
            _bielaRotate = (RotateTransform)tg.Children[0];
            _bielaTranslate = (TranslateTransform)tg.Children[1];

            // Calcula o comprimento da biela (entre as rodas externas)
            _bielaLength = Math.Sqrt(Math.Pow(_wheel3CenterX - _wheel1CenterX, 2) + Math.Pow(_wheel3CenterY - _wheel1CenterY, 2));

            // Animação contínua das rodas
            _wheelAnimation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(1)))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            _wheel1Rotate.BeginAnimation(RotateTransform.AngleProperty, _wheelAnimation);
            _wheel2Rotate.BeginAnimation(RotateTransform.AngleProperty, _wheelAnimation);
            _wheel3Rotate.BeginAnimation(RotateTransform.AngleProperty, _wheelAnimation);

            // Timer para atualizar a biela
            _startTime = DateTime.Now;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += UpdateBiela;
            _timer.Start();

            // Start horizontal locomotive animation based on runtime sizes
            StartLocomotiveAnimation();
        }

        private void UpdateBiela(object sender, EventArgs e)
        {
            // Calcula o ângulo atual das rodas (em radianos)
            double elapsed = (DateTime.Now - _startTime).TotalSeconds;
            double angleDeg = (elapsed * 360) % 360;
            double angleRad = angleDeg * Math.PI / 180.0;

            // Ponto de conexão na borda da roda 1 (externa esquerda)
            double rodOffset = 20;
            double x1 = _wheel1CenterX + rodOffset * Math.Cos(angleRad);
            double y1 = _wheel1CenterY + rodOffset * Math.Sin(angleRad);

            // Ponto de conexão na borda da roda 3 (externa direita)
            double x3 = _wheel3CenterX + rodOffset * Math.Cos(angleRad);
            double y3 = _wheel3CenterY + rodOffset * Math.Sin(angleRad);

            // Atualiza a posição e rotação da biela (entre as rodas externas)
            double dx = x3 - x1;
            double dy = y3 - y1;
            double bielaAngle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            // Move a biela para o ponto de conexão da roda 1
            _bielaTranslate.X = x1 - 150; // 150 = Canvas.Left da biela
            _bielaTranslate.Y = y1 - 200; // 200 = Canvas.Top da biela

            // Rotaciona a biela para apontar para a roda 3
            _bielaRotate.Angle = bielaAngle;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StartLocomotiveAnimation();
        }

        private void StartLocomotiveAnimation()
        {
            LocomotiveTranslate.BeginAnimation(TranslateTransform.XProperty, null);

            double containerWidth = 0;
            if (this.Content is Grid grid)
                containerWidth = grid.ActualWidth;
            else
                containerWidth = this.ActualWidth;

            double locomotiveWidth = LocomotiveCanvas.ActualWidth > 0 ? LocomotiveCanvas.ActualWidth : LocomotiveCanvas.Width;
            double maxX = Math.Max(0, containerWidth - locomotiveWidth);

            if (maxX <= 0)
            {
                LocomotiveTranslate.X = 0;
                return;
            }

            double seconds = Math.Max(2.0, maxX / 100.0);
            var duration = new Duration(TimeSpan.FromSeconds(seconds));

            _locomotiveAnimation = new DoubleAnimation
            {
                From = 0,
                To = maxX,
                Duration = duration,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            LocomotiveTranslate.X = 0;
            LocomotiveTranslate.BeginAnimation(TranslateTransform.XProperty, _locomotiveAnimation);
        }
    }
}