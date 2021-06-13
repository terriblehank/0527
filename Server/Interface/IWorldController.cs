using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldController
{
    void InitPropertyItemMappingList();

    void InitItemBlockMappingList();

    void RegisterItems();

    void RegisterBlocks();

    void SpawnWorld();

    void AddItemType(string type);

    void AddBlockType(string type);

    BlockData GetBlockDataInDB(PositionInt pos);

    List<BlockData> GetAllBlockDatas();

}
