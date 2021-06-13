using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public string id;
    public float pTest;
    public float targetX;
    public float targetY;
    private PlayerData data;

    public Animator animator;

    void Start()
    {
        data = Server.Instance.playerCtrl.GetPlayer(id);
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (!Client.Instance.ContainsPlayer(id))
        {
            Destroy(gameObject);
            return;
        }

        pTest = data.properties.Test;
        Position target = data.target;

        if (target != null)
        {
            targetX = data.target.x;
            targetY = data.target.y;
            animator.SetBool("hasTarget", true);
            Vector3 rotation = transform.rotation.eulerAngles;
            if (targetX > transform.position.x)
            {
                transform.rotation = Quaternion.Euler(rotation.x, 0, rotation.z);
            }
            else
            {
                transform.rotation = Quaternion.Euler(rotation.x, 180, rotation.z);
            }
        }
        else
        {
            animator.SetBool("hasTarget", false);
        }
        transform.position = new Vector3(data.pos.x, data.pos.y, data.pos.y);
    }
}
