using UnityEngine;

public class AmbienceSound : MonoBehaviour
{
    public Collider Area;       //Area of the sound
    public GameObject Player;       //Object to tracck

    // Update is called once per frame
    void Update()
    {
        //Locate closest point to the player
        Vector3 closestPoint = Area.ClosestPoint(Player.transform.position);
        // Set position to closest point to the player
        transform.position = closestPoint; 
    }
}
