using System.Windows;
using ReflectionTaskManager.ViewModels;

namespace ReflectionTaskManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}