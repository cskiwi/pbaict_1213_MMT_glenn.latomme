using System.Drawing;
using System.Windows.Forms;
using EngineV5;


namespace RoboLeague {
    public partial class GameForm : Form {
        private readonly RoboLeague _roboLeague;

        public GameForm() {
            InitializeComponent();
            const int width = 800;
            const int height = 600;

            Size = GamePanel.CalculateWindowBounds(new Size(width, height));

            _roboLeague = new RoboLeague(50f, width, height) {
                Dock = DockStyle.Fill
            };

            Controls.Add(_roboLeague);
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e) {
            _roboLeague.GameForm_KeyDown(sender, e);
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e) {
            _roboLeague.GameForm_KeyUp(sender, e);

        }
    }
}
