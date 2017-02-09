using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontend2;
using Frontend2.Hardware;

public class SoftwarePop
{
    private int cost;
    private string name;

    public SoftwarePop()
    {
        cost = 0;
        name = "untitled";
    }

    public SoftwarePop(string name, int cost)
    {
        this.name = name;
        this.cost = cost;
    }

    public int getCost()
    {
        return cost;
    }

    public string getName()
    {
        return name;
    }
}