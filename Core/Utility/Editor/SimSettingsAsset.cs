using UnityEngine;
using UnityEditor;
using System;
using ParticlePhysics2D;

public class SimSettingsAsset
{
    [MenuItem("Assets/Create/SimSettings")]
    public static void CreateAsset ()
    {
		CustomAssetUtility.CreateAsset<SimSettings>();
    }
}