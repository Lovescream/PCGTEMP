using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance { get; set; }

    public GameObject fxAudioSourcePrefab;
    public Pool<AudioSource> fxAudioSourcePool = new Pool<AudioSource>();
    private List<AudioClip> fxClipInOneFrame = new List<AudioClip>();

    public static float FXVolume {
        get => PlayerPrefs.GetFloat("Sound_Volume", 0.8f);
        set => PlayerPrefs.SetFloat("Sound_Volume", value);
    }


    private void Awake() {
        if (!Instance) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        fxAudioSourcePool.CreatePoolFromGameObject(fxAudioSourcePrefab, 30, true);
        //backgroundMute = PlayerPrefs.GetInt("BackgroundMute", 1) == 1;
    }




    public static AudioSource PlayFX(AudioClip clip, float volume = 1f) {

        // #1. 조건 검사.
        if (!clip) return null;
        if (FXVolume == 0) return null;
        if (volume == 0) return null;
        if (Instance.fxClipInOneFrame.Contains(clip)) return null; // 이미 해당 클립을 재생중이라면 skip.

        // #2. nowPlayingClip 에 추가.
        Instance.fxClipInOneFrame.Add(clip);

        // #3. AudioSource 풀에서 AudioSource 받아오기. (없으면 skip됨)
        AudioSource next = Instance.fxAudioSourcePool.GetNext();
        if (!next) return null;

        // #4. 소리 재생.
        next.volume = FXVolume;
        next.clip = clip;
        next.Play();
        return next;
    }

}