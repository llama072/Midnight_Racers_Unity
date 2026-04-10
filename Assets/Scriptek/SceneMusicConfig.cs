using System.Collections.Generic;
using UnityEngine;

public class SceneMusicConfig : MonoBehaviour
{
    [SerializeField] private List<MusicManager.TrackData> playlist;
    [SerializeField] private bool useLowPassFilter = false;
    [SerializeField] private bool playIntroFirst = false;

    void Start()
    {
        if (MusicManager.Instance == null) return;

        if (playIntroFirst)
        {
            // Először átadjuk a playlistet, MAJD indítjuk az intrót
            if (playlist != null && playlist.Count > 0)
                MusicManager.Instance.SetPlaylistWithoutPlaying(playlist, useLowPassFilter);

            MusicManager.Instance.PlayIntro();
        }
        else if (playlist != null && playlist.Count > 0)
        {
            MusicManager.Instance.SetPlaylist(playlist, useLowPassFilter);
        }
    }
}