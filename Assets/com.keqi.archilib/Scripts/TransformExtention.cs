using UnityEngine;
using UnityEngine.UI;

public static class TransformExtention {

    public static void SetLocalPositionX(this Transform transform, float x) {
        var pos = transform.localPosition;
        pos.x = x;
        transform.localPosition = pos;
    }
    public static void SetLocalPositionY(this Transform transform, float y) {
        var pos = transform.localPosition;
        pos.y = y;
        transform.localPosition = pos;
    }
    public static void SetLocalPositionZ(this Transform transform, float z) {
        var pos = transform.localPosition;
        pos.z = z;
        transform.localPosition = pos;
    }
    public static void SetPositionX(this Transform transform, float x){
        var pos=transform.position;
        pos.x=x;
        transform.position=pos;
    }
    public static void SetPositionY(this Transform transform, float y){
        var pos=transform.position;
        pos.y=y;
        transform.position=pos;
    }
    public static void SetPositionZ(this Transform transform, float z){
        var pos=transform.position;
        pos.z=z;
        transform.position=pos;
    }
    
}
public static class ImageExtension
{
    /// <summary>
    /// 改变图片的透明度
    /// </summary>
    /// <param name="image"></param>
    /// <param name="a">透明度，[0, 1]</param>
    public static void ChangeAlpha(this Image image, float alpha)
    {
        Color oldColor = image.color;
        image.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
    }
}
public static class GameobjectExtension
{

    /// <summary>
    /// 改变对象的层级
    /// </summary>
    /// <param name="gameobject">对象</param>
    /// <param name="layerName">层级名称</param>
    public static void ChangeLayer(this GameObject gameobject, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (gameobject)
        {
            foreach (Transform traform in gameobject.GetComponentsInChildren<Transform>(true))
            {
                traform.gameObject.layer = layer;
            }
        }
    }

    /// <summary>
    /// 改变对象的层级
    /// </summary>
    /// <param name="gameobject">对象</param>
    /// <param name="layer">层级编号</param>
    public static void ChangeLayer(this GameObject gameobject, int layer)
    {
        if (gameobject)
        {
            foreach (Transform traform in gameobject.GetComponentsInChildren<Transform>(true))
            {
                traform.gameObject.layer = layer;
            }
        }
    }

    /// <summary>
    /// 给对象加上元件
    /// 对象身上不存在这个元件
    /// 存在则不作处理
    /// </summary>
    /// <typeparam name="T">元件</typeparam>
    /// <param name="gameObject">对象</param>
    /// <returns></returns>
    public static T AddComponentIfNotFound<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();

        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    /// <summary>
    /// 移除物体上挂载的脚本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    public static void Remove<T>(this GameObject gameObject) where T : MonoBehaviour
    {
        if (gameObject.GetComponent<T>())
        {
            GameObject.Destroy(gameObject.GetComponent<T>());
        }
    }
}

