using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [SerializeField] AudioSource sfx;
    [SerializeField] AudioSource ambient;
    [SerializeField] AudioClip cardHover;
    [SerializeField] AudioClip cardSelect;
    [SerializeField] AudioClip cardConfirm;
    [SerializeField] AudioClip step;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip land;
    [SerializeField] AudioClip dash;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip pillar;
    [SerializeField] AudioClip pillarHurt;
    [SerializeField] AudioClip page;
    [SerializeField] AudioClip wind;
    [SerializeField] AudioClip sliceA;
    [SerializeField] AudioClip sliceB;

    AudioSource walkSource;
    int sliceFlip;

    void Awake()
    {
        Instance = this;
        EnsureClips();
        EnsureSources();

        if (ambient != null && wind != null)
        {
            Configure(ambient);
            ambient.clip = wind;
            ambient.loop = true;
            ambient.volume = 0.28f;
            ambient.Play();
        }
    }

    public void Bind(AudioSource sfxSource, AudioSource ambientSource)
    {
        sfx = sfxSource;
        ambient = ambientSource;
    }

    public void PlayHover() => Play(cardHover, 0.5f);
    public void PlaySelect() => Play(cardSelect, 0.7f);
    public void PlayConfirm() => Play(cardConfirm, 0.85f);
    public void PlayJump() => Play(jump, 0.7f);
    public void PlayLand() => Play(land, 0.55f);
    public void PlayDash() => Play(dash, 0.7f);
    public void PlayDeath() => Play(death, 0.9f);
    public void PlayPillar() => Play(pillar, 0.8f);
    public void PlayPillarHurt()
    {
        if (pillarHurt == null) EnsureClips();
        Play(pillarHurt, 1f);
    }
    public void PlayPage() => Play(page, 0.7f);

    public void PlaySlice()
    {
        sliceFlip++;
        var clip = sliceFlip % 2 == 1 ? sliceA : sliceB;
        if (clip == null) clip = sliceA != null ? sliceA : sliceB;
        Play(clip, 1f);
    }

    public void SetWalking(bool walking)
    {
        if (walkSource == null) return;
        if (walking)
        {
            if (walkSource.clip != step)
            {
                walkSource.clip = step;
                walkSource.loop = true;
            }
            if (step != null && !walkSource.isPlaying)
                walkSource.Play();
        }
        else if (walkSource.isPlaying)
        {
            walkSource.Stop();
        }
    }

    void Play(AudioClip clip, float volume)
    {
        if (clip == null) return;
        if (sfx == null) EnsureSources();
        if (sfx == null) return;
        sfx.PlayOneShot(clip, volume);
    }

    void EnsureSources()
    {
        if (sfx == null) sfx = GetComponent<AudioSource>();
        if (sfx == null) sfx = gameObject.AddComponent<AudioSource>();
        Configure(sfx);

        if (walkSource == null)
        {
            walkSource = gameObject.AddComponent<AudioSource>();
            walkSource.playOnAwake = false;
            walkSource.loop = true;
            walkSource.volume = 0.4f;
            Configure(walkSource);
        }
    }

    static void Configure(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.mute = false;
        source.ignoreListenerPause = true;
        source.dopplerLevel = 0f;
    }

    void EnsureClips()
    {
        cardHover = Fallback(cardHover, "Assets/Audio/card_hover.wav");
        cardSelect = Fallback(cardSelect, "Assets/Audio/card_select.wav");
        cardConfirm = Fallback(cardConfirm, "Assets/Audio/card_confirm.wav");
        step = Fallback(step, "Assets/Audio/Walk.wav");
        jump = Fallback(jump, "Assets/Audio/Jump.wav");
        land = Fallback(land, "Assets/Audio/Land.wav");
        dash = Fallback(dash, "Assets/Audio/BodyRoll.wav");
        death = Fallback(death, "Assets/Audio/death.wav");
        pillar = Fallback(pillar, "Assets/Audio/pillar.wav");
        pillarHurt = Fallback(pillarHurt, "Assets/Audio/PillarHurt.mp3");
        page = Fallback(page, "Assets/Audio/page.wav");
        wind = Fallback(wind, "Assets/Audio/Ambience.mp3");
        sliceA = Fallback(sliceA, "Assets/Audio/SwordSlice1.wav");
        sliceB = Fallback(sliceB, "Assets/Audio/SwordSlice2.wav");
    }

    static AudioClip Fallback(AudioClip current, string path)
    {
        if (current != null) return current;
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
#else
        return current;
#endif
    }
}
