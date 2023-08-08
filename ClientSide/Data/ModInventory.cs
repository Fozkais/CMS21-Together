namespace CMS21MP.ClientSide.Data
{
    public static class ModInventory
    {
        public static void UpdateInventory()
        {
            HandleNewItem();
            HandleNewGroupItem();
        }
        
        public static void HandleNewItem()
        {
            var localInventory = ClientData.localInventory.items;

            foreach (var item in localInventory)
            {
                if (!ClientData.playerInventory.Contains(item))
                {
                    ClientData.playerInventory.Add(item);
                    //TODO: Send Inventory packet
                }
            }

            foreach (var item in ClientData.playerInventory)
            {
                if (!localInventory._items.Contains(item))
                {
                    //TODO: Send Inventory packet
                    ClientData.playerInventory.Remove(item);
                }
            }
        }

        public static void HandleNewGroupItem()
        {
            var localInventory = ClientData.localInventory.groups;

            foreach (var item in localInventory)
            {
                if (!ClientData.playerGroupInventory.Contains(item))
                {
                    ClientData.playerGroupInventory.Add(item);
                    //TODO: Send Inventory packet
                }
            }

            foreach (var item in ClientData.playerGroupInventory)
            {
                if (!localInventory._items.Contains(item))
                {
                    //TODO: Send Inventory packet
                    ClientData.playerGroupInventory.Remove(item);
                }
            }
        }
    }
}