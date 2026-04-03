using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(AudioSource))]
public class RenderMovie : MonoBehaviour
{
    public MovieTexture movTexture;

    public AudioSource audioSource;

    public bool PlayWhenStart = true;

    void Start()
    {
        if (PlayWhenStart == true)
        {
            PlayMovie();
        }
    }

    public void PlayMovie()
    {
        // Get the components
        var renderer = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Check if the movie is assigned
        if (movTexture == null)
        {
            Debug.LogError("RenderMovie: No MovieTexture assigned!");
            return;
        }

        // Assign the movie texture to the material
        renderer.material.mainTexture = movTexture;

        // Assign the movie's audio clip, if available
        if (movTexture.audioClip != null)
        {
           // audioSource.clip = movTexture.audioClip;
        }
        else
        {
            Debug.LogWarning("RenderMovie: MovieTexture has no audio track.");
        }

        // Start playback
        movTexture.Play();

        if (audioSource.clip != null)
            audioSource.Play();
    }
    public void StopMovie()
    {
        movTexture.Stop();
        audioSource.Stop();

    }

    void Update()
    {
        // Optionally, restart when finished
        if (!movTexture.isPlaying && movTexture.isReadyToPlay)
        {
            movTexture.Play();
            if (audioSource.clip != null)
                audioSource.Play();
        }
    }
}
