using System.Collections.Generic;
using UnityEngine;

public class TablesManager : Singleton<TablesManager>
{
    private List<Table> tables = new List<Table>();

    public List<Table> Tables { get => tables; }


    void Awake()
    {
        CreateSingleton(false);
        InitializeTablesThatAreActiveByDefault();
    }


    public void RegisterTableInListWhenActive(Table currentTable)
    {
        if (!tables.Contains(currentTable))
        {
            tables.Add(currentTable);
        }
    }

    public Table GetRandomAvailableTableForClient()
    {
        List<int> availableIndexes = new List<int>();

        for (int i = 0; i < tables.Count; i++)
        {
            if (!tables[i].IsOccupied)
            {
                availableIndexes.Add(i);
            }
        }

        if (availableIndexes.Count == 0) return null;

        int randomAvailableIndex = availableIndexes[Random.Range(0, availableIndexes.Count)];

        tables[randomAvailableIndex].IsOccupied = true;
        return tables[randomAvailableIndex];
    }

    // Hacer null a la fuerza a la table que iguale cuando llame al metodo
    public Table FreeTable(Table tableToFree)
    {
        if (tableToFree != null)
        {
            tableToFree.IsOccupied = false;
        }

        return null;
    }


    // Solamente agrega a la lista las mesas que estan activas por default, las que se activan con desbloqueo no se agregan en este metodo
    private void InitializeTablesThatAreActiveByDefault()
    {
        GameObject[] tableObjects = GameObject.FindGameObjectsWithTag("Table");

        foreach (GameObject obj in tableObjects)
        {
            Table table = obj.GetComponentInParent<Table>();
            if (table != null && table.gameObject.activeSelf) // Provisorio el activeself
            {
                tables.Add(table);
            }
        }
    }
}
