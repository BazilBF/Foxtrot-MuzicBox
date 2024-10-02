using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class NetRenderer : MonoBehaviour
{


    private int _horizontalLineCnt = 0;
    private int _verticalLineCnt = 0;

    private float _gridCellBottomHeight = 0.0F;

    private float _width = 0.0F;
    private float _height = 0.0F;

    private Vector2 _topLeft = Vector2.zero;

    public float _phase = 0.0F;

    private float _duration = 0.0F;
    public Material lineMaterial= null;
    public float bottomWidthMultipliyer = 1.5F;
    LineRenderer _lineRenderer;


    void Awake()
    {
        
        
    }

    public void SetNetRenderer(int inVerticalLineCnt, float inGridCellBottomHeight, Vector2 inTopLeft, float inWidth, float inHeight, UnityEngine.Color inStartColor, UnityEngine.Color inEndColor,float inDuration) {

        this._lineRenderer = gameObject.AddComponent<LineRenderer>();
        this._lineRenderer.material = lineMaterial;
        this._lineRenderer.startWidth = 0.02f;
        this._lineRenderer.endWidth = 0.05f;

        

        this._lineRenderer.startColor = inStartColor;
        this._lineRenderer.endColor = inEndColor;

        this._horizontalLineCnt = (int)Mathf.Log(0.01F,inGridCellBottomHeight);
        this._verticalLineCnt = inVerticalLineCnt;

        this._lineRenderer.positionCount = this._horizontalLineCnt * 2;

        this._width = inWidth;
        this._height = inHeight;

        this._topLeft = inTopLeft;

        this._gridCellBottomHeight = inGridCellBottomHeight;
        

        this._duration = inDuration;

        float tmpVerticalWidth = inWidth / this._verticalLineCnt;
        float tmpVerticalLineEndLeftX = this._topLeft.x * this.bottomWidthMultipliyer;

        for (int i = 1; i < this._verticalLineCnt; i++) {
            GameObject tmpLineGameObject =new GameObject();
            tmpLineGameObject.transform.parent = this.transform;
            LineRenderer tmpLineRenderer = tmpLineGameObject.AddComponent<LineRenderer>();
            tmpLineRenderer.material = lineMaterial;
            tmpLineRenderer.startWidth = 0.02f;
            tmpLineRenderer.endWidth = 0.05f;

            tmpLineRenderer.startColor = inStartColor;
            tmpLineRenderer.endColor = inEndColor;
            tmpLineRenderer.positionCount = 2;

            tmpLineRenderer.SetPosition(0, new Vector3(this._topLeft.x+ i *tmpVerticalWidth, this._topLeft.y, 0.0F));
            tmpLineRenderer.SetPosition(1, new Vector3(tmpVerticalLineEndLeftX + tmpVerticalWidth * this.bottomWidthMultipliyer*i, this._topLeft.y - this._height - 0.5F, 0.0F));
        }

        GameObject tmpHorizonGameObject = new GameObject();
        tmpHorizonGameObject.transform.parent = this.transform;
        LineRenderer tmpHorizonLineRenderer = tmpHorizonGameObject.AddComponent<LineRenderer>();
        tmpHorizonLineRenderer.material = lineMaterial;
        tmpHorizonLineRenderer.startWidth = 0.02f;

        tmpHorizonLineRenderer.startColor = inStartColor;
        tmpHorizonLineRenderer.positionCount = 2;

        tmpHorizonLineRenderer.SetPosition(0, new Vector3(this._topLeft.x -0.1F, this._topLeft.y, 0.0F));
        tmpHorizonLineRenderer.SetPosition(1, new Vector3(this._topLeft.x + this._width + 0.1F, this._topLeft.y, 0.0F));





        /*float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(inStartColor, 0.0f), new GradientColorKey(inEndColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        this._lineRenderer.colorGradient = gradient;*/

    }

    void Update() {
        if (this._lineRenderer != null)
        {
            int pointIndex = 0;
            for (int i = _horizontalLineCnt; i > 0; i--)
            {
                float startPointX = this._topLeft.x + (i % 2 == 0 ? -0.1F : this._width+0.1F);
                float endPointX = this._topLeft.x + (i % 2 == 0 ? this._width + 0.1F : -0.1F);

                float lineY = this._topLeft.y - Mathf.Pow(this._gridCellBottomHeight, i - this._phase) * this._height;

                this._lineRenderer.SetPosition(pointIndex++, new Vector3(startPointX, lineY, 0.0F));
                this._lineRenderer.SetPosition(pointIndex++, new Vector3(endPointX, lineY, 0.0F));
            }

            
        }

        this._phase += Time.deltaTime * (1.0F / this._duration);
        if (this._phase > 1.0F)
        {
            this._phase = 0.0F;
        }
    }

    public void SetDuration(float inNewDuration) {
        this._duration = inNewDuration;
    }

}
