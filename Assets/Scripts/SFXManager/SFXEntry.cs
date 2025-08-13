using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFXEntry
{
    public string key;
    public AudioClip clip;
    [Range(0f, 1f)] public float clipVolume = 1f;
}
