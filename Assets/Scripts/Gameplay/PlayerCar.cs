using System;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// The player's car: three lanes, snappy tweened lane changes, tilt, input buffering and
    /// the invulnerability flicker used after a revive.
    /// </summary>
    public class PlayerCar : MonoBehaviour
    {
        public event Action<int> LaneChangeStarted;   // parameter: lane we are leaving
        public event Action<int> LaneChangeFinished;  // parameter: lane we arrived in

        SpriteRenderer _sr;
        SpriteRenderer _glow;
        SpriteRenderer _beam;
        SkinDef _skin;

        int _lane = 1;
        int _pendingDir;
        float _fromX, _toX, _tweenT;
        bool _moving;
        float _tilt;
        float _invulnUntil;
        float _clock;

        public int Lane => _lane;
        public float X => transform.position.x;
        public float Y => transform.position.y;
        public bool IsInvulnerable => Time.time < _invulnUntil;
        public SkinDef Skin => _skin;

        public void Build()
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 55;

            var glowGo = new GameObject("Glow", typeof(SpriteRenderer));
            glowGo.transform.SetParent(transform, false);
            _glow = glowGo.GetComponent<SpriteRenderer>();
            _glow.sprite = Art.Glow();
            _glow.sortingOrder = 54;
            var additive = Resources.Load<Shader>("Shaders/Additive");
            if (additive != null) _glow.sharedMaterial = new Material(additive);
            var b = Art.Glow().bounds.size;
            float scale = Layout.CarW * 3.1f / Mathf.Max(0.001f, b.x);
            _glow.transform.localScale = new Vector3(scale, scale * 1.15f, 1f);
            _glow.color = new Color(1f, 1f, 1f, 0.30f);

            var beamGo = new GameObject("Beam", typeof(SpriteRenderer));
            beamGo.transform.SetParent(transform, false);
            _beam = beamGo.GetComponent<SpriteRenderer>();
            _beam.sprite = Art.Beam();
            _beam.sortingOrder = 53;
            if (additive != null) _beam.sharedMaterial = new Material(additive);
            _beam.color = new Color(1f, 1f, 1f, 0.10f);
            var bb = Art.Beam().bounds.size;
            _beam.transform.localPosition = new Vector3(0f, Layout.CarH * 1.15f, 0f);
            _beam.transform.localScale = new Vector3(Layout.CarW * 1.7f / Mathf.Max(0.001f, bb.x),
                                                    Layout.CarH * 2.0f / Mathf.Max(0.001f, bb.y), 1f);
        }

        public void Setup(SkinDef skin, bool animate = true)
        {
            _skin = skin;
            if (_sr == null) Build();
            _sr.sprite = Art.Car(skin);
            _glow.color = new Color(skin.glow.r, skin.glow.g, skin.glow.b, 0.30f);
            _beam.color = new Color(skin.glow.r, skin.glow.g, skin.glow.b, 0.09f);
            if (animate) StartCoroutine(PopIn());
        }

        System.Collections.IEnumerator PopIn()
        {
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0.7f, 1f, Mathf.Clamp01(t / 0.25f));
                transform.localScale = new Vector3(k, k, 1f);
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        public void ResetForRun()
        {
            _lane = 1;
            _moving = false;
            _pendingDir = 0;
            _invulnUntil = 0f;
            _tweenT = 0f;
            transform.position = new Vector3(Layout.LaneToX(1), Layout.PlayerY, 0f);
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            if (_sr != null) _sr.color = Color.white;
        }

        public void Move(int dir)
        {
            if (dir == 0) return;

            if (_moving)
            {
                _pendingDir = dir;
                return;
            }

            int target = _lane + dir;
            if (target < 0 || target >= Layout.LaneCount)
            {
                // Already at the edge: give feedback but do nothing.
                Haptics.Light();
                _tilt = dir * 0.18f;
                return;
            }

            StartMoveTo(target);
        }

        void StartMoveTo(int target)
        {
            int from = _lane;
            _lane = target;
            _fromX = transform.position.x;
            _toX = Layout.LaneToX(target);
            _tweenT = 0f;
            _moving = true;
            if (LaneChangeStarted != null) LaneChangeStarted(from);
            if (AudioDirector.I != null) AudioDirector.I.Play(Sfx.Lane, 0.35f);
        }

        public void Tick(float dt)
        {
            _clock += dt;

            if (_moving)
            {
                _tweenT += dt / Mathf.Max(0.01f, Tuning.LaneChangeTime);
                float k = Mathf.Clamp01(_tweenT);
                // ease out cube for a snappy but smooth feel
                float e = 1f - Mathf.Pow(1f - k, 3f);
                float x = Mathf.Lerp(_fromX, _toX, e);
                transform.position = new Vector3(x, transform.position.y, 0f);
                _tilt = Mathf.Lerp(_tilt, (x - _toX) * -0.22f, 0.55f);

                if (k >= 1f)
                {
                    _moving = false;
                    _tweenT = 0f;
                    if (LaneChangeFinished != null) LaneChangeFinished(_lane);
                    if (_pendingDir != 0)
                    {
                        int d = _pendingDir;
                        _pendingDir = 0;
                        Move(d);
                    }
                }
            }
            else
            {
                _tilt = Mathf.Lerp(_tilt, 0f, 1f - Mathf.Exp(-9f * dt));
            }

            transform.rotation = Quaternion.Euler(0f, 0f, _tilt * 57.2958f);

            // invulnerability flicker
            if (_sr != null)
            {
                bool flick = IsInvulnerable && (Mathf.Repeat(_clock, 0.16f) < 0.08f);
                _sr.color = flick ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
            }

            if (_glow != null)
            {
                float pulse = 0.28f + 0.05f * Mathf.Sin(_clock * 3.1f);
                var c = _glow.color;
                _glow.color = new Color(c.r, c.g, c.b, pulse);
            }
        }

        public void SetInvulnerable(float seconds)
        {
            _invulnUntil = Mathf.Max(_invulnUntil, Time.time + seconds);
        }

        /// <summary>World-space AABB used for collision (already shrunk for fairness).</summary>
        public void GetBounds(out float cx, out float cy, out float hw, out float hh)
        {
            cx = transform.position.x;
            cy = transform.position.y;
            hw = Tuning.PlayerHitHalfW;
            hh = Tuning.PlayerHitHalfH;
        }

        public void Nudge(float dir)
        {
            _tilt = dir * 0.1f;
        }
    }
}
