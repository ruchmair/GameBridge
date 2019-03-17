using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCondition1 : MonoBehaviour
{

    public float health = 100;
    public bool glove;

    public bool Healthy()
    {
        return health > 70;
    }

    public bool HasLegendaryGloves()
    {
        return glove;
    }


}