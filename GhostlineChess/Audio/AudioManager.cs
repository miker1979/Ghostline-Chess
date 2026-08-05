using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GhostlineChess.Audio
{
    /// <summary>
    /// Mixes a looping ambient bed with simultaneous
    /// chess-event effects through one Windows output.
    /// </summary>
    public sealed class AudioManager : IDisposable
    {
        private const int OutputSampleRate = 48_000;

        private readonly string audioDirectory;
        private readonly object audioLock = new object();
        private MixingSampleProvider? mixer;
        private WaveOutEvent? outputDevice;
        private readonly List<AudioFileReader> readers =
            new List<AudioFileReader>();
        private readonly
            Dictionary<VolumeSampleProvider, float>
                effectVolumes =
                    new Dictionary<
                        VolumeSampleProvider,
                        float>();

        private VolumeSampleProvider? ambientVolume;
        private int volume = 55;
        private bool muted;
        private bool initializing;
        private bool disposed;

        /// <summary>
        /// Stores the audio directory without opening an
        /// output device during form construction.
        /// </summary>
        public AudioManager(string audioDirectory)
        {
            this.audioDirectory = audioDirectory;
        }

        /// <summary>
        /// Opens the Windows output and starts the mixer.
        /// This is called after the game window is visible.
        /// </summary>
        public void Initialize()
        {
            lock (audioLock)
            {
                if (disposed ||
                    initializing ||
                    outputDevice != null)
                {
                    return;
                }

                initializing = true;
            }

            MixingSampleProvider newMixer =
                new MixingSampleProvider(
                    WaveFormat.CreateIeeeFloatWaveFormat(
                        OutputSampleRate,
                        2))
                {
                    ReadFully = true
                };

            WaveOutEvent newOutputDevice =
                new WaveOutEvent
                {
                    DesiredLatency = 120
                };

            try
            {
                newOutputDevice.Init(newMixer);
                newOutputDevice.Play();
            }
            catch
            {
                newOutputDevice.Dispose();

                lock (audioLock)
                {
                    initializing = false;
                }

                throw;
            }

            lock (audioLock)
            {
                initializing = false;

                if (disposed)
                {
                    newOutputDevice.Stop();
                    newOutputDevice.Dispose();
                    return;
                }

                mixer = newMixer;
                outputDevice = newOutputDevice;
            }
        }

        /// <summary>
        /// Gets or sets the shared game-audio volume
        /// as a value from zero through one hundred.
        /// </summary>
        public int Volume
        {
            get => volume;
            set
            {
                lock (audioLock)
                {
                    volume = Math.Clamp(value, 0, 100);

                    if (ambientVolume != null)
                    {
                        ambientVolume.Volume =
                            AmbientVolumeScale;
                    }

                    foreach (
                        KeyValuePair<
                            VolumeSampleProvider,
                            float> effect in
                        effectVolumes)
                    {
                        effect.Key.Volume =
                            EffectVolumeScale *
                            effect.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the final mixed output
        /// is muted.
        /// </summary>
        public bool Muted
        {
            get => muted;
            set
            {
                muted = value;
                if (outputDevice != null)
                {
                    outputDevice.Volume =
                        muted ? 0F : 1F;
                }
            }
        }

        private float AmbientVolumeScale =>
            volume / 100F * 0.50F;

        private float EffectVolumeScale =>
            volume / 100F;

        /// <summary>
        /// Adds the six-minute background suite as a
        /// permanent looping input to the mixer.
        /// </summary>
        public void StartAmbience()
        {
            lock (audioLock)
            {
                if (disposed ||
                    mixer == null ||
                    ambientVolume != null)
                {
                    return;
                }

                string fullPath =
                    Path.Combine(
                        audioDirectory,
                        "Ghostline_Background_Suite.mp3");

                if (!File.Exists(fullPath))
                {
                    return;
                }

                AudioFileReader reader =
                    new AudioFileReader(fullPath);

                readers.Add(reader);

                LoopingSampleProvider loopingProvider =
                    new LoopingSampleProvider(reader);

                ambientVolume =
                    new VolumeSampleProvider(
                        ConvertToMixerFormat(
                            loopingProvider))
                    {
                        Volume = AmbientVolumeScale
                    };

                mixer.AddMixerInput(ambientVolume);
            }
        }

        /// <summary>
        /// Adds an event effect to the mixer without
        /// interrupting the ambient input.
        /// </summary>
        public void PlayEffect(
            string fileName,
            float relativeVolume = 1F)
        {
            lock (audioLock)
            {
                if (disposed || mixer == null)
                {
                    return;
                }

                string fullPath =
                    Path.Combine(
                        audioDirectory,
                        fileName);

                if (!File.Exists(fullPath))
                {
                    return;
                }

                AudioFileReader reader =
                    new AudioFileReader(fullPath);

                readers.Add(reader);

                float safeRelativeVolume =
                    Math.Clamp(
                        relativeVolume,
                        0F,
                        1F);

                VolumeSampleProvider effectVolume =
                    new VolumeSampleProvider(
                        ConvertToMixerFormat(reader))
                    {
                        Volume =
                            EffectVolumeScale *
                            safeRelativeVolume
                    };

                effectVolumes.Add(
                    effectVolume,
                    safeRelativeVolume);

                mixer.AddMixerInput(effectVolume);
            }
        }

        /// <summary>
        /// Stops the mixer and releases all audio files
        /// when the main form closes.
        /// </summary>
        public void Dispose()
        {
            lock (audioLock)
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                outputDevice?.Stop();
                outputDevice?.Dispose();

                foreach (AudioFileReader reader in readers)
                {
                    reader.Dispose();
                }

                readers.Clear();
                effectVolumes.Clear();
                mixer = null;
                outputDevice = null;
                initializing = false;
            }
        }

        /// <summary>
        /// Converts mono or differently sampled inputs
        /// to the mixer's 48 kHz stereo format.
        /// </summary>
        private static ISampleProvider ConvertToMixerFormat(
            ISampleProvider source)
        {
            ISampleProvider converted = source;

            if (converted.WaveFormat.Channels == 1)
            {
                converted =
                    new MonoToStereoSampleProvider(
                        converted);
            }

            if (converted.WaveFormat.SampleRate !=
                OutputSampleRate)
            {
                converted =
                    new WdlResamplingSampleProvider(
                        converted,
                        OutputSampleRate);
            }

            return converted;
        }

        /// <summary>
        /// Restarts a seekable WAV source whenever it
        /// reaches its end so the ambience remains continuous.
        /// </summary>
        private sealed class LoopingSampleProvider :
            ISampleProvider
        {
            private readonly AudioFileReader source;

            public LoopingSampleProvider(
                AudioFileReader source)
            {
                this.source = source;
            }

            public WaveFormat WaveFormat =>
                source.WaveFormat;

            public int Read(
                float[] buffer,
                int offset,
                int count)
            {
                int totalRead = 0;
                int resetsWithoutData = 0;

                while (totalRead < count)
                {
                    int read =
                        source.Read(
                            buffer,
                            offset + totalRead,
                            count - totalRead);

                    if (read == 0)
                    {
                        resetsWithoutData++;

                        if (source.Length == 0 ||
                            resetsWithoutData > 2)
                        {
                            break;
                        }

                        source.Position = 0;
                        continue;
                    }

                    resetsWithoutData = 0;
                    totalRead += read;
                }

                return totalRead;
            }
        }
    }
}
