using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianFlightControl : MonoBehaviour
{
    public bool RotateShipDuringFlight = false;
    Rigidbody2D OurRB { get; set; }
    bool FacingRight { get; set; } = true;

    void Start()
    {
        OurRB = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if ((!FacingRight && OurRB.velocity.x > 0) || (FacingRight && OurRB.velocity.x < 0))
            Flip();

        if (RotateShipDuringFlight)
        {
            if (OurRB.velocity.x != 0)
            {
                // Rotate toward the velocity vector
                float newRotation = Mathf.Atan(OurRB.velocity.y / OurRB.velocity.x);
                //Quaternion deltaRotation = Quaternion.Euler(0, 0, newRotation);
                OurRB.rotation = newRotation * Mathf.Rad2Deg;
                // float rotationDelta = OurRB.rotation - (newRotation * Mathf.Rad2Deg);
                // OurRB.rotation = OurRB.rotation + rotationDelta * Time.deltaTime * 25;
            }
        }
    }

    private void Flip()
    {
        FacingRight = !FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

}
