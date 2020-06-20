using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Veggie : MonoBehaviour
{
    public Rigidbody2D body;
    public List<GameObject> objects;
    public bool isHidden = true;
    public GameObject activator;

    private int value;
    private int times;
    private bool mad;

    private void Start()
    {
        if(isHidden)
            objects.ForEach(g => g.SetActive(false));
    }

    public void Appear(int val)
    {
        value = Random.Range(1 + val / 2, 5 + val * 2);
        objects.ForEach(g => g.SetActive(true));
        activator.SetActive(false);
    }

    public string GetIntro()
    {
        times++;
        return "Nice, this (carrot) is worth (" + value + ").";
    }

    public int GetValue()
    {
        return value;
    }

    public bool IsEx()
    {
        return times > 0;
    }

    public bool CanPick()
    {
        mad = !mad && Random.value < Mathf.Pow(0.6f, times);
        return mad;
    }

    public string GetExInfo()
    {
         return "This (carrot) was worth (" + value + ").";
    }
}
