using UnityEngine;

public class ExampleCondition2 : MonoBehaviour
{
    public int money = 100;
    public int stamina = 100;

    public bool Rich()
    {
        return money > 10000000;
    }

    public bool Tired()
    {
        return stamina < 30;
    }
}