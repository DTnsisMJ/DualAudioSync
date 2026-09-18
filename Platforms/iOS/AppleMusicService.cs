#if IOS
using MediaPlayer;
namespace DualAudioSync.Services
{
    public class AppleMusicService
    {
        MPMusicPlayerController player = MPMusicPlayerController.SystemMusicPlayer;

        public string GetCurrentTrackId()
        {
            var item = player.NowPlayingItem;
            return item?.PlaybackStoreID ?? "";
        }

        public void PlayTrackId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var descriptor = new MPMusicPlayerStoreQueueDescriptor(new string[] { id });
            player.SetQueue(descriptor);
            player.Play();
        }

        public void Play() => player.Play();
        public void Pause() => player.Pause();
    }
}
#endif