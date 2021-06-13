using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehaviour : MonoBehaviour
{
    public PositionInt pos;
    public GameObject model;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, Server.Instance.worldCtrl.WorldLength / 2f + 1f);
        model.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ChangeTerrain(string terrainSpriteName)
    {
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/World/Blocks/TerrainSprite/" + terrainSpriteName);
    }

    public void ChangeModel(string modelSpriteName)
    {
        ShowModel();
        model.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/World/Blocks/ModelSprite/" + modelSpriteName);
    }

    public void ShowModel()
    {
        model.SetActive(true);
    }

    public void HideModel() 
    {
        model.SetActive(false);
        model.transform.GetComponent<SpriteRenderer>().sprite = null;
    }

}
