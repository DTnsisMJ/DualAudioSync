using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using Firebase.Database;
using Firebase.Database.Query;
using System.Reactive.Linq;

namespace DualAudioSync.Services
{
    public class SyncService
    {
        private FirebaseClient firebase = new FirebaseClient("https://dualaudiosync-default-rtdb.firebaseio.com/");

        public class SalaData
        {
            public long playAt { get; set; }
            public string trackId { get; set; } = "";
            public string action { get; set; } = "";
        }

        public async Task<long> GetCorrectedTime()
        {
            try
            {
                var ntp = await Task.Run(() => {
                    var client = new UdpClient();
                    client.Connect("time.google.com", 123);
                    var data = new byte[48]; data[0] = 0x1B;
                    client.Send(data, data.Length);
                    var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                    var res = client.Receive(ref ep);
                    var intPart = (ulong)res[40] << 24 | (ulong)res[41] << 16 | (ulong)res[42] << 8 | (ulong)res[43];
                    return new DateTime(1900, 1, 1).AddMilliseconds((long)intPart * 1000).ToUniversalTime();
                });
                return (long)(ntp - new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
            catch { return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); }
        }

        public Task EnviarOrden(string codigo, long playAt, string trackId, string action)
        {
            return firebase.Child($"salas/{codigo}").PutAsync(new SalaData { playAt = playAt, trackId = trackId, action = action });
        }

        public IDisposable Escuchar(string codigo, Action<SalaData> onData)
        {
            return firebase.Child($"salas/{codigo}").AsObservable<SalaData>().Subscribe(d => { if (d.Object != null) onData(d.Object); });
        }
    }
}