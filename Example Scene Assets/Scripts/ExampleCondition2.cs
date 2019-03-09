using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCondition2 : MonoBehaviour
{

    public int money = 100;
    public int stamina = 100;

    public bool Rich()
    {
        if (money > 10000000)
            return true;

        return false;
    }

    public bool Tired()
    {
        if (stamina < 30)
            return true;

        return false;
    }


}