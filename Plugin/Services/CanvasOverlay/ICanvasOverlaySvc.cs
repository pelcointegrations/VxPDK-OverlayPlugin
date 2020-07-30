using Pelco.Phoenix.PluginHostInterfaces;
using PluginNs.Services.CanvasOverlay.Types.Shapes;

namespace PluginNs.Services.CanvasOverlay
{
    interface ICanvasOverlaySvc : IOCCPluginVideoOverlay
    {
        void CreateShape(BaseShape shape);
        void UpdateShape(BaseShape shape);
        void DeleteShape(BaseShape shape);
    }
}
