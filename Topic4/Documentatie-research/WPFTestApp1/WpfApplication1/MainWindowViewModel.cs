using System.IO;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace WpfApplication1 {
    public class MainWindowViewModel {
        public MainWindowViewModel() {
            var c =
                new FileStream(
                    @"G:\Projects\School\pbaict_1213_MMT_glenn.latomme\Topic4\Documentatie-research\WPFTestApp1\Models\Test.obj",
                    FileMode.Open, FileAccess.Read, FileShare.Read);
            var reader = new StudioReader();
            reader.TexturePath = ".";
            MyModel = reader.Read(c);
        }

        public Model3D MyModel { get; set; }
    }
}