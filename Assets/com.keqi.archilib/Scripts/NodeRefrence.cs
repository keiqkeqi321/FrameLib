#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace ArchiLib
{

    public class NodeRefrenceEditorWindow : EditorWindow
    {
        public GameObject selectObj;
        string objName;
        [MenuItem("GameObject/FindMonoRefrence", false, priority = -100)]
        private static void CopyGameObjectName()
        {


            GameObject[] selectedGameObjects = Selection.gameObjects;
            var window = GetWindow<NodeRefrenceEditorWindow>();

            //注入数据
            window.selectObj = selectedGameObjects[0];

            window.titleContent = new GUIContent("NodeRefrence");
            window.minSize = new Vector2(600, 400);
            window.Find();
            window.Show();
        }
        static string GetTransPath(GameObject obj)
        {
            string str = GetTransPath(obj.transform);
            str = str.Replace("Canvas (Environment)/", "");
            return str;
        }
        static string GetTransPath(Transform trans)
        {
            if (!trans.parent)
            {
                return trans.name;
            }

            return GetTransPath(trans.parent) + "/" + trans.name;
        }
        public void Find()
        {
            state = 0;
            refrenceNodes.Clear();
            Transform root = selectObj.transform;
            Transform parent = selectObj.transform;
            while (parent != null)
            {
                root = parent;
                parent = parent.parent;
            }

            Transform[] allTrans = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allTrans.Length; i++)
            {
                var item = allTrans[i];
                var components = item.GetComponents<Component>();
                for (int j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component is MonoBehaviour mono)
                    {
                        CheckAllObjHangUpFields(mono, mono);
                    }
                }
                if (state == 0) { state = -1; }
            }
            List<GameObject> referencedBy = new List<GameObject>();
            for (int i = 0; i < refrenceNodes.Count; i++)
            {
                var item = refrenceNodes[i].owner.gameObject;
                if (!referencedBy.Contains(item))
                {
                    referencedBy.Add(item);
                }
            }
            // if (referencedBy.Count != 0)
            // {
            //     Selection.objects = referencedBy.ToArray();
            // }
        }
        //检查单个挂载字段的路径是不是所选
        void Check(GameObject hangUpObj, MonoBehaviour ownerMono, string fieldInfoName)
        {
            if (hangUpObj != null)
            {
                string path = GetTransPath(hangUpObj.gameObject);
                if (path.ToLower().Equals(GetTransPath(selectObj).ToLower()) && selectObj == hangUpObj.gameObject)
                {
                    state = 1;
                    Debug.Log(objName + "节点被引用:" + ownerMono.name + "(" + ownerMono.GetType().Name + ") : " + fieldInfoName);
                    var node = new RefrenceNode()
                    {
                        owner = ownerMono,
                        fieldName = fieldInfoName
                    };
                    refrenceNodes.Add(node);
                }
            }
        }
        //获取每层类型的所有字段
        static List<FieldInfo> GetAllFileds(Type type)
        {
            var res = new List<FieldInfo>();
            while (type != null)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                fields.RemoveAll(f => (f.Name == "m_CachedPtr" || f.Name == "m_InstanceID" || f.Name == "m_UnityRuntimeErrorString"));
                for (int i = fields.Count - 1; i >= 0; i--)
                {
                    var field = fields[i];
                    if (res.FindIndex(item => item.Name == field.Name) != -1)
                    {
                        fields.RemoveAt(i);
                    }
                }
                res.AddRange(fields);
                type = type.BaseType;
            }
            return res;
        }
        //targetObj 需要检查所有字段的对象,递归参数,每次递归都不一样(第一遍是MonoBehaviour对象,之后是纯类对象)
        //ownerMono 最终挂载的 mono对象,一直不变
        //index 在数组中元素
        //fieldName MonoBehaviour上挂的字段名字
        void CheckAllObjHangUpFields(object targetObj, MonoBehaviour ownerMono, int index = -1, string monoHanedName = "")
        {

            var fields = GetAllFileds(targetObj.GetType());
            fields.ForEach((item) =>
            {
                if (item.GetValue(targetObj) != null)
                {
                    //检查数组字段
                    if (item.GetValue(targetObj).GetType().IsArray)
                    {
                        var arr = item.GetValue(targetObj) as Array;
                        if (item.GetValue(targetObj).GetType().GetElementType().IsSubclassOf(typeof(Component))
                        || item.GetValue(targetObj).GetType().GetElementType().Equals(typeof(Component))
                        )
                        {
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var arrItem = arr.GetValue(i);
                                if (arrItem != null && (arrItem as Component) != null)
                                {
                                    Check((arrItem as Component).gameObject, ownerMono, GetNameShow(i, item.Name, monoHanedName));
                                }
                            }
                        }
                        else if (item.GetValue(targetObj).GetType().GetElementType().IsSubclassOf(typeof(GameObject))
                        || item.GetValue(targetObj).GetType().GetElementType().Equals(typeof(GameObject))
                        )
                        {
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var arrItem = arr.GetValue(i);
                                if (arrItem != null && (arrItem as GameObject) != null)
                                {
                                    Check(arrItem as GameObject, ownerMono, GetNameShow(i, item.Name, monoHanedName));
                                }
                            }
                        }
                        else if (item.GetValue(targetObj).GetType().GetElementType().IsSerializable)
                        {
                            //检查序列化字段
                            //检查序列化字段
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var arrItem = arr.GetValue(i);
                                if (arrItem != null && ownerMono is MonoBehaviour)
                                {
                                    CheckAllObjHangUpFields(arrItem, ownerMono, i, item.Name);
                                }
                            }
                        }
                    }
                    //检查单泛型列表字段
                    else if (item.GetValue(targetObj) is IList)
                    {
                        var arr = item.GetValue(targetObj) as IList;
                        if (arr.GetType().GetGenericArguments().Length > 0)
                        {
                            Type elementType = arr.GetType().GetGenericArguments()[0];
                            if (elementType.IsSubclassOf(typeof(Component))
                            || elementType.Equals(typeof(Component))
                            )
                            {
                                for (int i = 0; i < (arr as ICollection).Count; i++)
                                {
                                    var arrItem = arr[i];
                                    if (arrItem != null && (arrItem as Component) != null && (arrItem as Component).gameObject != null)
                                    {
                                        Check((arrItem as Component).gameObject, ownerMono, GetNameShow(i, item.Name, monoHanedName));
                                    }
                                }
                            }
                            else if ((elementType.IsSubclassOf(typeof(GameObject)))
                            || (elementType.Equals(typeof(GameObject)))
                            )
                            {
                                for (int i = 0; i < (arr as ICollection).Count; i++)
                                {
                                    var arrItem = arr[i];
                                    if (arrItem != null && (arrItem as GameObject) != null)
                                    {
                                        Check(arrItem as GameObject, ownerMono, GetNameShow(i, item.Name, monoHanedName));
                                    }
                                }
                            }
                            else if (elementType.IsSerializable)
                            {
                                //检查序列化字段
                                for (int i = 0; i < (arr as ICollection).Count; i++)
                                {
                                    var arrItem = arr[i];
                                    if (arrItem != null && ownerMono is MonoBehaviour)
                                    {
                                        CheckAllObjHangUpFields(arrItem, ownerMono, i, item.Name);
                                    }
                                }
                            }
                        }
                    }
                    else if (item.GetValue(targetObj) is Component compoent)
                    {
                        if (compoent != null && compoent.gameObject != null)
                        {
                            Check(compoent.gameObject, ownerMono, GetNameShow(index, item.Name, monoHanedName));
                        }
                    }
                    else if (item.GetValue(targetObj) is GameObject obj)
                    {
                        if (obj != null)
                        {

                            Check(obj, ownerMono, GetNameShow(index, item.Name, monoHanedName));
                        }
                    }
                    //纯类序列化
                    else if (HasSerializeField(ownerMono, item.Name) && ownerMono is MonoBehaviour)
                    {
                        CheckAllObjHangUpFields(item.GetValue(targetObj), ownerMono, -1, item.Name);
                    }
                }
            });
        }
        string GetNameShow(int index, string fieldName, string monoHangedname)
        {
            if (index != -1)
            {
                if (string.IsNullOrEmpty(monoHangedname))
                {
                    return "array " + fieldName + "[" + index + "]";
                }
                else
                {
                    return "array " + monoHangedname + "[" + index + "]->" + fieldName;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(monoHangedname))
                {
                    return fieldName;
                }
                else
                {
                    return monoHangedname + "->" + fieldName;
                }
            }
            return monoHangedname;
        }
        bool HasSerializeField(MonoBehaviour mono, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(mono);
            SerializedProperty myFieldProperty = serializedObject.FindProperty(propertyName);
            if (myFieldProperty != null)
            {
                // 检查字段是否具有 [SerializeField] 特性
                bool hasSerializeFieldAttribute = myFieldProperty.hasVisibleChildren;
                return hasSerializeFieldAttribute;
            }
            return false;

        }

        public struct RefrenceNode
        {
            public MonoBehaviour owner;
            public string fieldName;
        }
        List<RefrenceNode> refrenceNodes = new List<RefrenceNode>();
        int state = 0;
        private void OnGUI()
        {
            if (refrenceNodes == null || selectObj == null)
            {
                GUILayout.Label("该会话已经失效!");
                return;
            }
            EditorGUILayout.BeginVertical();
            if (refrenceNodes != null)
            {
                GUIStyle gUIStyle = new GUIStyle();
                int height = 16;
                if (state == -1)
                {
                    gUIStyle.normal.textColor = Color.red;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel("fail,该物体没被场景中组件挂载或获取(脚本中有字段那种)!", gUIStyle);
                    GUILayout.EndHorizontal();
                }
                else if (state == 1)
                {
                    gUIStyle.normal.textColor = Color.green;
                    GUILayout.BeginHorizontal();
                    var objName = GetTransPath(selectObj);
                    EditorGUILayout.SelectableLabel(objName + "节点的被引用列表:", gUIStyle, GUILayout.Height(height));
                    GUILayout.EndHorizontal();
                    for (int i = 0; i < refrenceNodes.Count; i++)
                    {
                        var item22 = refrenceNodes[i];
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.SelectableLabel(item22.owner.gameObject.name + "(" + item22.owner.GetType().Name + ") : " + item22.fieldName, gUIStyle, GUILayout.Height(height));
                        if (GUILayout.Button(new GUIContent("go")
                        // , GUILayout.Width(60)
                        , GUILayout.MinWidth(60)
                            ))
                        {
                            EditorGUIUtility.PingObject(refrenceNodes[i].owner);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }
}
#endif