using UnityEngine;

[CreateAssetMenu(fileName = "GlobalColorManager", menuName = "ScriptableObjects/GlobalColorManager")]
public class GlobalColorManager : ScriptableObject
{
    public Color selectedColor = Color.white; // Default global color
}