using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager soundManagerInstance;

    [SerializeField]
    [Header("Music Volume.")]
    [Range(0, 1)]
    float musicVolume = 0.2f;

    [SerializeField]
    [Header("Sounds Volume.")]
    [Range(0, 1)]
    float soundsVolume = 1.0f;

    [Header("Add audio clips here.")]
    [SerializeField]
    private AudioClip backgroundMusic;
    [SerializeField]
    private AudioClip pushButtonSound;
    [SerializeField]
    private AudioClip destroyObjectSound;
    [SerializeField]
    private AudioClip reshuffleSound;
    [SerializeField]
    private AudioClip completeSound;
    [SerializeField]
    private AudioClip failedSound;
    [SerializeField]
    private AudioClip selectObjectSound;

    AudioSource music;

    //10 sounds will be able to play at the same time excluding music.
    AudioSource[] sounds = new AudioSource[10];

    //Clear sounds after playing them.
    List<int> clearSounds = new List<int>();

    public void Awake()
    {
        //We only need one instance of the SoundManager alive at all times. Delete the object if it already exists when going back to main menu.
        if (soundManagerInstance == null)
        {
            soundManagerInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        //Will keep SoundManager alive between the scenes.
        DontDestroyOnLoad(soundManagerInstance);

        //Set audiosources on object
        music = GetComponent<AudioSource>();
        music = gameObject.AddComponent<AudioSource>();
        for(int i = 0; i < sounds.Length; i++)
        {
            sounds[i] = GetComponent<AudioSource>();
            sounds[i] = gameObject.AddComponent<AudioSource>();
        }

        music.clip = backgroundMusic;
        music.loop = true;
        music.volume = musicVolume;

        foreach(AudioSource sound in sounds)
        {
            sound.loop = false;
            sound.volume = soundsVolume;
        }
    }

    void Update()
    {
        if(!music.isPlaying)
        {
            music.Play();

        }

        //Clear sounds if any to clear.
        if(clearSounds.Count > 0)
        {
            if(!sounds[clearSounds[0]].isPlaying)
            {
                sounds[clearSounds[0]].clip = null;
                if(sounds[clearSounds[0]].pitch > 1)
                {
                    sounds[0].pitch = 1;
                }
                clearSounds.RemoveAt(0);
            }
        }
    }

    public void PlayBackgroundMusic()
    {
        if(!music.isPlaying)
        {
            music.Play();
        }

    }

    public void PlayPushButtonSound()
    {
        for(int i = 0; i < sounds.Length; i++)
        {
            if(sounds[i].clip == null)
            {
                sounds[i].clip = pushButtonSound;
                sounds[i].Play();
                clearSounds.Add(i);
                break;
            }
        }
    }

    public void PlayDestroyObjectSound()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].clip == null)
            {
                sounds[i].clip = destroyObjectSound;
                sounds[i].Play();
                clearSounds.Add(i);
                break;
            }
        }
    }

    public void PlayReshuffleSound()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].clip == null)
            {
                sounds[i].clip = reshuffleSound;
                sounds[i].Play();
                clearSounds.Add(i);
                break;
            }
        }
    }

    public void PlayCompleteSound()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].clip == null)
            {
                sounds[i].clip = completeSound;
                sounds[i].Play();
                clearSounds.Add(i);
                break;
            }
        }
    }

    public void PlayFailedSound()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].clip == null)
            {
                sounds[i].clip = failedSound;
                sounds[i].Play();
                clearSounds.Add(i);
                break;
            }
        }
    }

    public void PlaySelectObjectSound(int sizeOfSelect)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].clip == null)
            {
                sounds[i].clip = selectObjectSound;
                if (sizeOfSelect == 1)
                {
                    sounds[i].pitch = 1;
                }
                else if (sizeOfSelect == 2)
                {
                    sounds[i].pitch = 1.1f;
                }
                else if (sizeOfSelect == 3)
                {
                    sounds[i].pitch = 1.2f;
                }
                else if (sizeOfSelect == 4)
                {
                    sounds[i].pitch = 1.3f;
                }
                else if (sizeOfSelect == 5)
                {
                    sounds[i].pitch = 1.4f;
                }
                else if (sizeOfSelect == 6)
                {
                    sounds[i].pitch = 1.5f;
                }
                else if (sizeOfSelect == 7)
                {
                    sounds[i].pitch = 1.6f;
                }
                else if (sizeOfSelect >= 8)
                {
                    sounds[i].pitch = 1.7f;
                }
                sounds[i].Play();
                clearSounds.Add(i);
                break;
            }
        }
        
    }

}
