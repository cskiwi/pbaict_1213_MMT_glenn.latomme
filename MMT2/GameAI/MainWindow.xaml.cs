using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using GameAI.RRT;

namespace GameAI {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private readonly RrtGenerator _rrt;

        public MainWindow() {
            InitializeComponent();
            _rrt = new RrtGenerator((int) DrawCanvas.Height, (int) DrawCanvas.Width) {
                //Start = new Point(1, 1), 
                //Target = new Point((int) DrawCanvas.Height - 1, (int) DrawCanvas.Width - 1)
            };

            // if start an target is setted, set to false, false
            _rrt.CalculateRrt(true, true);
            RrtBranch.BranchLength = 25;
            UpdateDrawing();
        }

        private void UpdateDrawing() {
            DrawCanvas.Children.Clear();
            foreach (Shape shape in _rrt.GetTreeShapes()) DrawCanvas.Children.Add(shape);
            Time.Content = _rrt.LastGenerationTime;
            var length = RrtGenerator.CalculateDistance(_rrt.FinalPath[0].Location, _rrt.FinalPath[_rrt.FinalPath.Count - 1].Location);
            Path.Content = _rrt.FinalPath.Count + " length in px: " + length;
            Tree.Content = _rrt.TotalBranches();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            _rrt.CalculateRrt(true, true);
            UpdateDrawing();
        }

        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Point click = e.GetPosition(DrawCanvas);
            // check if click is in canvas
            if (!(click.X > 0) || !(click.Y > 0) || !(click.X < DrawCanvas.Width) || !(click.Y < DrawCanvas.Height)) return;
            _rrt.Target = click;
            _rrt.CalculateRrt(false, false);
            UpdateDrawing();
        }

        private void DrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            Point click = e.GetPosition(DrawCanvas);
            // check if click is in canvas
            if (!(click.X > 0) || !(click.Y > 0) || !(click.X < DrawCanvas.Width) || !(click.Y < DrawCanvas.Height)) return;
            _rrt.Start = click;
            _rrt.CalculateRrt(false, false);
            UpdateDrawing();
        }
    }
}