using UnityEngine;
using Fusion;

public class FinishBoatPuzzle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var boat = other.GetComponent<BoatController>();

        if (boat == null)
        {
            boat = other.GetComponentInParent<BoatController>();
        }

        if (boat != null)
        {
            Debug.Log("Point B Reached");
            boat.CompletePuzzle();
        }
    }
}