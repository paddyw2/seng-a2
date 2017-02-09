using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontend2;
using Frontend2.Hardware;

public class SoftwareCoinRacks
{
    private int value;
    private int quantity;

    public SoftwareCoinRacks()
    {
        value = 0;
        quantity = 0;
    }

    public SoftwareCoinRacks(int value, int quantity)
    {
        this.value = value;
        this.quantity = quantity;
    }

    public int getValue()
    {
        return value;
    }

    public int getQuantity()
    {
        return quantity;
    }

    public void decQuantity()
    {
        quantity--;
    }
}