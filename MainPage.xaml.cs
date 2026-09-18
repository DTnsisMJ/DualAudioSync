using DualAudioSync.Services;

namespace DualAudioSync
{
    public partial class MainPage : ContentPage
    {
        SyncService sync = new SyncService();
        string codigoSala = "";
        IDisposable? listener = null;
        string lastTrackId = "";

#if IOS
        AppleMusicService musicService = new AppleMusicService();
#endif

        public MainPage() { InitializeComponent(); }

        void OnCrearClicked(object sender, EventArgs e)
        {
            codigoSala = new Random().Next(1000, 9999).ToString();
            Conectar(codigoSala);
        }

        void OnUnirseClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCodigo.Text)) return;
            codigoSala = TxtCodigo.Text.Trim();
            Conectar(codigoSala);
        }

        void Conectar(string codigo)
        {
            LblSala.Text = $"SALA: {codigo}";
            TxtCodigo.Text = codigo;
            BtnSync.IsEnabled = true;
            BtnPausa.IsEnabled = true;
            LblEstado.Text = "Esperando...";

            listener?.Dispose();
            listener = sync.Escuchar(codigo, async (data) =>
            {
                long ahora = await sync.GetCorrectedTime();
                long esperar = data.playAt - ahora;
                if (esperar > 0) await Task.Delay((int)esperar);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LblCancion.Text = $"ID: {data.trackId}";
                    LblEstado.Text = data.action == "play" ? "▶️ SONANDO" : "⏸️ PAUSA";
                    lastTrackId = data.trackId;

#if IOS
                    if(data.action == "play")
                    {
                        musicService.PlayTrackId(data.trackId);
                    }
                    else
                    {
                        musicService.Pause();
                    }
#endif
                });
            });
        }

        async void OnSyncClicked(object sender, EventArgs e)
        {
#if IOS
            string currentId = musicService.GetCurrentTrackId();
            if(string.IsNullOrEmpty(currentId))
            {
                await DisplayAlert("Error", "Pon una canción en Apple Music primero y dale Play", "OK");
                return;
            }
            long playAt = await sync.GetCorrectedTime() + 1200;
            lastTrackId = currentId;
            await sync.EnviarOrden(codigoSala, playAt, currentId, "play");
#else
            await DisplayAlert("Info", "Esto solo funciona en iPhone real", "OK");
#endif
        }

        async void OnPausaClicked(object sender, EventArgs e)
        {
            long playAt = await sync.GetCorrectedTime() + 500;
            await sync.EnviarOrden(codigoSala, playAt, lastTrackId, "pause");
        }
    }
}