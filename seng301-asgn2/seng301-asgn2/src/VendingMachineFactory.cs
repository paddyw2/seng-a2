using System;
using System.Collections.Generic;
using Frontend2;
using Frontend2.Hardware;

public class VendingMachineFactory : IVendingMachineFactory {
    private List<VendingMachineSoftware> createdMachines = new List<VendingMachineSoftware>();

    public int CreateVendingMachine(List<int> coinKinds, int selectionButtonCount, int coinRackCapacity, int popRackCapcity, int receptacleCapacity) {
        VendingMachineSoftware newMachine = new VendingMachineSoftware(coinKinds, selectionButtonCount, coinRackCapacity, popRackCapcity, receptacleCapacity);
        Console.WriteLine("VM: Creating vending machine...");
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
        VendingMachineSoftware machine = getMachineById(vmIndex);
        machine.configure(popNames, popCosts);
    }

    public void LoadCoins(int vmIndex, int coinKindIndex, List<Coin> coins) {
        Console.WriteLine("VM: Loading coins...");
        VendingMachineSoftware machine = getMachineById(vmIndex);
        machine.loadCoins(coinKindIndex, coins);
    }

    public void LoadPops(int vmIndex, int popKindIndex, List<PopCan> pops) {
        Console.WriteLine("VM: Loading pops..");
        VendingMachineSoftware machine = getMachineById(vmIndex);
        machine.loadPops(popKindIndex, pops);
    }

    public void InsertCoin(int vmIndex, Coin coin) {
        Console.WriteLine("VM: Inserting coins...");
        VendingMachineSoftware machine = getMachineById(vmIndex);
        machine.insertCoin(coin);
    }

    public void PressButton(int vmIndex, int value) {
        Console.WriteLine("VM: Pressing button...");
        VendingMachineSoftware machine = getMachineById(vmIndex);
        machine.pressButton(value);
    }

    public List<IDeliverable> ExtractFromDeliveryChute(int vmIndex) {
        Console.WriteLine("VM: Extracting...");
        VendingMachineSoftware machine = getMachineById(vmIndex);
        List<IDeliverable> deliverable = machine.extractChute();
        return deliverable;
    }

    public VendingMachineStoredContents UnloadVendingMachine(int vmIndex) {
        Console.WriteLine("VM: Unloading...");
        VendingMachineSoftware machine = getMachineById(vmIndex);
        return machine.unload();
    }

    public VendingMachineSoftware getMachineById(int id)
    {
        VendingMachineSoftware machine = null;
        int counter = 0;
        foreach(VendingMachineSoftware vm in createdMachines)
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