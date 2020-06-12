using Godot;

namespace Overlords.game.entities.player.local
{
    [Tool]
    public class AspectContainer : Container
    {
        private Vector2 _ratio = new Vector2(1, 1);
        
        [Export]
        private Vector2 EditorRatio
        {
            get => _ratio;
            set
            {
                _ratio = value;
                QueueSort();
            }
        }

        public override void _Notification(int what)
        {
            if (what != NotificationSortChildren) return;
            var rect = GetRect();
            RectMinSize = _ratio * (rect.Size.x / _ratio.x);
            FitChildInRect((Control) GetChild(0), new Rect2(Vector2.Zero, RectMinSize));
        }
    }
}