using UnityEngine;
public enum RoomSize { Small, Medium, Large }
[CreateAssetMenu(fileName = "NewRoomConfig", menuName = "Dungeon/Room Config")]
public class RoomConfig : ScriptableObject
{
    [Header("Configuración Básica")]
    public RoomSize size;
    public string roomID;

    [Header("Dificultad por Capas")]
    public int baseEnemyCount = 3;
    public AnimationCurve enemyCountByLayer; // Para escalar por capa
    public AnimationCurve enemyStrengthByLayer;

    [Header("Spawn Points")]
    public int enemySpawnPointCount;
    public int lootSpawnPointCount;
    public int trapSpawnPointCount;

    [Header("Configuración Visual")]
    public Material[] possibleFloorMaterials;
    public Material[] possibleWallMaterials;
    public Light[] possibleLightingSetups;

    [Header("Opciones de Gameplay")]
    public bool allowTraps = true;
    public bool allowLoot = true;

    public int GetEnemyCountForLayer(int layer)
    {
        float multiplier = enemyCountByLayer.Evaluate(Mathf.Clamp(layer, 1, 7));
        return Mathf.RoundToInt(baseEnemyCount * multiplier);
    }
}
