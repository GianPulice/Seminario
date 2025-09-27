using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DropTable", menuName = "ScriptableObjects/Dungeon/New Monster Drop Table")]
public class DropTable : ScriptableObject
{
    public List<DropEntry> drops;

    public List<DropEntry> GetDropsForLayer(int currentLayer)
    {
        List<DropEntry> validDrops = new List<DropEntry>();

        foreach (var drop in drops)
        {
            if (currentLayer >= drop.minLayer && currentLayer <= drop.maxLayer)
            {
                validDrops.Add(drop);
            }
        }

        return validDrops;
    }
}
