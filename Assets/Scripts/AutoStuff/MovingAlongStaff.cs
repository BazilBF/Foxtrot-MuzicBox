using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingAlongStaff : MonoBehaviour
{

    SpriteRenderer _objectSpriteRenderer;

    GameController _gameController;

    private float _currentScaleCoef = 0.0F;
    private Vector2 _currentScale = Vector2.zero;
    private Vector2 _targetScale = Vector2.zero;

    private float _startY = 0.0F;
    private float _startX = 0.0F;
    private float _destinationY = 500.0F;
    private float _destinationX = 0.0F;
    private bool _destroyObjectAtDestination = false;

    private float _offsetDestinationY = 0.01F;

    private float _duration = 2.0F;
    private float _speed;
    private float _initControllerSpeedCoef;

    private Vector2 _targetSize = Vector2.zero;

    private float _phase = 0.0F;

    public void SetObjectMove (Vector2 inTargetSize, Vector2 inDestination, float inDuration, GameController inGameController) {
        this._targetSize = inTargetSize;

        if (_objectSpriteRenderer != null) {
            float newScaleWidth = this._targetSize.x / _objectSpriteRenderer.sprite.bounds.size.x;
            float newScaleHeight = this._targetSize.y / _objectSpriteRenderer.sprite.bounds.size.y;
            this._targetScale = new Vector2(newScaleWidth, newScaleHeight);
            
        }
        else {
            this._targetScale = inTargetSize;
         }
        this.transform.transform.localScale = Vector3.zero;

        this._gameController = inGameController;
        this._speed = 1.0F / inDuration;

        this._initControllerSpeedCoef = inGameController.GetCurrentSpeedCoef();

        this._startY = this.transform.localPosition.y;
        this._startX = this.transform.localPosition.x;
        this._destinationY = inDestination.y;
        this._destinationX = inDestination.x;
        this._duration = inDuration;
    }

    public void SetObjectMove(Vector2 inTargetSize, Vector2 inDestination, float inDuration, bool inDstroyObjectAtDestination, GameController inGameController)
    {
        this.SetObjectMove(inTargetSize, inDestination, inDuration, inGameController);
        this._destroyObjectAtDestination = inDstroyObjectAtDestination;
    }

        void Awake()
    {
        
        _objectSpriteRenderer = this.GetComponent<SpriteRenderer>();
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        this._currentScaleCoef += Time.deltaTime;
        //this._rb.velocity = new Vector3(this.accelerationX * _currentScaleCoef, this.accelerationY * _currentScaleCoef, 0);

        float distanceX = this._destinationX - this._startX;
        float distanceY = this._destinationY - this._startY;

        if (this._phase < 1.0)
        {
            this._phase += Time.deltaTime * this._speed * (this._gameController.GetCurrentSpeedCoef() / this._initControllerSpeedCoef);
            float newLocalPositionX = this._startX + GetNewMovingValue(distanceX, this._phase);
            float newLocalPositionY = this._startY + GetNewMovingValue(distanceY, this._phase);

            this._currentScale.x = GetNewMovingValue(this._targetScale.x, this._phase);
            this._currentScale.y = GetNewMovingValue(this._targetScale.y, this._phase);

            this.transform.transform.localScale = new Vector3(this._currentScale.x, this._currentScale.y, 0.0F);
            this.transform.transform.localPosition = new Vector3(newLocalPositionX, newLocalPositionY, 0.0F);
        }
        else
        {
            if (this._phase != 1.0F) {
                this._phase = 1.0F;
                this.transform.transform.localPosition = new Vector3(this._destinationX, this._destinationY, 0.0F);
            }
            if (this._destroyObjectAtDestination)
            {
                Destroy(this.gameObject);
            }
        }

    }

    private float GetNewMovingValue(float inTargetMovin, float inPhase) {
        return inTargetMovin * Mathf.Pow(inPhase, 2);
    }

    public float GetPhase() {
        return this._phase;
    }
}
