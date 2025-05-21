using UnityEngine;

public class Lootbox : MonoBehaviour
{
    private Player player;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);

            DropBox();

            Destroy(gameObject);
        }
    }

    private void DropBox()
    {
        if (player == null) return;

        int randomBonus = Random.Range(0, 2);

        switch (randomBonus)
        {
            case 0:
                player.baseMovementSpeed += 10f;
                break;

            case 1:
                player.transform.localScale *= 1.5f;
                break;
        }
    }
}
