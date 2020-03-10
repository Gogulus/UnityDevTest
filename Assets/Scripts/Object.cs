using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    /// <summary>
    /// Flower information is stored here.
    /// The falling mechanism for the flower is also stored here.
    /// 
    /// The board will give flowers new coordinates though the newPositionY variable.
    /// </summary>
    /// 

    //Every type of Flower should be added here. The prefab is set to the correct type in the editor through this script.
    public enum ObjectType { Blue, Red, Yellow, Green, Purple }

    //If Flower is in place (after falling)
    private bool objectIsInPlace;

    [HideInInspector]
    public float newPositionY;

    //Set type in Prefab through the Inspector window
    public ObjectType blockType;

    [Header("It is recommended to have close to the same values on all the flower types.")]
    public float fallSpeed = 9;
    public float fallMargin = 0.2f;

    //Flower is spawning. Set the starting values and play the spawn behaviour (falling to the board).
    void Awake()
    {
        //Prepare for falling effect
        newPositionY = this.gameObject.transform.position.y;

        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, Random.Range(5, 6f));

        objectIsInPlace = false;

    }

    void Update()
    {
        ObjectBehaviour();
    }

    private void ObjectBehaviour()
    {
        //If the flower needs to fall, push down. Then set to exact position.
        if (this.gameObject.transform.position.y > newPositionY + fallMargin)
        {
            this.gameObject.transform.Translate(Vector3.down * Time.smoothDeltaTime * fallSpeed);
            objectIsInPlace = false;
        }
        else
        {
            //When in position, set in correct y position.
            this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, newPositionY);
            objectIsInPlace = true;
        }
    }

    public bool ObjectIsReady()
    {
        return objectIsInPlace;
    }
}
