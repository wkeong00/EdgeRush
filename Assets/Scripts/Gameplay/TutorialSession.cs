using System;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>An isolated practice road. It never creates a scored RunSession.</summary>
    public sealed class TutorialSession
    {
        public const int Version = 1;
        readonly PlayerCar _player;
        readonly ObstacleManager _traffic;
        readonly RoadView _road;
        Obstacle _practiceCar;
        bool _tagged, _advance, _retry;
        public int Step { get; private set; }
        public event Action<int> StepChanged;
        public event Action Completed;
        public event Action<string> Retried;

        public TutorialSession(PlayerCar player, ObstacleManager traffic, RoadView road)
        {
            _player = player; _traffic = traffic; _road = road;
        }

        public void Start()
        {
            _player.ResetForRun();
            _traffic.ResetForRun();
            _road.ResetForRun();
            Step = 0;
            _advance = _tagged = _retry = false;
            _player.LaneChangeFinished += OnArrived;
            _player.LaneChangeStarted += OnLeaving;
            StepChanged?.Invoke(Step);
        }

        public void Stop()
        {
            _player.LaneChangeFinished -= OnArrived;
            _player.LaneChangeStarted -= OnLeaving;
        }

        void OnArrived(int lane)
        {
            if ((Step == 0 && lane == 0) || (Step == 1 && lane == 1)) _advance = true;
        }

        void OnLeaving(int from)
        {
            if (Step != 2 || _practiceCar == null || from != 1) return;
            if (_practiceCar.y - _player.Y <= Tuning.NearMissWindow &&
                _practiceCar.y > _player.Y + 3f) _tagged = true;
            else _retry = true;
        }

        public void Tick(float dt)
        {
            _player.Tick(dt);
            if (_retry)
            {
                _retry = false;
                ResetCloseCall();
                Retried?.Invoke("WAIT UNTIL TRAFFIC IS CLOSE");
            }
            if (_advance)
            {
                _advance = false;
                Step++;
                if (Step == 2) ResetCloseCall();
                StepChanged?.Invoke(Step);
            }
            float speed = Step == 2 && !_tagged && _practiceCar != null &&
                _practiceCar.y <= _player.Y + 4.8f ? 0f : 4f;
            _traffic.Tick(dt, speed);
            _road.Tick(dt, speed, 0f);
            if (Step != 2 || _practiceCar == null) return;

            // Reversing into the practice car retries the same step, without death or economy events.
            float px, py, hw, hh;
            _player.GetBounds(out px, out py, out hw, out hh);
            if (Mathf.Abs(px - _practiceCar.x) <= hw + _practiceCar.halfW * Tuning.ObstacleHitShrink &&
                Mathf.Abs(py - _practiceCar.y) <= hh + _practiceCar.halfH * Tuning.ObstacleHitShrink)
            {
                ResetCloseCall();
                StepChanged?.Invoke(Step);
                Analytics.Event("tutorial_retry", "step=near_miss");
                Retried?.Invoke("TRY AGAIN - MOVE AWAY FROM THE CAR");
                return;
            }
            if (_practiceCar.Top < py - hh)
            {
                if (_tagged)
                {
                    Step = 3;
                    Completed?.Invoke();
                }
                else ResetCloseCall();
            }
        }

        void ResetCloseCall()
        {
            _traffic.ResetForRun();
            _player.ResetForRun();
            _tagged = false;
            _practiceCar = _traffic.Spawn(ObstacleKind.Car, 1, _player.Y + 10f);
        }
    }
}
