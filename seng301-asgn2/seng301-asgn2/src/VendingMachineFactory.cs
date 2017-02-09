using System;
using System.Collections.Generic;
using Frontend2;
using Frontend2.Hardware;

public class VendingMachineFactory : IVendingMachineFactory {
    private List<VendingMachine> createdMachines = new List<VendingMachine>();

    public int CreateVendingMachine(List<int> coinKinds, int selectionButtonCount, int coinRackCapacity, int popRackCapcity, int receptacleCapacity) {
        Console.WriteLine("VM: Creating vending machine...");
        VendingMachine newMachine = new VendingMachine(coinKinds.ToArray(), selectionButtonCount, coinRackCapacity, popRackCapcity, receptacleCapacity);
        // machine with id 0, is at index 0 in list
        // so before first machine, list size is 0, which gives our id
        int machineId = createdMachines.Count;
        // now add machine to id index
        createdMachines.Add(newMachine);
        // return id
        return machineId;
    }

    public void ConfigureVendingMachine(int vmIndex, List<string> popNames, List<int> popCosts) {
        Console.WriteLine("VM: Configuring vending machine...");
        VendingMachine machine = getMachineById(vmIndex);
        machine.Configure(popNames, popCosts);
    }

    public void LoadCoins(int vmIndex, int coinKindIndex, List<Coin> coins) {
        Console.WriteLine("VM: Loading coins...");
        VendingMachine machine = getMachineById(vmIndex);
        // create array of length equal to coin rack size
        CoinRack[] coinRacks = machine.CoinRacks;
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
        machine.LoadCoins(coinArray);
    }

    public void LoadPops(int vmIndex, int popKindIndex, List<PopCan> pops) {
        Console.WriteLine("VM: Loading pops..");
        VendingMachine machine = getMachineById(vmIndex);
        // create array of length the desired index
        PopCanRack[] popRacks = machine.PopCanRacks;
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
        machine.LoadPopCans(popArray);
    }

    public void InsertCoin(int vmIndex, Coin coin) {
        Console.WriteLine("VM: Inserting coins...");
    }

    public void PressButton(int vmIndex, int value) {
        Console.WriteLine("VM: Pressing button...");

    }

    public List<IDeliverable> ExtractFromDeliveryChute(int vmIndex) {
        Console.WriteLine("VM: Extracting...");
        return new List<IDeliverable>();
    }

    public VendingMachineStoredContents UnloadVendingMachine(int vmIndex) {
        Console.WriteLine("VM: Unloading...");
        return new VendingMachineStoredContents();
    }

    public VendingMachine getMachineById(int id)
    {
        VendingMachine machine = null;
        int counter = 0;
        foreach(VendingMachine vm in createdMachines)
        {
            if (counter == id)
            {
                machine = vm;
                break;
            }
            counter++;
        }
        if (machine == null)
            throw new Exception("Invalid vending machine index: " + id);

        return machine;
    }
}