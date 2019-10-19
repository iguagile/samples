using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;

namespace IguagileChat
{
    public class AudioPlayer : IDisposable
    {
        private readonly BufferedWaveProvider provider;
        private readonly WaveOutEvent waveOut = new WaveOutEvent();

        public AudioPlayer(WaveFormat format)
        {
            provider = new BufferedWaveProvider(format);
        }

        public void AddSamples(byte[] buffer)
        {
            provider.AddSamples(buffer, 0, buffer.Length);
        }

        public void Play()
        {
            waveOut.Init(provider);
            waveOut.Play();
        }

        public void Dispose()
        {
            waveOut.Dispose();
        }
    }
}
