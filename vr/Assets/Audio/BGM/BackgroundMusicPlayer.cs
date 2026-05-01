using UnityEngine;

public class SimpleIntroThenLoop : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float introLength = 10f;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;
    private bool introComplete = false;

    private void Awake()
    {
        if (FindObjectsOfType<SimpleIntroThenLoop>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = backgroundMusic;
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Play from the beginning (intro plays once)
        audioSource.time = 0;
        audioSource.Play();
    }

    private void Update()
    {
        if (!audioSource.isPlaying) return;

        // Check if intro is complete
        if (!introComplete && audioSource.time >= introLength)
        {
            introComplete = true;
            Debug.Log("Intro done, now in loop mode");
        }

        // Once intro is complete, loop the main section
        if (introComplete && audioSource.time >= backgroundMusic.length - 0.1f)
        {
            audioSource.time = introLength;
        }
    }
}