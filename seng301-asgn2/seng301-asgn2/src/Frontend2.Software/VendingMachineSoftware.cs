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
        insertedAmount = 0;
        // handlers
        CoinSlot coinSlot = vendingHardware.CoinSlot;
        coinSlot.CoinRejected += new EventHandler<CoinEventArgs>(printCoinRejected);
        coinSlot.CoinAccepted += new EventHandler<CoinEventArgs>(printCoinAccepted);
        SelectionButton[] buttons = vendingHardware.SelectionButtons;
        foreach(SelectionButton button in buttons)
        {
            button.Pressed += new EventHandler(printButtonPressed);
        }
    }

    public void configure(List<string> popNames, List<int> popCosts)
    {
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
        SoftwareCoinRacks rack = new SoftwareCoinRacks(coinKinds[coinKindIndex], coins.Count);
        softwareRacks.Add(rack);
        // create array of length equal to coin rack size
        CoinRack[] coinRacks = vendingHardware.CoinRacks;
        int length = coinRacks.Length;
        int[] coinArray = new int[length];
        // set that index value to the number of coins in list
        try
        {
            coinArray[coinKindIndex] = coins.Count;
        } catch (Exception e)
        {
            Console.WriteLine("Invalid coin index - larger than number of slots: " + coinKindIndex);
        }
        // load coins into machine
        vendingHardware.LoadCoins(coinArray);
    }



    public void loadPops(int popKindIndex, List<PopCan> pops)
    {
        // create array of length the desired index
        PopCanRack[] popRacks = vendingHardware.PopCanRacks;
        int length = popRacks.Length;
        int[] popArray = new int[length];
        // set that index value to the number of pops in list
        try
        {
            popArray[popKindIndex] = pops.Count;
        } catch (Exception e)
        {
            Console.WriteLine("Invalid pop index - larger than number of slots: " + popKindIndex);
        }
        // load pops into machine
        vendingHardware.LoadPopCans(popArray);
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
        // now button is pressed, check if valid
        // need inserted money, pop name/type, change needed
        // enough money inserted, enough pop etc.

        // get price
        SoftwarePop pop = pops[button];
        int price = pop.getCost();
        string name = pop.getName();
        Console.WriteLine("Requested: " + name + ":" + price);

        // check if enough money inserted
        if (insertedAmount >= price)
        {
            // dispense pop
            PopCanRack[] racks = vendingHardware.PopCanRacks;
            racks[button].DispensePopCan();
            // take money into machine from receptacle
            CoinReceptacle recep = vendingHardware.CoinReceptacle;
            recep.StoreCoins();
            // calculate and dispense change
            int change = insertedAmount - price;

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

            // reset inserted amount
            insertedAmount = 0;
        } else
        {
            Console.WriteLine("Not enough money entered");
        }
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
        contents.CoinsInCoinRacks = new List<List<Coin>>();
        return contents;
    }

    /* Event Handlers */
    void printCoinRejected(object sender, CoinEventArgs e)
    {
        Console.WriteLine("Coin slot just rejected this coin: " + e.Coin);
    }

    void printCoinAccepted(object sender, CoinEventArgs e)
    {
        insertedAmount = insertedAmount + e.Coin.Value;
        Console.WriteLine("Coin slot just accepted this coin: " + e.Coin);
        Console.WriteLine("Inserted value: " + insertedAmount);
    }

    void printButtonPressed(object sender, EventArgs e)
    {
        Console.WriteLine("Button pressed");
    }
}