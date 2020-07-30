using System.Windows.Media;

namespace PluginNs.Services.CanvasOverlay.Types.Shapes
{
    class FillableShape : BaseShape
    {
        public Color BorderColor { get; set; }
        public Color FillColor { get; set; }

        public FillableShape()
        { }
    }
}
