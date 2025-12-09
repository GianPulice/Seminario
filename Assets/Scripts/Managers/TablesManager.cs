using System.Collections.Generic;
using UnityEngine;

public class TablesManager : Singleton<TablesManager>
{
    private List<Table> tables = new List<Table>();

    public List<Table> Tables { get => tables; }


    void Awake()
    {
        CreateSingleton(false);
        SuscribeToSaveSystemManagerEvents();
        InitializeTablesThatAreActiveByDefault();
    }

    void OnDestroy()
    {
        UnsuscribeToSaveSystemManagerEvents();
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


    private void SuscribeToSaveSystemManagerEvents()
    {
        SaveSystemManager.OnSaveAllGameData += OnSaveTables;
        SaveSystemManager.OnLoadAllGameData += OnLoadTables;
    }

    private void UnsuscribeToSaveSystemManagerEvents()
    {
        SaveSystemManager.OnSaveAllGameData -= OnSaveTables;
        SaveSystemManager.OnLoadAllGameData -= OnLoadTables;
    }

    private void OnSaveTables()
    {
        SaveData data = SaveSystemManager.LoadGame();

        data.tablesData.Clear();

        foreach (var table in tables)
        {
            if (table.IsDirty) // solo guardamos si está sucia
            {
                data.tablesData.Add(new TableSaveData
                {
                    tableNumber = table.TableNumber,
                    isDirty = table.IsDirty
                });
            }
        }

        SaveSystemManager.SaveGame(data);
    }

    private void OnLoadTables()
    {
        SaveData data = SaveSystemManager.LoadGame();

        if (data.tablesData == null || data.tablesData.Count == 0)
            return;

        foreach (var tableData in data.tablesData)
        {
            Table table = tables.Find(t => t.TableNumber == tableData.tableNumber);
            if (table != null)
            {
                table.SetDirty(tableData.isDirty);
            }
        }
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
