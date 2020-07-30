using System.Windows;

namespace PluginNs.Services.CanvasOverlay.Types.Shapes
{
    class RectangleShape : FillableShape
    {
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }

        public RectangleShape()
        { }
    }
}
