using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SoundData", menuName = "JangLib/SoundData")]
public class SoundDataSO : ScriptableObject
{
    public AudioClip clip;
    public AudioMixerGroup output;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 1.5f)] public float pitch = 1f;
    public bool loop = false;
    
    public bool useRandomPitch = false;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
}