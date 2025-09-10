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
        private RotateTransform _roda1Rotacao;
        private RotateTransform _roda2Rotacao;
        private RotateTransform _roda3Rotacao;
        private RotateTransform _bielaRotacao;
        private TranslateTransform _bielaTranslacao;

        private double _raioRoda = 30;
        private double _centroRoda1X = 150;
        private double _centroRoda1Y = 200;
        private double _centroRoda2X = 250;
        private double _centroRoda2Y = 200;
        private double _centroRoda3X = 350;
        private double _centroRoda3Y = 200;
        private double _comprimentoBiela;

        private DispatcherTimer _temporizador;
        private DateTime _inicio;

        private DoubleAnimation _animacaoLocomotiva;
        private double _ultimaPosXLocomotiva = 0;
        private int _direcao = 1; // 1 = direita, -1 = esquerda
        private double _anguloRoda = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Carregado;
            SizeChanged += MainWindow_TamanhoAlterado;
        }

        private void MainWindow_Carregado(object sender, RoutedEventArgs e)
        {
            _roda1Rotacao = (RotateTransform)Wheel1.Template.FindName("WheelRotate", Wheel1);
            _roda2Rotacao = (RotateTransform)Wheel2.Template.FindName("WheelRotate", Wheel2);
            _roda3Rotacao = (RotateTransform)Wheel3.Template.FindName("WheelRotate", Wheel3);

            var grupoTransformacao = (TransformGroup)Biela.RenderTransform;
            _bielaRotacao = (RotateTransform)grupoTransformacao.Children[0];
            _bielaTranslacao = (TranslateTransform)grupoTransformacao.Children[1];

            _comprimentoBiela = Math.Sqrt(Math.Pow(_centroRoda3X - _centroRoda1X, 2) + Math.Pow(_centroRoda3Y - _centroRoda1Y, 2));

            _inicio = DateTime.Now;
            _temporizador = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _temporizador.Tick += AtualizarAnimacao;
            _temporizador.Start();

            IniciarAnimacaoLocomotiva();
        }

        private void AtualizarAnimacao(object sender, EventArgs e)
        {
            // Detecta direção do movimento
            double posXAtual = LocomotiveTranslate.X;
            if (posXAtual > _ultimaPosXLocomotiva + 0.1)
                _direcao = 1; // Direita
            else if (posXAtual < _ultimaPosXLocomotiva - 0.1)
                _direcao = -1; // Esquerda

            _ultimaPosXLocomotiva = posXAtual;

            // Atualiza ângulo das rodas conforme direção
            double tempoDecorrido = (DateTime.Now - _inicio).TotalSeconds;
            double velocidade = 240; // graus por segundo
            _anguloRoda += _direcao * velocidade * _temporizador.Interval.TotalSeconds;
            _anguloRoda %= 360;

            _roda1Rotacao.Angle = _anguloRoda;
            _roda2Rotacao.Angle = _anguloRoda;
            _roda3Rotacao.Angle = _anguloRoda;

            // Atualiza biela (usando o ângulo atual)
            double anguloRad = _anguloRoda * Math.PI / 180.0;
            double deslocamentoBiela = 20;
            double x1 = _centroRoda1X + deslocamentoBiela * Math.Cos(anguloRad);
            double y1 = _centroRoda1Y + deslocamentoBiela * Math.Sin(anguloRad);
            double x3 = _centroRoda3X + deslocamentoBiela * Math.Cos(anguloRad);
            double y3 = _centroRoda3Y + deslocamentoBiela * Math.Sin(anguloRad);

            double dx = x3 - x1;
            double dy = y3 - y1;
            double anguloBiela = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            _bielaTranslacao.X = x1 - 150;
            _bielaTranslacao.Y = y1 - 200;
            _bielaRotacao.Angle = anguloBiela;
        }

        private void MainWindow_TamanhoAlterado(object sender, SizeChangedEventArgs e)
        {
            IniciarAnimacaoLocomotiva();
        }

        private void IniciarAnimacaoLocomotiva()
        {
            LocomotiveTranslate.BeginAnimation(TranslateTransform.XProperty, null);

            double larguraContainer = 0;
            if (this.Content is Grid grid)
                larguraContainer = grid.ActualWidth;
            else
                larguraContainer = this.ActualWidth;

            double larguraLocomotiva = LocomotiveCanvas.ActualWidth > 0 ? LocomotiveCanvas.ActualWidth : LocomotiveCanvas.Width;
            double maxX = Math.Max(0, larguraContainer - larguraLocomotiva);

            if (maxX <= 0)
            {
                LocomotiveTranslate.X = 0;
                return;
            }

            double segundos = Math.Max(2.0, maxX / 100.0);
            var duracao = new Duration(TimeSpan.FromSeconds(segundos));

            _animacaoLocomotiva = new DoubleAnimation
            {
                From = 0,
                To = maxX,
                Duration = duracao,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            LocomotiveTranslate.X = 0;
            LocomotiveTranslate.BeginAnimation(TranslateTransform.XProperty, _animacaoLocomotiva);
        }
    }
}