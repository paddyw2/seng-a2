using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontend2;
using Frontend2.Hardware;

public class StoredContents : VendingMachineStoredContents
{
    public StoredContents()
    {

    }

    public void setCoinList(List<List<Coin>> coinList)
    {
        this.CoinsInCoinRacks = coinList;
    }

    public void setStorageList(List<Coin> coinList)
    {
        this.PaymentCoinsInStorageBin = coinList;
    }

    public void setPopList(List<List<PopCan>> popList)
    {
        this.PopCansInPopCanRacks = popList;
    }
}