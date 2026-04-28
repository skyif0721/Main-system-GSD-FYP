using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class damage : MonoBehaviour
{
    public bool hitEnemy;
    public bool hitPlayer;

    public int damageAmount = 0;

    private void takeDamage()
    {
        if (hitPlayer)
        {
            
        }

        if (hitEnemy)
        {

        }
    }
}
