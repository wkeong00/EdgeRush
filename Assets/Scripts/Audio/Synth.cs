using System;
using UnityEngine;

namespace EdgeRush
{
    public enum Wave { Sine, Saw, Square, Triangle, Noise }

    /// <summary>
    /// Procedural audio: a small subtractive synth that renders the music stems and the sound
    /// effects at startup. No audio files ship with the game. If you later drop real clips into
    /// Assets/Resources/Audio with the same names, AudioDirector will prefer those instead.
    /// </summary>
    public static class Synth
    {
        public const int MusicSR = 32000;
        public const int SfxSR = 44100;
        public const float BPM = 128f;
        public const int BeatsPerLoop = 16;
        public static float SecondsPerBeat => 60f / BPM;
        public static float LoopSeconds => BeatsPerLoop * SecondsPerBeat;

        // ------------------------------------------------------------- core
        public static AudioClip ToClip(string name, float[] samples, int sr)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, sr, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static float Osc(Wave wave, float phase, System.Random rnd)
        {
            switch (wave)
            {
                case Wave.Sine: return Mathf.Sin(phase * Mathf.PI * 2f);
                case Wave.Saw: return 2f * (phase - Mathf.Floor(phase + 0.5f));
                case Wave.Square: return phase % 1f < 0.5f ? 1f : -1f;
                case Wave.Triangle: return 4f * Mathf.Abs(phase % 1f - 0.5f) - 1f;
                default: return (float)(rnd.NextDouble() * 2.0 - 1.0);
            }
        }

        /// <summary>
        /// Adds one note. Wraps around the end of the buffer so looping music has no seam.
        /// </summary>
        public static void Note(float[] buf, int sr, float startSec, float durSec, float freqStart, float freqEnd,
                                float amp, Wave wave, float attack = 0.005f, float decay = 0.4f,
                                float lowpassHz = 12000f, float drive = 1f, int seed = 1)
        {
            if (buf == null || buf.Length == 0) return;
            int n = buf.Length;
            int start = Mathf.RoundToInt(startSec * sr);
            int len = Mathf.RoundToInt(durSec * sr);
            if (len <= 0) return;

            var rnd = new System.Random(seed);
            float phase = 0f;
            float lp = 0f;
            float invSr = 1f / sr;
            float nyquist = sr * 0.45f;

            for (int i = 0; i < len; i++)
            {
                float u = i / (float)len;
                float f = Mathf.Lerp(freqStart, freqEnd, u);
                f = Mathf.Clamp(f, 20f, nyquist);
                phase += f * invSr;
                if (phase > 1000f) phase -= Mathf.Floor(phase);

                float s = Osc(wave, phase, rnd);
                if (drive != 1f) s = (float)Math.Tanh(s * drive);

                // one pole low pass
                float a = 1f - Mathf.Exp(-2f * Mathf.PI * lowpassHz * invSr);
                lp += (s - lp) * Mathf.Clamp01(a);
                s = lp;

                float t = i * invSr;
                float env = Mathf.Clamp01(t / Mathf.Max(0.0005f, attack));
                env *= Mathf.Exp(-t / Mathf.Max(0.01f, decay));
                env *= 1f - Mathf.Clamp01((t - (durSec - 0.02f)) / 0.02f); // click-free end

                int idx = (start + i) % n;
                if (idx < 0) idx += n;
                buf[idx] += s * env * amp;
            }
        }

        static float[] Buffer(int sr, float seconds)
        {
            return new float[Mathf.RoundToInt(sr * seconds)];
        }

        static void Normalize(float[] buf, float peak)
        {
            float max = 0.0001f;
            for (int i = 0; i < buf.Length; i++) max = Mathf.Max(max, Mathf.Abs(buf[i]));
            float k = peak / max;
            if (k >= 1f) k = Mathf.Min(k, 4f);
            for (int i = 0; i < buf.Length; i++) buf[i] = (float)Math.Tanh(buf[i] * k * 0.85f);
        }

        // ------------------------------------------------------------ music
        // Am - F - C - G, one bar each, four bars per loop.
        static readonly float[] BassRoots = { 110.00f, 87.31f, 130.81f, 98.00f };
        static readonly float[][] ArpNotes =
        {
            new[] { 220.00f, 261.63f, 329.63f, 440.00f },  // Am
            new[] { 174.61f, 220.00f, 261.63f, 349.23f },  // F
            new[] { 261.63f, 329.63f, 392.00f, 523.25f },  // C
            new[] { 196.00f, 246.94f, 293.66f, 392.00f },  // G
        };
        static readonly float[][] LeadNotes =
        {
            new[] { 659.25f, 523.25f, 440.00f },
            new[] { 523.25f, 440.00f, 349.23f },
            new[] { 783.99f, 659.25f, 523.25f },
            new[] { 587.33f, 493.88f, 392.00f },
        };

        public static AudioClip MusicLayer(int layer, string name)
        {
            int sr = MusicSR;
            var buf = Buffer(sr, LoopSeconds);
            float spb = SecondsPerBeat;

            for (int bar = 0; bar < 4; bar++)
            {
                float barStart = bar * 4f * spb;

                if (layer == 0)
                {
                    // bass groove
                    float[] offsets = { 0f, 0.75f, 1.5f, 2.5f, 3f };
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        float f = BassRoots[bar];
                        Note(buf, sr, barStart + offsets[i] * spb, 0.42f, f, f * 0.985f, 0.55f,
                             Wave.Saw, 0.004f, 0.30f, 420f, 1.3f, 7 + i);
                        Note(buf, sr, barStart + offsets[i] * spb, 0.42f, f * 0.5f, f * 0.5f, 0.30f,
                             Wave.Sine, 0.006f, 0.36f, 200f, 1f, 3);
                    }
                }
                else if (layer == 1)
                {
                    // 16th note arpeggio
                    int[] pattern = { 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3 };
                    for (int step = 0; step < 16; step++)
                    {
                        float t = barStart + step * spb * 0.25f;
                        float f = ArpNotes[bar][pattern[step]];
                        Note(buf, sr, t, 0.16f, f, f, 0.20f, Wave.Triangle, 0.003f, 0.12f, 5200f, 1f, step);
                        Note(buf, sr, t, 0.16f, f * 2.001f, f * 2.001f, 0.07f, Wave.Saw, 0.002f, 0.09f, 7000f, 1f, step + 1);
                    }
                }
                else if (layer == 2)
                {
                    // lead phrase
                    float[] beats = { 0f, 1.5f, 2.5f };
                    float[] durs = { 0.55f, 0.42f, 0.36f };
                    for (int i = 0; i < 3; i++)
                    {
                        float f = LeadNotes[bar][i];
                        float t = barStart + beats[i] * spb;
                        Note(buf, sr, t, durs[i], f, f * 0.999f, 0.22f, Wave.Saw, 0.02f, 0.55f, 3800f, 1.1f, 11 + i);
                        Note(buf, sr, t, durs[i], f * 1.005f, f * 1.005f, 0.10f, Wave.Triangle, 0.03f, 0.5f, 4200f, 1f, 15 + i);
                        // echo
                        Note(buf, sr, t + spb * 0.5f, durs[i] * 0.8f, f, f, 0.06f, Wave.Saw, 0.02f, 0.45f, 3000f, 1f, 19 + i);
                    }
                }
                else
                {
                    // percussion
                    for (int beat = 0; beat < 4; beat++)
                    {
                        float t = barStart + beat * spb;
                        // kick
                        Note(buf, sr, t, 0.16f, 130f, 42f, 0.55f, Wave.Sine, 0.001f, 0.12f, 1200f, 1f, 23 + beat);
                        // hat on every 8th
                        Note(buf, sr, t, 0.035f, 9000f, 9000f, 0.10f, Wave.Noise, 0.001f, 0.025f, 12000f, 1f, 31 + beat);
                        Note(buf, sr, t + spb * 0.5f, 0.03f, 11000f, 11000f, 0.07f, Wave.Noise, 0.001f, 0.02f, 14000f, 1f, 41 + beat);
                    }
                    // backbeat
                    Note(buf, sr, barStart + spb, 0.14f, 600f, 400f, 0.16f, Wave.Noise, 0.001f, 0.09f, 5200f, 1f, 51);
                    Note(buf, sr, barStart + spb * 3f, 0.14f, 600f, 400f, 0.16f, Wave.Noise, 0.001f, 0.09f, 5200f, 1f, 52);
                }
            }

            Normalize(buf, layer == 0 ? 0.85f : (layer == 3 ? 0.55f : 0.65f));

            // tiny fade at the loop seam to hide any discontinuity
            int fade = Mathf.Min(64, buf.Length / 8);
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                buf[i] *= k;
                buf[buf.Length - 1 - i] *= k;
            }

            return ToClip(name, buf, sr);
        }

        // -------------------------------------------------------------- sfx
        public static AudioClip SfxWhoosh()
        {
            var buf = Buffer(SfxSR, 0.30f);
            var rnd = new System.Random(5);
            float lp = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float u = i / (float)buf.Length;
                float cutoff = Mathf.Lerp(6500f, 700f, u * u);
                float a = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SfxSR);
                lp += ((float)(rnd.NextDouble() * 2 - 1) - lp) * a;
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(u * 1.15f));
                buf[i] = lp * env * 0.55f;
            }
            return ToClip("sfx_whoosh", buf, SfxSR);
        }

        public static AudioClip SfxCoin()
        {
            var buf = Buffer(SfxSR, 0.20f);
            Note(buf, SfxSR, 0f, 0.08f, 1046.5f, 1046.5f, 0.35f, Wave.Triangle, 0.002f, 0.06f, 9000f, 1f, 2);
            Note(buf, SfxSR, 0.055f, 0.14f, 1568f, 1568f, 0.30f, Wave.Triangle, 0.002f, 0.09f, 9000f, 1f, 3);
            Normalize(buf, 0.8f);
            return ToClip("sfx_coin", buf, SfxSR);
        }

        public static AudioClip SfxCombo()
        {
            var buf = Buffer(SfxSR, 0.34f);
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            for (int i = 0; i < notes.Length; i++)
                Note(buf, SfxSR, i * 0.055f, 0.18f, notes[i], notes[i], 0.26f, Wave.Square, 0.002f, 0.10f, 6000f, 1f, i);
            Normalize(buf, 0.85f);
            return ToClip("sfx_combo", buf, SfxSR);
        }

        public static AudioClip SfxCrash()
        {
            var buf = Buffer(SfxSR, 0.75f);
            var rnd = new System.Random(11);
            float lp = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float u = i / (float)buf.Length;
                float cutoff = Mathf.Lerp(4200f, 380f, u);
                float a = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SfxSR);
                lp += ((float)(rnd.NextDouble() * 2 - 1) - lp) * a;
                buf[i] += lp * Mathf.Exp(-u * 6f) * 0.9f;
            }
            Note(buf, SfxSR, 0f, 0.55f, 120f, 28f, 0.9f, Wave.Sine, 0.001f, 0.30f, 1400f, 1.2f, 4);
            Note(buf, SfxSR, 0f, 0.30f, 1800f, 900f, 0.18f, Wave.Square, 0.001f, 0.12f, 7000f, 1f, 5);
            Normalize(buf, 0.95f);
            return ToClip("sfx_crash", buf, SfxSR);
        }

        public static AudioClip SfxClick()
        {
            var buf = Buffer(SfxSR, 0.07f);
            Note(buf, SfxSR, 0f, 0.05f, 1500f, 1100f, 0.30f, Wave.Square, 0.001f, 0.02f, 8000f, 1f, 6);
            Normalize(buf, 0.55f);
            return ToClip("sfx_click", buf, SfxSR);
        }

        public static AudioClip SfxLane()
        {
            var buf = Buffer(SfxSR, 0.13f);
            var rnd = new System.Random(8);
            float lp = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float u = i / (float)buf.Length;
                float a = 1f - Mathf.Exp(-2f * Mathf.PI * 2600f / SfxSR);
                lp += ((float)(rnd.NextDouble() * 2 - 1) - lp) * a;
                buf[i] = lp * Mathf.Sin(Mathf.PI * u) * 0.28f;
            }
            return ToClip("sfx_lane", buf, SfxSR);
        }

        public static AudioClip SfxRecord()
        {
            var buf = Buffer(SfxSR, 0.9f);
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f };
            for (int i = 0; i < notes.Length; i++)
            {
                Note(buf, SfxSR, i * 0.085f, 0.42f, notes[i], notes[i], 0.24f, Wave.Triangle, 0.004f, 0.28f, 8000f, 1f, i);
                Note(buf, SfxSR, i * 0.085f, 0.30f, notes[i] * 2f, notes[i] * 2f, 0.08f, Wave.Sine, 0.004f, 0.18f, 9000f, 1f, i + 9);
            }
            Normalize(buf, 0.85f);
            return ToClip("sfx_record", buf, SfxSR);
        }

        public static AudioClip SfxRevive()
        {
            var buf = Buffer(SfxSR, 0.6f);
            Note(buf, SfxSR, 0f, 0.55f, 220f, 880f, 0.30f, Wave.Saw, 0.01f, 0.4f, 4000f, 1f, 3);
            for (int i = 0; i < 5; i++)
                Note(buf, SfxSR, 0.05f + i * 0.06f, 0.25f, 1200f + i * 220f, 1400f + i * 220f, 0.10f,
                     Wave.Triangle, 0.005f, 0.15f, 9000f, 1f, i);
            Normalize(buf, 0.8f);
            return ToClip("sfx_revive", buf, SfxSR);
        }

        public static AudioClip SfxBuy()
        {
            var buf = Buffer(SfxSR, 0.4f);
            Note(buf, SfxSR, 0f, 0.12f, 880f, 880f, 0.30f, Wave.Square, 0.002f, 0.06f, 8000f, 1f, 2);
            Note(buf, SfxSR, 0.1f, 0.25f, 1320f, 1320f, 0.28f, Wave.Square, 0.002f, 0.12f, 8000f, 1f, 3);
            Normalize(buf, 0.7f);
            return ToClip("sfx_buy", buf, SfxSR);
        }

        /// <summary>Seamless one second engine hum, pitch-bent at runtime.</summary>
        public static AudioClip EngineLoop()
        {
            int sr = 22050;
            int n = sr; // exactly one second so it loops cleanly
            var buf = new float[n];
            var rnd = new System.Random(13);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                float f = 55f + 6f * Mathf.Sin(2f * Mathf.PI * t * 2f);
                float saw = 2f * ((t * f) % 1f) - 1f;
                float noise = (float)(rnd.NextDouble() * 2 - 1);
                float a = 1f - Mathf.Exp(-2f * Mathf.PI * 240f / sr);
                lp += (saw * 0.6f + noise * 0.25f - lp) * a;
                // crossfade the seam
                float seam = Mathf.Min(1f, Mathf.Min(t, 1f - t) * 40f);
                buf[i] = lp * 0.5f * seam;
            }
            return ToClip("engine_loop", buf, sr);
        }
    }
}
