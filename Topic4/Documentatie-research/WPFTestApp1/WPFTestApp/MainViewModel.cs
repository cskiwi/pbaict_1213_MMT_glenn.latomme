using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace WPFTestApp {
    /// <summary>
    ///     Provides a ViewModel for the Main window.
    /// </summary>
    public class MainViewModel {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel() {
            var modelGroup = new Model3DGroup();
            Model3D world = ModelImporter.Load(@".\Models\WorldV3.obj");
            modelGroup.Children.Add(world);
            ModelInViewPort = modelGroup;
        }

        /// <summary>
        ///     Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public Model3D ModelInViewPort { get; set; }
    }
}