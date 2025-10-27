using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleManager manager;
    public CollectibleType type;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.CollectItem(this);
            Destroy(gameObject);
        }
    }
}

public enum CollectibleType
{
    Dish,
    Clothing,
    Food,
    Key
}