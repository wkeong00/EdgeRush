using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    public enum ObstacleKind
    {
        Car,
        Truck,
        Barrier,
        Mover,
    }

    public class Obstacle
    {
        public ObstacleKind kind;
        public GameObject go;
        public SpriteRenderer sr;
        public float x;
        public float y;
        public float halfW;
        public float halfH;
        public float speedFactor = 1f;
        public bool active;
        public bool nearMissTagged;
        public bool awarded;
        public Color tint;

        // moving cars
        public bool oscillates;
        public float baseX;
        public float phase;
        public float frequency;
        public float amplitude;

        public float Left => x - halfW;
        public float Right => x + halfW;
        public float Top => y + halfH;
        public float Bottom => y - halfH;
    }

    public class CoinPickup
    {
        public GameObject go;
        public SpriteRenderer sr;
        public float x, y;
        public bool active;
        public float bob;
    }

    /// <summary>Pools and moves traffic, barriers, moving cars and coins.</summary>
    public class ObstacleManager : MonoBehaviour
    {
        readonly List<Obstacle> _pool = new List<Obstacle>();
        readonly List<CoinPickup> _coinPool = new List<CoinPickup>();
        readonly List<Obstacle> _active = new List<Obstacle>();
        readonly List<CoinPickup> _activeCoins = new List<CoinPickup>();

        Transform _root;
        float _clock;

        public IReadOnlyList<Obstacle> Active => _active;
        public IReadOnlyList<CoinPickup> ActiveCoins => _activeCoins;

        public void Build()
        {
            _root = new GameObject("Obstacles").transform;
            _root.SetParent(transform, false);
        }

        Obstacle Take()
        {
            for (int i = 0; i < _pool.Count; i++)
                if (!_pool[i].active) return _pool[i];

            var go = new GameObject("obs", typeof(SpriteRenderer));
            go.transform.SetParent(_root, false);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 50;
            var o = new Obstacle { go = go, sr = sr };
            _pool.Add(o);
            return o;
        }

        public Obstacle Spawn(ObstacleKind kind, int lane, float y)
        {
            var o = Take();
            o.kind = kind;
            o.active = true;
            o.awarded = false;
            o.nearMissTagged = false;
            o.oscillates = false;
            o.phase = 0f;
            o.speedFactor = 1f;

            switch (kind)
            {
                case ObstacleKind.Car:
                    o.sr.sprite = Art.TrafficCar(Random.Range(0, 3));
                    o.halfW = Layout.CarW * 0.5f;
                    o.halfH = Layout.CarH * 0.5f;
                    o.speedFactor = 0.92f;
                    o.tint = Color.white;
                    break;
                case ObstacleKind.Truck:
                    o.sr.sprite = Art.Truck(Random.Range(0, 2));
                    o.halfW = Layout.TruckW * 0.5f;
                    o.halfH = Layout.TruckH * 0.5f;
                    o.speedFactor = 0.88f;
                    o.tint = Color.white;
                    break;
                case ObstacleKind.Barrier:
                    o.sr.sprite = Art.Barrier();
                    o.halfW = Layout.BarrierW * 0.5f;
                    o.halfH = Layout.BarrierH * 0.5f;
                    o.speedFactor = 1f;
                    o.tint = Color.white;
                    break;
                default:
                    o.sr.sprite = Art.MoverCar();
                    o.halfW = Layout.CarW * 0.5f;
                    o.halfH = Layout.CarH * 0.5f;
                    o.speedFactor = 0.9f;
                    o.oscillates = true;
                    o.phase = Random.Range(0f, Mathf.PI * 2f);
                    o.frequency = Random.Range(0.45f, 0.75f);
                    o.amplitude = Layout.LaneWidth * Random.Range(0.5f, 0.75f);
                    o.tint = Color.white;
                    break;
            }

            o.x = Layout.LaneToX(lane);
            o.baseX = o.x;
            o.y = y;
            o.sr.color = o.tint;
            o.sr.enabled = true;
            o.go.SetActive(true);
            ApplyTransform(o);
            _active.Add(o);
            return o;
        }

        static void ApplyTransform(Obstacle o)
        {
            o.go.transform.localPosition = new Vector3(o.x, o.y, 0f);
        }

        public CoinPickup SpawnCoin(int lane, float y)
        {
            CoinPickup c = null;
            for (int i = 0; i < _coinPool.Count; i++)
                if (!_coinPool[i].active) { c = _coinPool[i]; break; }

            if (c == null)
            {
                var go = new GameObject("coin", typeof(SpriteRenderer));
                go.transform.SetParent(_root, false);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = Art.Coin();
                sr.sortingOrder = 52;
                c = new CoinPickup { go = go, sr = sr };
                _coinPool.Add(c);
            }

            c.active = true;
            c.x = Layout.LaneToX(lane);
            c.y = y;
            c.bob = Random.Range(0f, Mathf.PI * 2f);
            c.sr.enabled = true;
            c.go.SetActive(true);
            c.go.transform.localPosition = new Vector3(c.x, c.y, 0f);
            _activeCoins.Add(c);
            return c;
        }

        public void Tick(float dt, float speed)
        {
            _clock += dt;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var o = _active[i];
                o.y -= speed * o.speedFactor * dt;

                if (o.oscillates)
                {
                    o.phase += dt * o.frequency * Mathf.PI * 2f;
                    o.x = Mathf.Clamp(o.baseX + Mathf.Sin(o.phase) * o.amplitude,
                                      -Layout.RoadHalf + o.halfW, Layout.RoadHalf - o.halfW);
                }

                ApplyTransform(o);

                if (o.y < Layout.DespawnY)
                {
                    Despawn(o);
                    _active.RemoveAt(i);
                }
            }

            for (int i = _activeCoins.Count - 1; i >= 0; i--)
            {
                var c = _activeCoins[i];
                c.y -= speed * dt;
                c.bob += dt * 6f;
                float sc = 1f + Mathf.Sin(c.bob) * 0.06f;
                c.go.transform.localPosition = new Vector3(c.x, c.y, 0f);
                c.go.transform.localScale = new Vector3(sc, sc, 1f);
                if (c.y < Layout.DespawnY)
                {
                    DespawnCoin(c);
                    _activeCoins.RemoveAt(i);
                }
            }
        }

        void Despawn(Obstacle o)
        {
            o.active = false;
            o.sr.enabled = false;
            o.go.SetActive(false);
        }

        void DespawnCoin(CoinPickup c)
        {
            c.active = false;
            c.sr.enabled = false;
            c.go.SetActive(false);
        }

        public void Remove(Obstacle o)
        {
            if (o == null || !o.active) return;
            _active.Remove(o);
            Despawn(o);
        }

        public void CollectCoin(CoinPickup c)
        {
            if (c == null || !c.active) return;
            _activeCoins.Remove(c);
            DespawnCoin(c);
        }

        /// <summary>Removes everything within a radius ahead of the player (used on revive).</summary>
        public int ClearAround(float yFrom, float yTo)
        {
            int removed = 0;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var o = _active[i];
                if (o.y > yFrom && o.y < yTo)
                {
                    Despawn(o);
                    _active.RemoveAt(i);
                    removed++;
                }
            }
            return removed;
        }

        public void ResetForRun()
        {
            for (int i = 0; i < _pool.Count; i++) Despawn(_pool[i]);
            for (int i = 0; i < _coinPool.Count; i++) DespawnCoin(_coinPool[i]);
            _active.Clear();
            _activeCoins.Clear();
            _clock = 0f;
        }
    }
}
