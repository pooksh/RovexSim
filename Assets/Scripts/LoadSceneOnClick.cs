using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour {

    public AudioClip clip;
    private AudioSource source;
    [SerializeField] private bool playAudio = false;
    [SerializeField] private bool enableDebugLogs = true;

    void Start()
    {
        if (playAudio) {
            source = GetComponent<AudioSource>();
            if (source == null) {
                Debug.LogError("Please add an audio source");
            }
        }
    }
    
    public void LoadByIndex(int sceneIndex)
    {
        PlayNoise();
        SceneManager.LoadScene(sceneIndex);
        if (enableDebugLogs) {
            Debug.Log($"Scene with index {sceneIndex} Loaded");
        }
    }

    public void LoadByName(string name) {
        PlayNoise();
        SceneManager.LoadScene(name);
        if (enableDebugLogs) {
            Debug.Log($"Scene with name {name} Loaded");
        }
    }

    private void PlayNoise() {
        if (playAudio) {
            source.PlayOneShot(clip);
        }
    }
}
