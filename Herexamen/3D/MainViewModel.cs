using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace _3D {
    public class MainViewModel {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel() {
            var worldGroup = new Model3DGroup();
            var fishGroup = new Model3DGroup();
            var waterGroup = new Model3DGroup();

            Model3D world = ModelImporter.Load(@".\Models\1.obj");
            Model3D fish = ModelImporter.Load(@".\Models\2.obj");
            Model3D water = ModelImporter.Load(@".\Models\3.obj");


            worldGroup.Children.Add(world);
            fishGroup.Children.Add(fish);
            waterGroup.Children.Add(water);

            WorldModel = worldGroup;
            FishModel = fishGroup;
            WaterModel = waterGroup;
        }

        public Model3D WorldModel { get; set; }
        public Model3D FishModel { get; set; }
        public Model3D WaterModel { get; set; }
    }
}