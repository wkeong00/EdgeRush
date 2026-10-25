using UnityEngine;

namespace EdgeRush
{
    public enum Sfx
    {
        Click,
        Whoosh,
        Coin,
        Combo,
        Crash,
        Lane,
        Record,
        Revive,
        Buy,
    }

    /// <summary>
    /// Four stem music player (layers fade in with speed), pooled one-shots and an engine loop.
    /// Clips are synthesised at startup unless a same-named clip exists in Resources/Audio.
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector I { get; private set; }

        const int LayerCount = 4;
        static readonly string[] LayerNames = { "music_bass", "music_arp", "music_lead", "music_perc" };

        AudioSource[] _music;
        AudioSource[] _sfx;
        AudioSource _engine;
        AudioClip[] _sfxClips;
        AudioClip _engineClip;

        readonly float[] _layerVol = new float[LayerCount];
        readonly float[] _layerTarget = new float[LayerCount];

        bool _musicOn = true;
        bool _sfxOn = true;
        bool _ducked;
        bool _musicPlaying;
        float _speedT;
        float _engineVol;

        public void Init(bool musicOn, bool sfxOn)
        {
            I = this;
            _musicOn = musicOn;
            _sfxOn = sfxOn;

            _music = new AudioSource[LayerCount];
            for (int i = 0; i < LayerCount; i++)
            {
                _music[i] = gameObject.AddComponent<AudioSource>();
                _music[i].playOnAwake = false;
                _music[i].loop = true;
                _music[i].spatialBlend = 0f;
                _music[i].volume = 0f;
                var clip = Resources.Load<AudioClip>("Audio/" + LayerNames[i]);
                if (clip == null) clip = Synth.MusicLayer(i, LayerNames[i]);
                _music[i].clip = clip;
            }

            _sfx = new AudioSource[6];
            for (int i = 0; i < _sfx.Length; i++)
            {
                _sfx[i] = gameObject.AddComponent<AudioSource>();
                _sfx[i].playOnAwake = false;
                _sfx[i].spatialBlend = 0f;
            }

            _engine = gameObject.AddComponent<AudioSource>();
            _engine.playOnAwake = false;
            _engine.loop = true;
            _engine.spatialBlend = 0f;
            _engine.volume = 0f;
            _engineClip = Resources.Load<AudioClip>("Audio/engine_loop");
            if (_engineClip == null) _engineClip = Synth.EngineLoop();
            _engine.clip = _engineClip;

            _sfxClips = new AudioClip[]
            {
                LoadOrMake("sfx_click", Synth.SfxClick),
                LoadOrMake("sfx_whoosh", Synth.SfxWhoosh),
                LoadOrMake("sfx_coin", Synth.SfxCoin),
                LoadOrMake("sfx_combo", Synth.SfxCombo),
                LoadOrMake("sfx_crash", Synth.SfxCrash),
                LoadOrMake("sfx_lane", Synth.SfxLane),
                LoadOrMake("sfx_record", Synth.SfxRecord),
                LoadOrMake("sfx_revive", Synth.SfxRevive),
                LoadOrMake("sfx_buy", Synth.SfxBuy),
            };
        }

        static AudioClip LoadOrMake(string name, System.Func<AudioClip> make)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + name);
            return clip != null ? clip : make();
        }

        public void StartMusic()
        {
            if (_musicPlaying || _music == null) return;
            _musicPlaying = true;
            double t0 = AudioSettings.dspTime + 0.15;
            for (int i = 0; i < LayerCount; i++)
            {
                if (_music[i].clip == null) continue;
                _music[i].PlayScheduled(t0);
            }
            ApplyMusicVolumes(true);
        }

        public void StopMusic(float fade = 0.25f)
        {
            _musicPlaying = false;
            for (int i = 0; i < LayerCount; i++)
                if (_music[i] != null) _music[i].Stop();
        }

        public void SetSpeedT(float t)
        {
            _speedT = Mathf.Clamp01(t);
            _layerTarget[0] = 0.55f + 0.08f * _speedT;
            _layerTarget[1] = 0.16f + 0.30f * _speedT;
            _layerTarget[2] = 0.02f + 0.30f * Mathf.Max(0f, _speedT - 0.15f);
            _layerTarget[3] = 0.20f + 0.32f * _speedT;
        }

        void Update()
        {
            if (_music == null) return;
            for (int i = 0; i < LayerCount; i++)
            {
                if (_music[i] == null) continue;
                float target = _musicOn ? _layerTarget[i] : 0f;
                _layerVol[i] = Mathf.MoveTowards(_layerVol[i], target, Time.unscaledDeltaTime * 0.9f);
                _music[i].volume = _layerVol[i];
            }

            if (_engine != null)
            {
                float target = _engineVol * (_sfxOn ? 1f : 0f);
                _engine.volume = Mathf.MoveTowards(_engine.volume, target, Time.unscaledDeltaTime * 0.8f);
                _engine.pitch = 0.75f + 0.85f * _speedT;
            }
        }

        void ApplyMusicVolumes(bool immediate)
        {
            for (int i = 0; i < LayerCount; i++)
            {
                if (_music[i] == null) continue;
                _layerVol[i] = immediate ? 0f : _layerVol[i];
                _music[i].volume = _layerVol[i];
            }
        }

        public void SetMusicEnabled(bool on)
        {
            _musicOn = on;
            if (!on) ApplyMusicVolumes(true);
        }

        public void SetSfxEnabled(bool on)
        {
            _sfxOn = on;
        }

        public void DuckForAd(bool ducked)
        {
            _ducked = ducked;
            AudioListener.volume = ducked ? 0f : 1f;
        }

        public void SetEngine(bool playing, float speedT)
        {
            _speedT = Mathf.Clamp01(speedT);
            _engineVol = playing ? 0.16f : 0f;
            if (_engine == null || _engine.clip == null) return;
            if (playing && !_engine.isPlaying) _engine.Play();
            if (!playing && _engine.isPlaying && _engine.volume < 0.002f) _engine.Stop();
        }

        public void Play(Sfx sfx, float volume = 1f)
        {
            if (!_sfxOn || _ducked || _sfx == null || _sfxClips == null) return;
            int idx = (int)sfx;
            if (idx < 0 || idx >= _sfxClips.Length || _sfxClips[idx] == null) return;

            for (int i = 0; i < _sfx.Length; i++)
            {
                var src = _sfx[i];
                if (src.isPlaying) continue;
                src.volume = 1f;
                src.PlayOneShot(_sfxClips[idx], Mathf.Clamp01(volume));
                return;
            }
            _sfx[0].PlayOneShot(_sfxClips[idx], Mathf.Clamp01(volume));
        }

        public void PlayUiClick() => Play(Sfx.Click, 0.55f);
    }
}

