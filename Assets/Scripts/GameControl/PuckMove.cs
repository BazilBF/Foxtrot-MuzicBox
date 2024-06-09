using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PuckMove : MonoBehaviour
{
    public float currentScaleCoef = 0.0F;
    public float currentScale = 0.0F;

    public float destinationY = 500;

    public float a = 0.0F;

    public float duration = 2.0F;
    public float curY;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody2D>() as Rigidbody2D;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.transform.localScale = Vector3.zero;
        this.curY = this.transform.position.y;
        this.a = (2 * destinationY) / Mathf.Pow(this.duration, 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.currentScale < 1.0)
        {
            this.currentScaleCoef += Time.deltaTime;
            this.rb.velocity = new Vector3(0,this.a*currentScaleCoef,0);
            float phase = Mathf.Lerp(0.0f, 1.0f, this.currentScaleCoef / this.duration);
            this.currentScale = 1.0F - Mathf.Pow(1 - phase, 2);
            this.transform.transform.localScale = new Vector3(this.currentScale, this.currentScale, this.currentScale);
        }
        else
        {
            this.rb.velocity =Vector3.zero;
        }
    }
}
