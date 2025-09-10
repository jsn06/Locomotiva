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
        private RotateTransform rot1, rot2, rot3, rotBiela;
        private TranslateTransform movBiela;

        private double raio = 30;
        private double x1 = 150, y1 = 200;
        private double x2 = 250, y2 = 200;
        private double x3 = 350, y3 = 200;
        private double tamBiela;

        private DispatcherTimer timer;
        private DateTime inicio;

        private DoubleAnimation animacao;
        private double ultX = 0;
        private int direcao = 1; // 1 = direita, -1 = esquerda
        private double angulo = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += JanelaCarregada;
            SizeChanged += TamanhoMudou;
        }

        private void JanelaCarregada(object sender, RoutedEventArgs e)
        {
            rot1 = (RotateTransform)roda1.Template.FindName("RotacaoRoda", roda1);
            rot2 = (RotateTransform)roda2.Template.FindName("RotacaoRoda", roda2);
            rot3 = (RotateTransform)roda3.Template.FindName("RotacaoRoda", roda3);

            var grupo = (TransformGroup)biela.RenderTransform;
            rotBiela = (RotateTransform)grupo.Children[0];
            movBiela = (TranslateTransform)grupo.Children[1];

            tamBiela = Math.Sqrt(Math.Pow(x3 - x1, 2) + Math.Pow(y3 - y1, 2));

            inicio = DateTime.Now;
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += Atualiza;
            timer.Start();

            IniciaAnimacao();
        }

        private void Atualiza(object sender, EventArgs e)
        {
            // Descobre se está indo pra direita ou esquerda
            double xAtual = movLocomotiva.X;
            if (xAtual > ultX + 0.1)
                direcao = 1;
            else if (xAtual < ultX - 0.1)
                direcao = -1;

            ultX = xAtual;

            // Gira as rodas
            double vel = 240;
            angulo += direcao * vel * timer.Interval.TotalSeconds;
            angulo %= 360;

            rot1.Angle = angulo;
            rot2.Angle = angulo;
            rot3.Angle = angulo;

            // Move a biela junto
            double rad = angulo * Math.PI / 180.0;
            double off = 20;
            double bx1 = x1 + off * Math.Cos(rad);
            double by1 = y1 + off * Math.Sin(rad);
            double bx3 = x3 + off * Math.Cos(rad);
            double by3 = y3 + off * Math.Sin(rad);

            double dx = bx3 - bx1;
            double dy = by3 - by1;
            double angBiela = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            movBiela.X = bx1 - 150;
            movBiela.Y = by1 - 200;
            rotBiela.Angle = angBiela;
        }

        private void TamanhoMudou(object sender, SizeChangedEventArgs e)
        {
            IniciaAnimacao();
        }

        private void IniciaAnimacao()
        {
            movLocomotiva.BeginAnimation(TranslateTransform.XProperty, null);

            double largura = 0;
            if (this.Content is Grid grid)
                largura = grid.ActualWidth;
            else
                largura = this.ActualWidth;

            double larguraLocomotiva = tela.ActualWidth > 0 ? tela.ActualWidth : tela.Width;
            double maxX = Math.Max(0, largura - larguraLocomotiva);

            if (maxX <= 0)
            {
                movLocomotiva.X = 0;
                return;
            }

            double segundos = Math.Max(2.0, maxX / 100.0);
            var duracao = new Duration(TimeSpan.FromSeconds(segundos));

            animacao = new DoubleAnimation
            {
                From = 0,
                To = maxX,
                Duration = duracao,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            movLocomotiva.X = 0;
            movLocomotiva.BeginAnimation(TranslateTransform.XProperty, animacao);
        }
    }
}