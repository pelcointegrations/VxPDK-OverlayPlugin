using System;

namespace PluginNs.Services.CanvasOverlay.Types.Shapes
{
    class BaseShape : IEquatable<BaseShape>
    {
        public string Id { get; }
        public bool IsDirty { get; set; }

        private bool _delete;

        public BaseShape()
        {
            Id = Guid.NewGuid().ToString();
            IsDirty = true;
        }

        public bool Delete
        {
            get => _delete;
            set
            {
                if (value)
                {
                    _delete = value;
                    IsDirty = true;
                }
            }
        }

        public bool Equals(BaseShape other)
        {
            if (other == null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseShape);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
