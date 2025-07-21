using System.Collections.Generic;
using UnityEngine;

// 직렬화 가능한 데이터 클래스
[System.Serializable]
public class LevelData
{
    public string levelName;
    public LevelMetaData metaData;
    public List<ChunkData> chunks = new List<ChunkData>();
}

[System.Serializable]
public class LevelMetaData
{
    public Vector3 minBounds;
    public Vector3 maxBounds;
    public float chunkSize;
}

[System.Serializable]
public class ChunkData
{
    public Vector3Int position;
    public List<LevelObjectData> objects = new List<LevelObjectData>();
}

[System.Serializable]
public class LevelObjectData
{
    public string id;
    public string objectName;
    public string prefabName;
    public string prefabPath;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public bool isActive;
    public Vector3Int chunkPosition;
}