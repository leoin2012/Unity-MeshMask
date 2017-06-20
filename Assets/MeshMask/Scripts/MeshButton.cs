/* ==============================================================================
 * 功能描述：利用MeshMask组件的顶点信息，通过Ray-Crossing算法来确定点击判断（像素级点击）
 * 优势：相比Unity自带像素级点击方案，不需要设置Sprite为Read/Write Enable，且Sprite可以合入图集
 * 创 建 者：shuchangliu
 * ==============================================================================*/

using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Custom/Mesh Button")]
[RequireComponent(typeof(Image))]
public class MeshButton : UIBehaviour, ICanvasRaycastFilter
{

    public MeshMask mask;

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

    #region ICanvasRaycastFilter
    public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        //Stopwatch sw = new Stopwatch();
        //sw.Start();

        Sprite sprite = image.overrideSprite;
        if (sprite == null)
            return true;

        bool ret = true;
        if (this.mask != null && this.mask.vertices != null)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(image.rectTransform, screenPoint, eventCamera, out local);

            List<Vector2> vertices = this.mask.vertices.Select(
                x =>
                {
                    Vector3 p = this.transform.InverseTransformPoint(this.mask.transform.TransformPoint(x));
                    return new Vector2(p.x, p.y);
                }).ToList();

            ret = ImageUtil.Contains(local, vertices);
        }

        //sw.Stop();
        //UnityEngine.Debug.Log("点击检测耗时:" + sw.ElapsedTicks + " tick");

        return ret;
    }
    #endregion
}
