using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace Locomotiva
{
    public partial class MainWindow : Window
    {
        private RotateTransform _wheel1Rotate;
        private RotateTransform _wheel2Rotate;
        private RotateTransform _wheel3Rotate;
        private RotateTransform _bielaRotate;
        private TranslateTransform _bielaTranslate;

        private double _wheelRadius = 30;
        private double _wheel1CenterX = 150;
        private double _wheel1CenterY = 200;
        private double _wheel2CenterX = 250;
        private double _wheel2CenterY = 200;
        private double _wheel3CenterX = 350;
        private double _wheel3CenterY = 200;
        private double _bielaLength;

        private DispatcherTimer _timer;
        private DateTime _startTime;

        private DoubleAnimation _locomotiveAnimation;
        private double _lastLocomotiveX = 0;
        private int _direction = 1; // 1 = direita, -1 = esquerda
        private double _wheelAngle = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            DrawTrack();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _wheel1Rotate = (RotateTransform)Wheel1.Template.FindName("WheelRotate", Wheel1);
            _wheel2Rotate = (RotateTransform)Wheel2.Template.FindName("WheelRotate", Wheel2);
            _wheel3Rotate = (RotateTransform)Wheel3.Template.FindName("WheelRotate", Wheel3);

            var tg = (TransformGroup)Biela.RenderTransform;
            _bielaRotate = (RotateTransform)tg.Children[0];
            _bielaTranslate = (TranslateTransform)tg.Children[1];

            _bielaLength = Math.Sqrt(Math.Pow(_wheel3CenterX - _wheel1CenterX, 2) + Math.Pow(_wheel3CenterY - _wheel1CenterY, 2));

            _startTime = DateTime.Now;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += UpdateAnimation;
            _timer.Start();

            StartLocomotiveAnimation();
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            // Detecta direção do movimento
            double currentX = LocomotiveTranslate.X;
            if (currentX > _lastLocomotiveX + 0.1)
                _direction = 1; // Direita
            else if (currentX < _lastLocomotiveX - 0.1)
                _direction = -1; // Esquerda

            _lastLocomotiveX = currentX;

            // Atualiza ângulo das rodas conforme direção
            double elapsed = (DateTime.Now - _startTime).TotalSeconds;
            double speed = 240; // graus por segundo (aumentado de 180 para 240)
            _wheelAngle += _direction * speed * _timer.Interval.TotalSeconds;
            _wheelAngle %= 360;

            _wheel1Rotate.Angle = _wheelAngle;
            _wheel2Rotate.Angle = _wheelAngle;
            _wheel3Rotate.Angle = _wheelAngle;

            // Atualiza biela (usando o ângulo atual)
            double angleRad = _wheelAngle * Math.PI / 180.0;
            double rodOffset = 20;
            double x1 = _wheel1CenterX + rodOffset * Math.Cos(angleRad);
            double y1 = _wheel1CenterY + rodOffset * Math.Sin(angleRad);
            double x3 = _wheel3CenterX + rodOffset * Math.Cos(angleRad);
            double y3 = _wheel3CenterY + rodOffset * Math.Sin(angleRad);

            double dx = x3 - x1;
            double dy = y3 - y1;
            double bielaAngle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            _bielaTranslate.X = x1 - 150;
            _bielaTranslate.Y = y1 - 200;
            _bielaRotate.Angle = bielaAngle;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StartLocomotiveAnimation();
            DrawTrack();
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

        private void DrawTrack()
        {
            TrackCanvas.Children.Clear();

            double width = this.ActualWidth;
            double railTop1 = 236;
            double railTop2 = 254;
            double sleeperTop = 245;
            double sleeperHeight = 12;
            double sleeperWidth = 60;
            double sleeperSpacing = 40;

            // Ajusta altura do trilho conforme a janela
            double canvasHeight = LocomotiveCanvas.Height + 60;
            TrackCanvas.Width = width;
            TrackCanvas.Height = canvasHeight;

            // Trilhos (rails) - aparência metálica
            var railBrush = new LinearGradientBrush();
            railBrush.StartPoint = new Point(0, 0.5);
            railBrush.EndPoint = new Point(1, 0.5);
            railBrush.GradientStops.Add(new GradientStop(Colors.DimGray, 0.0));
            railBrush.GradientStops.Add(new GradientStop(Colors.LightGray, 0.3));
            railBrush.GradientStops.Add(new GradientStop(Colors.Gainsboro, 0.5));
            railBrush.GradientStops.Add(new GradientStop(Colors.LightGray, 0.7));
            railBrush.GradientStops.Add(new GradientStop(Colors.DimGray, 1.0));

            Rectangle rail1 = new Rectangle
            {
                Fill = railBrush,
                Height = 8,
                Width = width
            };
            Canvas.SetTop(rail1, railTop1);
            TrackCanvas.Children.Add(rail1);

            Rectangle rail2 = new Rectangle
            {
                Fill = railBrush,
                Height = 8,
                Width = width
            };
            Canvas.SetTop(rail2, railTop2);
            TrackCanvas.Children.Add(rail2);

            // Dormentes (sleepers) - aparência de madeira
            var sleeperBrush = new LinearGradientBrush();
            sleeperBrush.StartPoint = new Point(0, 0.5);
            sleeperBrush.EndPoint = new Point(1, 0.5);
            sleeperBrush.GradientStops.Add(new GradientStop(Colors.SaddleBrown, 0.0));
            sleeperBrush.GradientStops.Add(new GradientStop(Colors.Peru, 0.5));
            sleeperBrush.GradientStops.Add(new GradientStop(Colors.Sienna, 1.0));
            for (double x = 0; x < width; x += sleeperSpacing)
            {
                Rectangle sleeper = new Rectangle
                {
                    Fill = sleeperBrush,
                    Width = sleeperWidth,
                    Height = sleeperHeight,
                    RadiusX = 3,
                    RadiusY = 3,
                    Stroke = Brushes.Sienna,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(sleeper, x);
                Canvas.SetTop(sleeper, sleeperTop);
                TrackCanvas.Children.Add(sleeper);
            }
        }
    }
}