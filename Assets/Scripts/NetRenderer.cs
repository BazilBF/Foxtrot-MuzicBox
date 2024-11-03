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
    public Material lineMaterial = null;
    public float bottomWidthMultipliyer = 10.0F;
    private LineRenderer _lineRenderer;

    private LineRenderer[] _verticalLineRenderers = null;

    private UnityEngine.Color _lineStartColor;
    private UnityEngine.Color _lineEndColor;

    private UnityEngine.Color _lineStartColorDelta;
    private UnityEngine.Color _lineEndColorDelta;

    private float _colorPhase = 0.0F;
    private float _colorPhaseDelta = 0.0F;


    void Awake()
    {


    }

    public void SetNetRenderer(int inVerticalLineCnt, float inGridCellBottomHeight, Vector2 inTopLeft, float inWidth, float inHeight, UnityEngine.Color inStartColor, UnityEngine.Color inEndColor, float inDuration)
    {

        this._lineRenderer = gameObject.AddComponent<LineRenderer>();
        this._lineRenderer.material = lineMaterial;
        this._lineRenderer.startWidth = 0.02f;
        this._lineRenderer.endWidth = 0.05f;
        this._lineStartColor = inStartColor;
        this._lineEndColor = inEndColor;


        this._lineRenderer.startColor = inStartColor;
        this._lineRenderer.endColor = inEndColor;

        this._horizontalLineCnt = (int)Mathf.Log(0.01F, inGridCellBottomHeight);
        this._verticalLineCnt = inVerticalLineCnt;

        this._lineRenderer.positionCount = this._horizontalLineCnt * 2;

        this._width = inWidth;
        this._height = inHeight;

        this._topLeft = inTopLeft;

        this._gridCellBottomHeight = inGridCellBottomHeight;


        this._duration = inDuration;

        float tmpVerticalWidth = inWidth / this._verticalLineCnt;
        float tmpVerticalLineEndLeftX = this._topLeft.x * this.bottomWidthMultipliyer;

        this._verticalLineRenderers = new LineRenderer[this._verticalLineCnt - 1];

        for (int i = 1; i < this._verticalLineCnt; i++)
        {
            GameObject tmpLineGameObject = new GameObject();
            tmpLineGameObject.transform.parent = this.transform;
            LineRenderer tmpLineRenderer = tmpLineGameObject.AddComponent<LineRenderer>();
            tmpLineRenderer.material = lineMaterial;
            tmpLineRenderer.startWidth = 0.02f;
            tmpLineRenderer.endWidth = 0.05f;

            tmpLineRenderer.startColor = inStartColor;
            tmpLineRenderer.endColor = inEndColor;
            tmpLineRenderer.positionCount = 2;

            tmpLineRenderer.SetPosition(0, new Vector3(this._topLeft.x + i * tmpVerticalWidth, this._topLeft.y, 0.0F));
            tmpLineRenderer.SetPosition(1, new Vector3(tmpVerticalLineEndLeftX + tmpVerticalWidth * this.bottomWidthMultipliyer * i, this._topLeft.y - this._height - 0.5F, 0.0F));

            this._verticalLineRenderers[i - 1] = tmpLineRenderer;
        }

        GameObject tmpHorizonGameObject = new GameObject();
        tmpHorizonGameObject.transform.parent = this.transform;
        LineRenderer tmpHorizonLineRenderer = tmpHorizonGameObject.AddComponent<LineRenderer>();
        tmpHorizonLineRenderer.material = lineMaterial;
        tmpHorizonLineRenderer.startWidth = 0.02f;

        tmpHorizonLineRenderer.startColor = inStartColor;
        tmpHorizonLineRenderer.positionCount = 2;

        tmpHorizonLineRenderer.SetPosition(0, new Vector3(this._topLeft.x - 0.1F, this._topLeft.y, 0.0F));
        tmpHorizonLineRenderer.SetPosition(1, new Vector3(this._topLeft.x + this._width + 0.1F, this._topLeft.y, 0.0F));


    }

    void Update()
    {
        if (this._lineRenderer != null)
        {
            int pointIndex = 0;
            for (int i = _horizontalLineCnt; i > 0; i--)
            {
                float startPointX = this._topLeft.x + (i % 2 == 0 ? -0.1F : this._width + 0.1F);
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

        if (this._colorPhaseDelta > 0.0F)
        {
            UdpateColors();
        }
    }

    public void StartChangeColor(UnityEngine.Color inStartColor, UnityEngine.Color inEndColor, float inChangeDuration)
    {

        this._colorPhase = 0.0F;
        this._colorPhaseDelta = 1.0F / inChangeDuration;

        this._lineStartColorDelta = new UnityEngine.Color(inStartColor.r - this._lineStartColor.r, inStartColor.g - this._lineStartColor.g, inStartColor.b - this._lineStartColor.b);
        this._lineEndColorDelta = new UnityEngine.Color(inEndColor.r - this._lineEndColor.r, inEndColor.g - this._lineEndColor.g, inEndColor.b - this._lineEndColor.b);

    }

    private void UdpateColors()
    {
        this._colorPhase += Time.deltaTime * this._colorPhaseDelta;
        UnityEngine.Color tmpNewStartColor;
        UnityEngine.Color tmpNewEndColor;
        if (this._colorPhase < 1.0F)
        {
            tmpNewStartColor = new UnityEngine.Color(this._lineStartColor.r + this._lineStartColorDelta.r * this._colorPhase, this._lineStartColor.g + this._lineStartColorDelta.g * this._colorPhase, this._lineStartColor.b + this._lineStartColorDelta.b * this._colorPhase);
            tmpNewEndColor = new UnityEngine.Color(this._lineEndColor.r + this._lineEndColorDelta.r * this._colorPhase, this._lineEndColor.g + this._lineEndColorDelta.g * this._colorPhase, this._lineEndColor.b + this._lineEndColorDelta.b * this._colorPhase);
        }
        else
        {
            this._colorPhaseDelta = 0.0F;
            this._colorPhase = 0.0F;
            this._lineStartColor = new UnityEngine.Color(this._lineStartColor.r + this._lineStartColorDelta.r, this._lineStartColor.g + this._lineStartColorDelta.g, this._lineStartColor.b + this._lineStartColorDelta.b);
            this._lineEndColor = new UnityEngine.Color(this._lineEndColor.r + this._lineEndColorDelta.r, this._lineEndColor.g + this._lineEndColorDelta.g, this._lineEndColor.b + this._lineEndColorDelta.b);
            tmpNewStartColor = this._lineStartColor;
            tmpNewEndColor = this._lineEndColor;
        }
        this._lineRenderer.startColor = tmpNewStartColor;
        this._lineRenderer.endColor = tmpNewEndColor;

        for (int i = 0; i < this._verticalLineRenderers.Length; i++)
        {
            this._verticalLineRenderers[i].startColor = tmpNewStartColor;
            this._verticalLineRenderers[i].endColor = tmpNewEndColor;
        }
    }

    public void SetDuration(float inNewDuration)
    {
        this._duration = inNewDuration;
    }

}
