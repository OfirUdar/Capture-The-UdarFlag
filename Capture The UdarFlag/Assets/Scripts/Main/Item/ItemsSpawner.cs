using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class ItemsSpawner : MonoBehaviour
{
    public static ItemsSpawner Instance { get; private set; }
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    [SerializeField] private ItemSpawnMethod _itemsSpawnMethod;
    [Tooltip("The accuracy pointof the item spawn")]
    [SerializeField] private float _accuracySpawn = 1.5f;
    [Header("Items To Spawn")]
    [SerializeField] private List<ItemToSpawn> _itemsToSpawn = new List<ItemToSpawn>();

    private void Awake()
    {
        Instance = this;
    }

    public void CreateItems()
    {      
        List<Item> items = ArrangeItems();
        SpawnItems(items);

    }


    private List<Item> ArrangeItems() //arrange the items to list
    {
        List<Item> items = new List<Item>();
        for (int i = 0; i < _itemsToSpawn.Count; i++)
        {
            foreach (ItemToSpawn item in _itemsToSpawn)
            {
                if (i < item.amount)
                    items.Add(item.itemPfb);
            }
        }
        return items;
    }

    private void SpawnItems(List<Item> items)
    {
        int index = 0;
        foreach (Item item in items)
        {
            Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            if (_itemsSpawnMethod == ItemSpawnMethod.RoundRobin)
            {
                if (index >= _spawnPoints.Count) { index = 0; }

                point = _spawnPoints[index];

                index++;
            }
            SpawnItem(item, point);
        }
    }

    private void SpawnItem(Item item, Transform point)
    {
      Vector3 randomPos = new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
        Item itemInstance = Instantiate(item, randomPos + point.position, Quaternion.identity);
        NetworkServer.Spawn(itemInstance.gameObject);
    }
}
[System.Serializable]
public class ItemToSpawn
{
    public Item itemPfb;
    public int amount;
}
public enum ItemSpawnMethod
{
    Random,
    RoundRobin
}
