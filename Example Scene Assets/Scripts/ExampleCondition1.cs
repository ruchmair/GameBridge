using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCondition1 : MonoBehaviour
{

    public float health = 100;
    public bool glove;

    public bool Healthy()
    {
        if (health > 70)
            return true;

        return false;
    }

    public bool HasLegendaryGloves()
    {
        if (glove)
            return true;

        return false;
    }

    public bool Del()
    {
        Debug.Log("deld");

        return true;
    }
}