using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class BuildingBlueprint {
    readonly string prefabPath;
    readonly Vector3Int location;
    readonly List<Tuple<Type, int>> requiredItems;

    public BuildingBlueprint(string prefabPath, Vector3Int location, List<Tuple<Type, int>> requiredItems)
    {
        this.prefabPath = prefabPath;
        this.location = location;
        this.requiredItems = requiredItems;
    }

    public string PrefabPath => prefabPath;

    public Vector3Int Location => location;

    public List<Tuple<Type, int>> RequiredItems => requiredItems;
}