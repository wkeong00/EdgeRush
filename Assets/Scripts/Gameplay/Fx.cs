using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>Pooled 2D particles: sparks, coin bursts, crash debris, neon trails.</summary>
    public class FxManager : MonoBehaviour
    {
        class Particle
        {
            public SpriteRenderer sr;
            public Vector3 vel;
            public float life, maxLife, spin, drag, fadePower;
            public Color color;
            public float scaleFrom, scaleTo;
            public bool active;
        }

        readonly List<Particle> _pool = new List<Particle>();
        Transform _root;
        Material _normalMat;
        Material _additiveMat;

        public void Build(int prewarm = 90)
        {
            _root = new GameObject("FxParticles").transform;
            _root.SetParent(transform, false);

            var spriteShader = Resources.Load<Shader>("Shaders/SpriteTint");
            if (spriteShader == null) spriteShader = Shader.Find("Sprites/Default");
            var addShader = Resources.Load<Shader>("Shaders/Additive");
            if (addShader == null) addShader = Shader.Find("Sprites/Default");

            _normalMat = new Material(spriteShader) { name = "FxNormal" };
            _additiveMat = new Material(addShader) { name = "FxAdditive" };

            for (int i = 0; i < prewarm; i++) CreateParticle();
        }

        Particle CreateParticle()
        {
            var go = new GameObject("p", typeof(SpriteRenderer));
            go.transform.SetParent(_root, false);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Art.SoftDot();
            sr.enabled = false;
            go.SetActive(false);
            var p = new Particle { sr = sr, active = false, maxLife = 1f };
            _pool.Add(p);
            return p;
        }

        Particle Take()
        {
            for (int i = 0; i < _pool.Count; i++)
                if (!_pool[i].active) return _pool[i];
            return CreateParticle();
        }

        public void Burst(Vector3 pos, Color color, int count, float speedMin, float speedMax, float size,
                          float life = 0.5f, bool additive = true, float gravity = 0f, Sprite sprite = null)
        {
            for (int i = 0; i < count; i++)
            {
                var p = Take();
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float spd = Random.Range(speedMin, speedMax);
                p.vel = new Vector3(Mathf.Cos(ang) * spd, Mathf.Sin(ang) * spd, 0f) + Vector3.up * gravity;
                p.life = p.maxLife = life * Random.Range(0.7f, 1.3f);
                p.color = color;
                p.spin = Random.Range(-220f, 220f);
                p.drag = 2.5f;
                p.fadePower = 1.4f;
                p.scaleFrom = size * Random.Range(0.6f, 1.2f);
                p.scaleTo = p.scaleFrom * 0.15f;
                p.sr.sprite = sprite ?? Art.SoftDot();
                p.sr.sharedMaterial = additive ? _additiveMat : _normalMat;
                p.sr.color = color;
                p.sr.transform.position = pos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);
                p.sr.transform.localScale = Vector3.one * p.scaleFrom;
                p.sr.sortingOrder = 60;
                p.sr.enabled = true;
                p.sr.gameObject.SetActive(true);
                p.active = true;
            }
        }

        public void Sparks(Vector3 pos, Color color, int count = 12, float speed = 9f)
            => Burst(pos, color, count, speed * 0.4f, speed, 0.55f, 0.35f, true);

        public void CoinBurst(Vector3 pos)
        {
            Burst(pos, new Color(1f, 0.85f, 0.3f), 12, 2f, 6.5f, 0.45f, 0.45f, true);
            Burst(pos, Color.white, 4, 3f, 7f, 0.25f, 0.3f, true);
        }

        public void Crash(Vector3 pos, Color carColor)
        {
            Burst(pos, new Color(1f, 0.7f, 0.25f), 26, 3f, 14f, 0.7f, 0.8f, true);
            Burst(pos, carColor, 16, 2f, 10f, 0.6f, 0.7f, false, sprite: Art.White());
            Burst(pos, Color.white, 10, 1f, 6f, 0.5f, 0.55f, true);
        }

        public void NearMissTrail(Vector3 pos, Color color)
            => Burst(pos, color, 6, 1.5f, 5f, 0.4f, 0.3f, true);

        public void Tick(float dt)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                var p = _pool[i];
                if (!p.active) continue;
                p.life -= dt;
                if (p.life <= 0f)
                {
                    p.active = false;
                    p.sr.enabled = false;
                    p.sr.gameObject.SetActive(false);
                    continue;
                }
                float t = 1f - p.life / p.maxLife;
                p.vel *= Mathf.Max(0f, 1f - p.drag * dt);
                p.sr.transform.position += p.vel * dt;
                p.sr.transform.Rotate(0f, 0f, p.spin * dt);
                float s = Mathf.Lerp(p.scaleFrom, p.scaleTo, t);
                p.sr.transform.localScale = new Vector3(s, s, 1f);
                var c = p.color;
                c.a = Mathf.Pow(1f - t, p.fadePower);
                p.sr.color = c;
            }
        }

        public void ClearAll()
        {
            foreach (var p in _pool)
            {
                p.active = false;
                if (p.sr != null) p.sr.enabled = false;
            }
        }
    }

    /// <summary>Camera shake and the slight zoom-out at high speed.</summary>
    public class CameraRig : MonoBehaviour
    {
        public Camera Cam;
        float _baseSize;
        float _shakeAmp, _shakeTime, _shakeDur;

        public void Build(Camera cam)
        {
            Cam = cam;
            _baseSize = cam.orthographicSize;
        }

        public void Shake(float amplitude, float duration)
        {
            _shakeAmp = Mathf.Max(_shakeAmp, amplitude);
            _shakeDur = _shakeTime = Mathf.Max(_shakeTime, duration);
        }

        public void Tick(float dt, float speedT, bool playing)
        {
            float targetSize = _baseSize * (1f + 0.10f * Mathf.Clamp01(speedT));
            if (!playing) targetSize = _baseSize;
            Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, targetSize, 1f - Mathf.Exp(-3f * dt));

            Vector3 offset = Vector3.zero;
            if (_shakeTime > 0f)
            {
                _shakeTime -= dt;
                float k = Mathf.Clamp01(_shakeTime / Mathf.Max(0.01f, _shakeDur));
                float amp = _shakeAmp * k * k;
                offset = new Vector3(Random.Range(-amp, amp), Random.Range(-amp, amp), 0f);
                if (_shakeTime <= 0f) _shakeAmp = 0f;
            }
            Cam.transform.localPosition = new Vector3(0f, 0f, -10f) + offset;
        }
    }
}


