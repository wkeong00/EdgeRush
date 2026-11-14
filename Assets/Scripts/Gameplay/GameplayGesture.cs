namespace EdgeRush
{
    /// <summary>One pointer owns a gesture; a swipe fires immediately and cannot become a tap.</summary>
    public sealed class GameplayGesture
    {
        bool _active, _consumed;
        int _pointer;
        float _x, _y, _started;

        public void Begin(int pointer, float x, float y, float now)
        {
            if (_active) return;
            _active = true;
            _consumed = false;
            _pointer = pointer;
            _x = x;
            _y = y;
            _started = now;
        }

        public int Move(int pointer, float x, float y, float threshold)
        {
            if (!_active || _consumed || pointer != _pointer) return 0;
            float dx = x - _x, dy = y - _y;
            if (System.Math.Abs(dx) <= threshold || System.Math.Abs(dx) <= System.Math.Abs(dy)) return 0;
            _consumed = true;
            return dx > 0f ? 1 : -1;
        }

        public int End(int pointer, float x, float y, float now, float width, float threshold)
        {
            if (!_active || pointer != _pointer) return 0;
            int direction = Move(pointer, x, y, threshold);
            if (direction == 0 && !_consumed && now - _started < 0.32f &&
                System.Math.Abs(x - _x) < threshold && System.Math.Abs(y - _y) < threshold)
                direction = x < width * 0.5f ? -1 : 1;
            Cancel();
            return direction;
        }

        public void Cancel() { _active = false; _consumed = false; }
    }
}
