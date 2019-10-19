using Iguagile;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace IguagileChat
{
    class Program
    {
        private static readonly int port = 4000;

        private static readonly IguagileClient client = new IguagileClient();
        private static readonly WaveFormat waveFormat = new WaveFormat(44100/2, 1);

        static void Main(string[] args)
        {
            Console.Write("Enter server address: ");
            var address = Console.ReadLine();

            string name;
            do
            {
                Console.Write("Your name: ");
                name = Console.ReadLine().Trim();
            } while (name == "");

            var receiver = new RpcMessageReceiver();
            client.AddRpc(nameof(receiver.WriteMessage), receiver);
            client.OnConnected += () => client.Rpc(nameof(receiver.WriteMessage), RpcTargets.AllClients, $"* {name} joined.");
            client.OnError += Console.WriteLine;

            var players = new Dictionary<int, AudioPlayer>();
            client.OnBinaryReceived += (id, data) =>
            {
                if (!players.ContainsKey(id))
                {
                    var player = new AudioPlayer(waveFormat);
                    player.AddSamples(data);
                    player.Play();
                    players[id] = player;
                }
                else
                {
                    players[id].AddSamples(data);
                }
            };

            client.StartAsync(address, port, Protocol.Tcp);

            var waveIn = new WaveInEvent
            {
                WaveFormat = waveFormat,
                BufferMilliseconds = 100
            };
            waveIn.DataAvailable += (s, e) =>
            {
                var sum = 0.0;
                for (int i = 0; i < e.BytesRecorded; i += waveIn.WaveFormat.BlockAlign)
                {
                    short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i + 0]);
                    float normalized = sample / 32768f;
                    var square = normalized * normalized;
                    sum += square;
                }

                var average = sum / e.BytesRecorded;
                const double threshold = 25e-6;
                if (average > threshold)
                {
                    client.SendBinaryAsync(e.Buffer, RpcTargets.OtherClients);
                }
            };
            waveIn.StartRecording();

            while (true)
            {
                var message = Console.ReadLine().Trim();

                if (!client.IsConnected)
                {
                    break;
                }

                client.Rpc(nameof(receiver.WriteMessage), RpcTargets.AllClients, $"{name}: {message}");
            }

            foreach (var (_, player) in players)
            {
                player.Dispose();
            }

            receiver.WriteMessage("Connection closed. Press enter to exit.");
            Console.ReadLine();
        }
    }
}
