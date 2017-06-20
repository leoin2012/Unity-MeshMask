/* ==============================================================================
 * 功能描述：利用修改图片顶点原理的Mask(相比Unity自带Mask少消耗1-2个DrawCall)
 *           与MeshImage配合使用
 * 用法：1、类似Unity自带的Mask，在Image组件拖放好Mask图片后，点击MeshMask组件菜单->生成Mesh Mask即可。子GO的Image将会被自动附上Mesh Image组件并修改顶点，变成遮罩后的形状。
 *       2、可以对PolygonCollider2D组件进行形状微调，点击MeshMask组件菜单->更新Mesh Mask。遮罩形状将刷新
 * 创 建 者：shuchangliu
 * ==============================================================================*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI/Custom/Mesh Mask")]
[RequireComponent(typeof(PolygonCollider2D))]
public class MeshMask : MonoBehaviour{

    //----------------------Inspector字段-------------
    [ReadOnly]
    [Tooltip("根据PolygonCollider2D自动算出")]
    public List<int> triangles;
    [ReadOnly]
    [Tooltip("根据PolygonCollider2D自动算出")]
    public List<Vector2> vertices;
    //-----------------------------------------------

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

    private PolygonCollider2D _polygon2d;
    public PolygonCollider2D polygon2d
    {
        get
        {
            if (_polygon2d == null)
            {
                _polygon2d = this.GetComponent<PolygonCollider2D>();
            }
            return _polygon2d;
        }
    }

    public void Awake()
    {
        List<Image> childs = this.GetComponentsInChildren<Image>().ToList();
        foreach (var child in childs)
        {
            if (child.gameObject != this.gameObject)
            {
                MeshImage mi = null;
                if (child.gameObject.GetComponent<MeshImage>() != null)
                {
                    mi = child.gameObject.GetComponent<MeshImage>();
                    mi.mask = this;
                }

                MeshRawImage mri = null;
                if (child.gameObject.GetComponent<MeshRawImage>() != null)
                {
                    mri = child.gameObject.GetComponent<MeshRawImage>();
                    mri.mask = this;
                }

                MeshButton mb = null;
                if (child.gameObject.GetComponent<MeshButton>() != null)
                {
                    mb = child.gameObject.GetComponent<MeshButton>();
                    mb.mask = this;
                }
            }
        }
    }

    #region ContextMenu
    [ContextMenu("根据Image组件生成Mask")]
    private void CreateMeshMaskByImage()
    {
#if UNITY_EDITOR
        GameObject o = Selection.activeGameObject;
        if (o != null && o.GetComponent<Image>() != null)
        {
            Image img = o.GetComponent<Image>();
            string path = AssetDatabase.GetAssetPath(img.mainTexture);
            if (!string.IsNullOrEmpty(path))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                TextureImporterSettings cacheSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(cacheSettings);

                //将Texture临时设置为可读写
                TextureImporterSettings tmp = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(tmp);
                tmp.readable = true;
                textureImporter.SetTextureSettings(tmp);
                AssetDatabase.ImportAsset(path);

                SobelEdgeDetection sobel = new SobelEdgeDetection();

                Texture2D targetT2d = sobel.Detect(img.mainTexture as Texture2D);

                List<Vector2> vertices = EdgeUtil.GetPoints(targetT2d);

                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 vec = vertices[i];
                    vec.x -= targetT2d.width * 0.5f;
                    vec.y -= targetT2d.height * 0.5f;
                    vertices[i] = vec;
                }

                //恢复Texture设置
                textureImporter.SetTextureSettings(cacheSettings);
                AssetDatabase.ImportAsset(path);

                MeshMask mm = o.GetComponent<MeshMask>();
                PolygonCollider2D polygon2d = o.GetComponent<PolygonCollider2D>();
                if (mm == null)
                {
                    mm = o.AddComponent<MeshMask>();
                }
                if (polygon2d == null)
                {
                    polygon2d = o.AddComponent<PolygonCollider2D>();
                }

                vertices = vertices.Select(p =>
                {
                    return new Vector2(p.x / img.mainTexture.width * image.rectTransform.rect.width, p.y / img.mainTexture.height * image.rectTransform.rect.height);
                }).ToList();
                polygon2d.SetPath(0, vertices.ToArray());

                mm.vertices = polygon2d.GetPath(0).ToList();
                if (mm.vertices[0] == mm.vertices[vertices.Count - 1])
                    mm.vertices.RemoveAt(mm.vertices.Count - 1); 

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Triangulator tr = new Triangulator(mm.vertices.ToArray());
                mm.triangles = tr.Triangulate().ToList();
                sw.Stop();
                Debug.Log("三角化耗时:" + sw.ElapsedMilliseconds + "ms");

                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Image组件还没设置图片");
            }
        }
        else
        {
            Debug.LogError("选中物体没有Image组件，请添加");
        }
#endif
    }

    /// <summary>
    /// 刷新Mesh数据
    /// </summary>
    [ContextMenu("根据Collider组件生成Mask")]
    private void CreateMeshMaskByPolygonCollider2D()
    {
#if UNITY_EDITOR
        vertices = polygon2d.GetPath(0).ToList();
        if(vertices[0] == vertices[vertices.Count-1])
            vertices.RemoveAt(vertices.Count - 1);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Triangulator tr = new Triangulator(vertices.ToArray());
        triangles = tr.Triangulate().ToList();
        sw.Stop();
        Debug.Log("三角化耗时:" + sw.ElapsedMilliseconds + "ms");
#endif
    }
    #endregion

}
