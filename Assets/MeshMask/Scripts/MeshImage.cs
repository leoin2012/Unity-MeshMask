/* ==============================================================================
 * 功能描述：利用修改图片顶点原理的Mask(相比Unity自带Mask少消耗1-2个DrawCall)
 *           与MeshMask配合使用
 * 用法：把该脚本挂到想要修改遮罩的Image组件GO上          
 * 创 建 者：shuchangliu
 * ==============================================================================*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

[AddComponentMenu("UI/Custom/Mesh Image")]
[RequireComponent(typeof(Image))]
public class MeshImage : BaseMeshEffect
{
    public MeshMask mask;
    [Tooltip("每次ModifyMesh时让Mask与Image做差集重建网格，Mask会退化成凸边形，解决SpriteWrap问题(不建议勾选)")]
    public bool isRebuildMeshMask;

    private List<UIVertex> _uiVertices = new List<UIVertex>();

    private Vector3 lastPos = Vector3.zero;
    private Quaternion lastRot = Quaternion.identity;
    private Vector3 lastScale = Vector3.one;

    private Image _image;
    public Image image
    {
        get
        {
            if (_image == null)
            {
                _image = this.GetComponent<Image>();
            }
            return _image;
        }
    }

    void Update()
    {
        if (transform.localPosition != lastPos || transform.localRotation != lastRot || transform.localScale != lastScale)
        {
            graphic.SetVerticesDirty();
            lastPos = transform.localPosition;
            lastRot = transform.localRotation;
            lastScale = transform.localScale;
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (this.enabled)
        {
            vh.Clear();
            _uiVertices.Clear();
            
            if (mask)
            {
                List<Vector2> vertices = mask.vertices.Select(
                    x => { return this.transform.InverseTransformPoint(this.mask.transform.TransformPoint(x)); }).Select(x => { return new Vector2(x.x, x.y); }).ToList();
                List<int> triangles = mask.triangles;

                if (isRebuildMeshMask)
                {
#if UNITY_EDITOR
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
#endif

                    List<Vector2> interPoly;
                    List<Vector2> localPoly = new List<Vector2>();
                    Rect rect = this.transform.GetComponent<RectTransform>().rect;
                    localPoly.Add(new Vector2(rect.xMin, rect.yMin));
                    localPoly.Add(new Vector2(rect.xMin, rect.yMax));
                    localPoly.Add(new Vector2(rect.xMax, rect.yMax));
                    localPoly.Add(new Vector2(rect.xMax, rect.yMin));

                    if (ImageUtil.PolygonClip(vertices, localPoly, out interPoly))
                    {
                        if (interPoly[0] == interPoly[interPoly.Count - 1])
                            interPoly.RemoveAt(interPoly.Count - 1);

                        vertices = interPoly;
                        Triangulator tr = new Triangulator(interPoly.ToArray());
                        triangles = tr.Triangulate().ToList();

#if UNITY_EDITOR
                        sw.Stop();
                        UnityEngine.Debug.Log("重建Mask网格耗时:" + sw.ElapsedMilliseconds + "ms," + sw.ElapsedTicks + "ticks");
#endif
                    }

#if UNITY_EDITOR
                    sw.Stop();
                    sw = null;
#endif
                }

                if (vertices != null && triangles != null)
                {
                    float tw = image.rectTransform.rect.width;
                    float th = image.rectTransform.rect.height;
                    Vector4 uv = image.overrideSprite != null ? DataUtility.GetInnerUV(image.overrideSprite) : Vector4.zero;
                    float w = uv.z - uv.x;
                    float h = uv.w - uv.y;
                    float uvScaleX = w / tw;
                    float uvScaleY = h / th;
                    float uvCenterX = uv.x + w * image.rectTransform.pivot.x;
                    float uvCenterY = uv.y + h * image.rectTransform.pivot.y;

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        UIVertex v = new UIVertex();
                        v.color = image.color;
                        v.position = vertices[i];
                        v.uv0 = new Vector2(v.position.x * uvScaleX + uvCenterX, v.position.y * uvScaleY + uvCenterY);
                        _uiVertices.Add(v);
                    }

                    vh.AddUIVertexStream(_uiVertices, triangles);
                }
            }

        }
    }

    

    

}
