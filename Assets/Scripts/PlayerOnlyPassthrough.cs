using UnityEngine;

public class PlayerOnlyPassthrough : MonoBehaviour
{
    private Collider objCollider;
    
    void Start()
    {
        objCollider = GetComponent<Collider>();

        int playerLayer = LayerMask.NameToLayer("Player");
        Physics.IgnoreLayerCollision(gameObject.layer, playerLayer, true);
    }
}