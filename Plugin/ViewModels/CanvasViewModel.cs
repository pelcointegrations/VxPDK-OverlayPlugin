using NLog;
using System.Windows.Media.Imaging;

namespace PluginNs.ViewModels
{
    class CanvasViewModel : BaseViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private WriteableBitmap _overlayBitmap;
        private double _centerX;
        private double _centerY;
        private double _angle;

        public CanvasViewModel()
        { }

        public WriteableBitmap OverlayBitmap
        {
            get => _overlayBitmap;
            set => SetProperty(ref _overlayBitmap, value);
        }

        public double Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }

        public double CenterX
        {
            get => _centerX;
            set => SetProperty(ref _centerX, value);
        }

        public double CenterY
        {
            get => _centerY;
            set => SetProperty(ref _centerY, value);
        }

        public void CreateBitmap(double width, double height, double angle)
        {
            var bitmap = BitmapFactory.New((int)width, (int)height);
            OverlayBitmap = BitmapFactory.ConvertToPbgra32Format(bitmap);

            Angle = angle;
            CenterX = width / 2.0;
            CenterY = height / 2.0;
        }
    }
}
