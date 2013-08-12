using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using GameAI.RRT;

namespace GameAI {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private readonly List<Obstacle> _obstacles;
        private readonly RrtGenerator _rrt;

        public MainWindow() {
            InitializeComponent();
            _rrt = new RrtGenerator((int) DrawCanvas.Height, (int) DrawCanvas.Width) {
                // Start = new Point(1, 1), 
                //Target = new Point((int) DrawCanvas.Height - 1, (int) DrawCanvas.Width - 1)
            };

            _obstacles = new List<Obstacle>();

            // if start an target is setted, set to false, false
            _rrt.CalculateRrt(true, true, _obstacles);
            RrtBranch.BranchLength = 25;
            UpdateDrawing();
        }

        private void UpdateDrawing() {
            DrawCanvas.Children.Clear();
            if (_rrt == null) return;
            foreach (Shape shape in _rrt.GetTreeShapes()) DrawCanvas.Children.Add(shape);
            if (_obstacles != null)
                foreach (Obstacle obstacle in _obstacles) DrawCanvas.Children.Add(obstacle.GetShape());

            Time.Content = _rrt.LastGenerationTime;
            double length = 0;
            if (_rrt.FinalPath.Count > 0)
                length = RrtGenerator.CalculateDistance(_rrt.FinalPath[0].Location, _rrt.FinalPath[_rrt.FinalPath.Count - 1].Location);
            Path.Content = _rrt.FinalPath.Count + " length in px: " + length;
            Tree.Content = _rrt.TotalBranches();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            _rrt.CalculateRrt(true, true, _obstacles);
            UpdateDrawing();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e) {
            _obstacles.Clear();
            _rrt.CalculateRrt(false, false, _obstacles);
            UpdateDrawing();
        }

        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Point click = e.GetPosition(DrawCanvas);
            // check if click is in canvas
            if (!(click.X > 0) || !(click.Y > 0) || !(click.X < DrawCanvas.Width) || !(click.Y < DrawCanvas.Height)) return;
            if (StartButton.IsChecked != null && StartButton.IsChecked.Value)
                _rrt.Start = click;
            else _rrt.Target = click;
            _rrt.CalculateRrt(false, false, _obstacles);
            UpdateDrawing();
        }

        private void DrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            Point click = e.GetPosition(DrawCanvas);
            // check if click is in canvas
            if (!(click.X > 0) || !(click.Y > 0) || !(click.X < DrawCanvas.Width) || !(click.Y < DrawCanvas.Height)) return;
            _obstacles.Add(new Obstacle(click, (int)RadiusSlider.Value));

            _rrt.CalculateRrt(false, false, _obstacles);
            UpdateDrawing();
        }
    }
}