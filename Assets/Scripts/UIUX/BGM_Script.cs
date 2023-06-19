using UnityEngine;

public class BGM_Script : MonoBehaviour
{
    public AudioClip[] ClipBGM;
    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Call_BGM(1);
    }

    public void Call_BGM(int BGMnumber) {
        audioSource.Stop();
        audioSource.clip = ClipBGM[BGMnumber];
        audioSource.Play();
    }
}
