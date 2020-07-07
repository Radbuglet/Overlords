using System;
using Godot;

namespace Overlords.game.entities.player.gui
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
            RectMinSize = _ratio * Math.Min(rect.Size.x / _ratio.x, rect.Size.y / _ratio.y);
            FitChildInRect((Control) GetChild(0), new Rect2((RectSize - RectMinSize) / 2, RectMinSize));
        }
    }
}