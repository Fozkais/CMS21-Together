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

            for (int i = 0; i < localInventory._items.Count; i++)
            {
                if (!ClientData.playerInventory.Contains(localInventory._items[i]))
                {
                    ClientData.playerInventory.Add(localInventory._items[i]);
                    //TODO: Send Inventory packet
                }
            }

            for (int i = 0; i < ClientData.playerInventory.Count; i++)
            {
                if (!localInventory._items.Contains(ClientData.playerInventory[i]))
                {
                    //TODO: Send Inventory packet
                    ClientData.playerInventory.Remove(ClientData.playerInventory[i]);
                }
            }
            
        }

        public static void HandleNewGroupItem()
        {
            var localInventory = ClientData.localInventory.groups;


            for (int i = 0; i < localInventory._items.Count; i++)
            {
                var item = localInventory._items[i];
                if (!ClientData.playerGroupInventory.Contains(item))
                {
                    ClientData.playerGroupInventory.Add(item);
                    //TODO: Send Inventory packet
                }
            }

            for (int i = 0; i < ClientData.playerGroupInventory.Count; i++)
            {
                var item = ClientData.playerGroupInventory[i];
                if (!localInventory._items.Contains(item))
                {
                    //TODO: Send Inventory packet
                    ClientData.playerGroupInventory.Remove(item);
                }
            }
        }
    }
}