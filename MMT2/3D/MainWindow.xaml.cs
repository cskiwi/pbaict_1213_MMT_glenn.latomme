using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace _3D {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            DataContext = new MainViewModel();

        }

 
    }
}