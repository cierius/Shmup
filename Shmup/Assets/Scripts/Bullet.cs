using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage = 10;

    private void Awake()
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>());
        transform.SetParent(null);
        transform.localScale = new Vector3(.25f, .25f, 1f);
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Unwalkable")
        {
            Destroy(this.gameObject);
        }
        else if(coll.gameObject.tag == "Enemy")
        {
            coll.gameObject.GetComponent<EnemyAI>().health -= bulletDamage;
            print(coll.gameObject.GetComponent<EnemyAI>().health);
            Destroy(this.gameObject);
        }
    }
    }
