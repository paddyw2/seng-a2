using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontend2;
using Frontend2.Hardware;

public class VendingMachineSoftware
{
    private VendingMachine vendingHardware;
    private int buttonCount;
    private int coinRackCapacity;
    private int popRackCapacity;
    private int receptacleCapacity;
    private int insertedAmount;
    private List<SoftwarePop> pops;
    private List<SoftwareCoinRacks> softwareRacks;
    private List<int> coinKinds;

    public VendingMachineSoftware()
    {
        throw new Exception("No arguments given");
    }

    public VendingMachineSoftware(List<int> coinKinds, int selectionButtonCount, int coinRackCapacity, int popRackCapacity, int receptacleCapacity)
    {
        // process information
        buttonCount = selectionButtonCount;
        this.coinRackCapacity = coinRackCapacity;
        this.popRackCapacity = popRackCapacity;
        this.receptacleCapacity = receptacleCapacity;
        this.coinKinds = coinKinds;
        // create machine hardware
        vendingHardware = new VendingMachine(coinKinds.ToArray(), selectionButtonCount, coinRackCapacity, popRackCapacity, receptacleCapacity);
        // default information
        pops = new List<SoftwarePop>();
        softwareRacks = new List<SoftwareCoinRacks>();
        // create software version of coin racks
        for(int i=0;i<coinKinds.Count; i++)
        {
            SoftwareCoinRacks newCoinRack = new SoftwareCoinRacks(coinKinds[i], 0);
            softwareRacks.Add(newCoinRack);
        }
        insertedAmount = 0;
        // set up handlers
        CoinSlot coinSlot = vendingHardware.CoinSlot;
        coinSlot.CoinRejected += new EventHandler<CoinEventArgs>(printCoinRejected);
        coinSlot.CoinAccepted += new EventHandler<CoinEventArgs>(printCoinAccepted);
        SelectionButton[] buttons = vendingHardware.SelectionButtons;
        // add handler to each button
        foreach(SelectionButton button in buttons)
        {
            button.Pressed += new EventHandler(printButtonPressed);
        }
        CoinRack[] coinRacks = vendingHardware.CoinRacks;
        // add handler to each coin rack
        foreach(CoinRack coinRack in coinRacks)
        {
            coinRack.CoinAdded += new EventHandler<CoinEventArgs>(printCoinLoaded);
        }
        PopCanRack[] popRacks = vendingHardware.PopCanRacks;
        // add handler to each pop rack
        foreach (PopCanRack rack in popRacks)
        {
            rack.PopCanRemoved += new EventHandler<PopCanEventArgs>(printPopDispensed);
        }
    }

    public void configure(List<string> popNames, List<int> popCosts)
    {
        // reset software list in case of reconfiguring
        pops.Clear();
        int counter = 0;
        foreach (string name in popNames)
        {
            SoftwarePop pop = new SoftwarePop(name, popCosts[counter]);
            pops.Add(pop);
            counter++;
        }
        vendingHardware.Configure(popNames, popCosts);
    }

    public void loadCoins(int coinKindIndex, List<Coin> coins)
    {
        // load in software
        SoftwareCoinRacks getRack = softwareRacks[coinKindIndex];
        getRack.incQuantity(coins.Count);
        // load in hardware
        CoinRack[] racks = vendingHardware.CoinRacks;
        CoinRack coinRack = null;
        try
        {
            coinRack = racks[coinKindIndex];
        } catch (Exception e)
        {
            Console.WriteLine("Invalid coin kind index");
        }
        coinRack.LoadCoins(coins);
        //Console.WriteLine("Load: " + coinKinds[coinKindIndex]);
    }

    public void loadPops(int popKindIndex, List<PopCan> pops)
    {
        // load in hardware
        PopCanRack[] racks = vendingHardware.PopCanRacks;
        PopCanRack popRack = null;
        try
        {
            popRack = racks[popKindIndex];
        } catch (Exception e)
        {
            Console.WriteLine("Invalid pop kind index");
        }
        popRack.LoadPops(pops);
    }

    public void insertCoin(Coin coin)
    {
        CoinSlot coinSlot = vendingHardware.CoinSlot;
        coinSlot.AddCoin(coin);
    }

    public void pressButton(int button)
    {
        SelectionButton[] buttons = vendingHardware.SelectionButtons;
        SelectionButton chosenButton = null;
        try
        {
            chosenButton = buttons[button];
        } catch (Exception e)
        {
            Console.WriteLine("Invalid button chosen");
        }
        // press button
        chosenButton.Press();

        // now button is pressed, process action

        // get price and name of chosen pop
        SoftwarePop pop = pops[button];
        int price = pop.getCost();
        string name = pop.getName();
        Console.WriteLine("Requested: " + name + ":" + price);
        PopCanRack[] racks = vendingHardware.PopCanRacks;

        // check if enough money inserted
        if (insertedAmount >= price && racks[button].Count > 0)
        {
            // dispense pop
            racks[button].DispensePopCan();
            // take money into machine from receptacle
            CoinReceptacle recep = vendingHardware.CoinReceptacle;
            recep.StoreCoins();
            // calculate and dispense change
            int change = insertedAmount - price;
            int remainingCredit = dispenseChange(change);
            // reset inserted amount taking into
            // account any credit
            insertedAmount = remainingCredit;
            Console.WriteLine("Remaining credit: " + remainingCredit);
        } else
        {
            if(racks[button].Count > 0)
                Console.WriteLine("Not enough money entered");
            else
                Console.WriteLine("Not enough pop");
        }
   }

    public int dispenseChange(int change)
    {
        /* Change Algorithm */
        // change algorithm from A1 (slightly modified)
        int val = change;
        int upperBound = val + 1;

        int largestCoinVal = 0;
        // loop change process until either
        // no more valid coins, or change
        // value is met (where the loop will
        // be broken)
        while (true)
        {
            // loop through coin slots to find
            // next denomination
            // if found, sets the largestSlot
            // to a Coin, which has a value
            SoftwareCoinRacks largestSlot = null;
            int slotIndex = 0;
            int counter = 0;
            foreach (SoftwareCoinRacks slot in softwareRacks)
            {
                int rackValue = slot.getValue();
                if( rackValue >= largestCoinVal && rackValue < upperBound)
                {
                    largestCoinVal = rackValue;
                    largestSlot = slot;
                    slotIndex = counter;
                }
                counter++;
            }

            // check if largest coin is null
            // if so, then we have not found
            // a suitable change coin (we will
            // short change them)
            if (largestSlot == null)
                break;

            // now we have next largest coin
            // decrease until target met, each
            // time removing a coin from the
            // coin slot, and adding it to the
            // coin change list
            bool runLoop = true;
            bool changeFinished = false;
            while (runLoop && largestSlot.getQuantity() > 0)
            {
                // decrement change total by coin
                // denomination
                val = val - largestCoinVal;
                if (val >= 0)
                {
                    // returned coin may be "incorrect" if the
                    // slot was loaded incorrectly - this is
                    // the desired functionality
                    CoinRack[] coinRacks = vendingHardware.CoinRacks;
                    // release coin in hardware and in software
                    coinRacks[slotIndex].ReleaseCoin();
                    largestSlot.decQuantity();
                    Console.WriteLine("Dispensing coin: " + largestCoinVal);

                    // if new change value is zero, all change
                    // has been added to the coin change list
                    // so terminate loop
                    if (val == 0)
                    {
                        runLoop = false;
                        changeFinished = true;
                    }
                }
                else
                {
                    // if change value is negative, reverse
                    // last decrement and move to lower
                    // denomination
                    val = val + largestCoinVal;
                    runLoop = false;
                }
            }

            // if change finished flag is set, then
            // exit main loop
            if (changeFinished)
                break;

            // else, reset variables and start next
            // loop to find lower denomination
            upperBound = largestCoinVal;
            largestCoinVal = 0;
        }
        return val;
    }

    public List<IDeliverable> extractChute()
    {
        DeliveryChute chute = vendingHardware.DeliveryChute;
        IDeliverable[] array = chute.RemoveItems();
        return array.ToList();
    }

    public VendingMachineStoredContents unload()
    {
        VendingMachineStoredContents contents = new VendingMachineStoredContents();
        // coin rack coins
        CoinRack[] racks = vendingHardware.CoinRacks;
        foreach(CoinRack rack in racks)
        {
            List<Coin> emptiedCoins = rack.Unload();
            contents.CoinsInCoinRacks.Add(emptiedCoins);
        }
        // storage coins
        CoinReceptacle recep = vendingHardware.StorageBin;
        List<Coin> recepCoins = recep.Unload();
        foreach(Coin coin in recepCoins)
        {
            contents.PaymentCoinsInStorageBin.Add(coin);
        }
        // pops
        PopCanRack[] popRacks = vendingHardware.PopCanRacks;
        foreach(PopCanRack rack in popRacks)
        {
            List<PopCan> emptiedPops = rack.Unload();
            contents.PopCansInPopCanRacks.Add(emptiedPops);
        }
        return contents;
    }

    /* Event Handlers */
    void printCoinRejected(object sender, CoinEventArgs e)
    {
        Console.WriteLine("Coin slot just rejected this coin: " + e.Coin);
    }

    void printCoinAccepted(object sender, CoinEventArgs e)
    {
        // increment credit amount
        insertedAmount = insertedAmount + e.Coin.Value;
        //Console.WriteLine("Coin slot just accepted this coin: " + e.Coin);
        Console.WriteLine("Inserted value: " + insertedAmount);
    }

    void printButtonPressed(object sender, EventArgs e)
    {
        //Console.WriteLine("Button pressed");
    }

    void printCoinLoaded(object sender, CoinEventArgs e)
    {
        // this code updates the software understanding of
        // the coin racks when the receptacle stores its
        // coins by listening to event, as it may go into
        // storage bin if coin rack full
        int coinValue = e.Coin.Value;
        Console.WriteLine("Coin rack loaded with: " + coinValue);
        CoinRack rack = (CoinRack) sender;
        // implement code here to update software coin rack
        // presume that receptacle always puts coins in right
        // racks, so the coin value indicates which rack
        foreach(SoftwareCoinRacks coinRack in softwareRacks)
        {
            if(coinRack.getValue() == coinValue)
            {
                coinRack.incQuantity(1);
                break;
            }
        }
    }

    void printPopDispensed(object sender, PopCanEventArgs e)
    {
        string popName = e.PopCan.Name;
        Console.WriteLine("Pop dispensed: " + popName);
    }
}