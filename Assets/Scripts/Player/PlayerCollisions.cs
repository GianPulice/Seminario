using UnityEngine;

public class PlayerCollisions
{
    private PlayerController playerController;


    public PlayerCollisions(PlayerController playerController)
    {
        this.playerController = playerController;
    }


    public void OnCollisionsEnter(Collision collision)
    {

    }

    public void OnCollisionsStay(Collision collision)
    {

    }

    public void OnCollisionsExit(Collision collision)
    {

    }

    public void OnTriggerEnter(Collider collider)
    {
        //OnTriggerEnterWithPortalDungeon(collider);
    }


    /* ----------------------------------------COLLISION ENTER-------------------------------------------- */

    /* ----------------------------------------COLLISION STAY--------------------------------------------- */

    /* ----------------------------------------COLLISION EXIT--------------------------------------------- */

    /* ----------------------------------------TRIGGER ENTER---------------------------------------------- */

    private void OnTriggerEnterWithPortalDungeon(Collider collider)
    {
        if (collider.gameObject.CompareTag("PortalDungeon"))
        {
            collider.gameObject.SetActive(false); 
        }
    }
}
