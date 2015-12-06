using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.Assertions;
//<summary>
//Detects if any publicly accessible fields haven't been set via inspector, and sets them automatically if they have the [AutoLink] attribute.
//</summary> <remarks>
//Currently requires [System.Serializable] classes to have a default constructor and extended from Object to detect if they are empty.
//You can use the [CanBeDefaultOrNull] attribute if you want this script to ignore a field.
//Currently buggy in that it will also detect non-inspector-serializable classes (i.e. dictionary), but for 99.99% the 'proper' style would be to make those properties.
//There is AutoLink functionality; for example, [AutoLink(parentTag="Player")] would set the field to the nearest appropriate component on a player-tagged gameobject.
//</remarks>
[ExecuteInEditMode]
public class AutoLink : MonoBehaviour {
#if UNITY_EDITOR
    /*
    //tests
    public GameObject testPublic;

    [CanBeDefaultOrNull]
    public GameObject testPublicAttribute;

    [SerializeField]
    private GameObject testSerializeField;

    [HideInInspector]
    public GameObject testHideInInspector;

    private GameObject testPrivate;

    public int testPublicInt;

    [CanBeDefaultOrNull]
    public int testPublicIntAttribute;

    [SerializeField]
    private int testSerializeFieldInt;

    [HideInInspector]
    public int testHideInInspectorInt;

    private int testPrivateInt;

    public string testPublicString;

    [CanBeDefaultOrNull]
    public string testPublicStringAttribute;

    [SerializeField]
    private string testSerializeFieldString;

    [HideInInspector]
    public string testHideInInspectorString;

    private string testPrivateString;

    public string[] testPublicStringArray;

    [CanBeDefaultOrNull]
    public string[] testPublicStringArrayAttribute;

    [SerializeField]
    private string[] testSerializeFieldStringArray;

    [HideInInspector]
    public string[] testHideInInspectorStringArray;

    private string[] testPrivateStringArray;

    public List<int> testPublicIntList;

    [CanBeDefaultOrNull]
    public List<int> testPublicIntListAttribute;

    [SerializeField]
    private List<int> testSerializeFieldIntList;

    [HideInInspector]
    public List<int> testHideInInspectorIntList;

    private List<int> testPrivateIntList;

    public Transform testPublicTransform;

    [CanBeDefaultOrNull]
    public Transform testPublicAttributeTransform;

    [SerializeField]
    private Transform testSerializeFieldTransform;

    [HideInInspector]
    public Transform testHideInInspectoTransform;

    private Transform testPrivateTransform;

    [System.Serializable]
    public class TestSerializedClass { public int test; }

    public TestSerializedClass testPublicSerialized;

    [CanBeDefaultOrNull]
    public TestSerializedClass testPublicAttributeSerialized;

    [SerializeField]
    private TestSerializedClass testSerializeFieldSerialized;

    [HideInInspector]
    public TestSerializedClass testHideInInspectorSerialized;

    private TestSerializedClass testPrivateSerialized;
    
    public Vector3 testPublicVector3;

    [CanBeDefaultOrNull]
    public Vector3 testPublicAttributeVector3;

    [SerializeField]
    private Vector3 testSerializeFieldVector3;

    [HideInInspector]
    public Vector3 testHideInInspectorVector3;

    private Vector3 testPrivateVector3;

    [AutoLink]
    public Transform testPublicTransformAutoLink;

    [AutoLink]
    [CanBeDefaultOrNull]
    public Transform testPublicAttributeTransformAutoLink;

    [AutoLink]
    [SerializeField]
    private Transform testSerializeFieldTransformAutoLink;

    [AutoLink(parentName="EnableToCheckEditorLinks")]
    public Transform testPublicTransformAutoLinkByName;

    [AutoLink(parentTag=Tags.untagged, parentName="EnableToCheckEditorLinks")]
    public Transform testPublicTransformAutoLinkByNameAndTag;
     * */
    ///<summary>Called when inspector is loaded.</summary>
    void Awake()
    {
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            Destroy(this.gameObject); //don't run in play mode
    }

    //<summary>Called when anything is changed in the inspector.</summary>
	void Update () {
        CheckAllLinks();
	}
    /// <summary>
    /// Uses Reflection to check all inspector-accessible fields to determine that they have been linked.
    /// </summary>
    void CheckAllLinks()
    {
        foreach (MonoBehaviour m in GameObject.FindObjectsOfType(typeof(MonoBehaviour))) //for every monobehaviour
        {
            if (m is UnityEngine.EventSystems.UIBehaviour)
                continue; //ignore Unity defined classes

            var type = m.GetType();
            foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) //reflection
            {

                //check for excuses
                if (!System.Attribute.GetCustomAttributes(f).Any(a => a is UnityEngine.HideInInspector))
                {
                    //autolink
                    RunAutolinking(f, m);

                    if (isNullOrEmpty(f, m))
                    {
                        if (!System.Attribute.GetCustomAttributes(f).Any(a => a is CanBeDefaultOrNullAttribute))
                            Debug.LogWarning(type + "." + f.Name + " (" + m.gameObject.name + ") has not been set", m.gameObject);
                    }
                }
            }
            foreach (FieldInfo f in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                //check for excuses/premission
                if (System.Attribute.GetCustomAttributes(f).Any(a => a is UnityEngine.SerializeField)
                    && !System.Attribute.GetCustomAttributes(f).Any(a => a is UnityEngine.HideInInspector))
                {
                    //autolink
                    RunAutolinking(f, m);

                    if (isNullOrEmpty(f, m) && !System.Attribute.GetCustomAttributes(f).Any(a => a is CanBeDefaultOrNullAttribute))
                        Debug.LogWarning(type + "." + f.Name + " (" + m.gameObject.name + ") has not been set", m.gameObject);
                }
            }
        }
    }
    /// <summary>
    /// Links a inspector-accessible field to the appropriate value, if possible.
    /// </summary>
    /// <param name="field">The field to set.</param>
    /// <param name="script">The script on which to set the field.</param>
    void RunAutolinking(FieldInfo field, MonoBehaviour script)
    {
        foreach (Attribute a in System.Attribute.GetCustomAttributes(field))
        {
            if (a is AutoLinkAttribute)
            {
                //do autolinking

                AutoLinkAttribute autolinkInfo = (AutoLinkAttribute)a;

                //check if autolinking is valid first
                if(checkExistingLinkValid(field, script, autolinkInfo))
                    return;

                Transform[] parentNodes;

                if (!String.IsNullOrEmpty(autolinkInfo.parentTag))
                {
                    //grab by tag first
                    if (autolinkInfo.parentTag != Tags.untagged)
                    {
                        parentNodes = Array.ConvertAll<GameObject, Transform>(GameObject.FindGameObjectsWithTag(autolinkInfo.parentTag), o => o.transform);
                    }
                    else
                    {
                        //we can't find untagged objects by tag, but we can compare to check if an object is untagged
                        parentNodes = (FindObjectsOfType(typeof(Transform)) as Transform[]).Where(t => t.CompareTag(autolinkInfo.parentTag)).ToArray();
                    }
                    if (!String.IsNullOrEmpty(autolinkInfo.parentName))
                    {
                        //filter by name
                        parentNodes = parentNodes.Where(t => t.name == autolinkInfo.parentName).ToArray();
                    }
                    if (!String.IsNullOrEmpty(autolinkInfo.childPath))
                    {
                        //map to children
                        parentNodes = Array.ConvertAll<Transform, Transform>(parentNodes, t => t.Find(autolinkInfo.childPath));
                        parentNodes = parentNodes.Where(t => t != null).ToArray(); //remove null values
                    }
                }
                else
                {
                    //check that the other two tags aren't empty first
                    if (String.IsNullOrEmpty(autolinkInfo.parentName))
                    {
                        if (String.IsNullOrEmpty(autolinkInfo.childPath))
                        {
                            //if every tag is empty, then we'll limit the search to the local transform
                            Component possibleMatch = script.GetComponentInChildren(field.FieldType);
                            if (possibleMatch != null)
                            {
                                field.SetValue(script, possibleMatch);
                                UnityEditor.EditorUtility.SetDirty(script);
                                return;
                            }
                            else
                                continue;
                            
                        }
                        else
                        {
                            //if only the path is given, use the script's parent as the parent node
                            Transform parent = script.transform.Find(autolinkInfo.childPath);
                            if(parent != null)
                            {
                                Type type = field.FieldType;
                                UnityEngine.Object possibleMatch;
                                if(type == typeof(Transform))
                                    possibleMatch = parent;
                                else if(type == typeof(GameObject))
                                    possibleMatch = parent.gameObject;
                                else
                                    possibleMatch = parent.GetComponent(type);

                                if (possibleMatch != null)
                                {
                                    field.SetValue(script, possibleMatch);
                                    UnityEditor.EditorUtility.SetDirty(script);
                                    return;
                                }
                            }
                            continue;
                        }
                    }
                    else
                    {
                        //start with the universal set of all Transforms; there is no way to get the array of matching names, then filter by name
                        parentNodes = (FindObjectsOfType(typeof(Transform)) as Transform[]).Where(t => t.name == autolinkInfo.parentName).ToArray();
                        if (!String.IsNullOrEmpty(autolinkInfo.childPath))
                        {
                            //map to children
                            parentNodes = Array.ConvertAll<Transform, Transform>(parentNodes, t => t.Find(autolinkInfo.childPath));
                            parentNodes = parentNodes.Where(t => t != null).ToArray(); //remove null values
                        }

                    }
                }
                //use the parent nodes to try to find
                foreach (Transform t in parentNodes)
                {
                    Type type = field.FieldType;
                    UnityEngine.Object possibleMatch;
                    if (type == typeof(Transform))
                        possibleMatch = t;
                    else if (type == typeof(GameObject))
                        possibleMatch = t.gameObject;
                    else
                        possibleMatch = t.GetComponent(type);
                    if (possibleMatch != null)
                    {
                        field.SetValue(script, possibleMatch);
                        UnityEditor.EditorUtility.SetDirty(script);
                        return;
                    }
                }
            }
        }
        //if nothing gets autolinked, then the checkNull/Empty functionality will catch it
    }
    /// <summary>
    /// Determines if an autolinked attribute has a valid link.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <param name="script">The script on which to check the field.</param>
    /// <param name="autolinkInfo">The data included in the field's [AutoLink].</param>
    /// <returns><c>True</c> if the link is valid; otherwise, <c>False</c>.</returns>
    bool checkExistingLinkValid(FieldInfo field, MonoBehaviour script, AutoLinkAttribute autolinkInfo)
    {
        var value = field.GetValue(script);
        if (value == null || value.Equals(null)) //GameObjects being null aren't really null, so need to use equals as well
            return false;

        Assert.IsTrue(value is Component || value is GameObject, "AutoLink can only link components");

        //if the value could be valid, we have to find the link between it and the parent

        if (String.IsNullOrEmpty(autolinkInfo.childPath))
        {
            //check if all three fields are empty first
            if (String.IsNullOrEmpty(autolinkInfo.parentTag) && String.IsNullOrEmpty(autolinkInfo.parentName))
            {
                //if so, then validation is trivial
                return value.Equals(script.GetComponentInChildren(value.GetType()));
            }

            //if not we're at least dealing only with the local parent
            Transform valueTransfrom;
            Component testComponent = value as Component;
            if (testComponent != null)
                valueTransfrom = testComponent.transform;
            else
                valueTransfrom = ((GameObject)value).transform;
            if (!String.IsNullOrEmpty(autolinkInfo.parentName) && valueTransfrom.name != autolinkInfo.parentName)
                return false; //check parent name is the same
            if (!String.IsNullOrEmpty(autolinkInfo.parentTag) && !valueTransfrom.CompareTag(autolinkInfo.parentTag))
                return false; //check parent tag is the same
            return true;
        }
        else
        {
            // else we need to traverse up the tree to find the parent
            //obtain transform
            Transform currentParent;
            Component testComponent = value as Component;
            if (testComponent != null)
                currentParent = testComponent.transform;
            else
                currentParent = ((GameObject)value).transform;
            String[] parentNames = autolinkInfo.childPath.Split(new char[] {'/'});
            for (int i = parentNames.Length - 1; i >= 0; i--) //iterate backwards, since it's a path down the hierarchy tree
            {
                if (parentNames[i] != currentParent.name)
                    return false;
                currentParent = currentParent.parent;
            }

            //current parent should now be the parent node of the tree, if it is correct

            if (!String.IsNullOrEmpty(autolinkInfo.parentName))
            {
                if (!String.IsNullOrEmpty(autolinkInfo.parentTag) && !currentParent.CompareTag(autolinkInfo.parentTag))
                    return false;
                return currentParent.name == autolinkInfo.parentName;
            }
            else if (!String.IsNullOrEmpty(autolinkInfo.parentTag))
            {
                return currentParent.CompareTag(autolinkInfo.parentTag);
            }
            else
            {
                //if only the path is given, parent node should be the same as script's parent
                return currentParent.Equals(script.transform);
            }
        }
    }
    /// <summary>
    /// Determines if a field is null or empty.
    /// </summary>
    /// <param name="f">The field to check.</param>
    /// <param name="m">The script on which to check the field.</param>
    /// <returns><c>True</c> if the field is null or empty; otherwise, <c>False</c>.</returns>
    bool isNullOrEmpty(FieldInfo f, MonoBehaviour m)
    {
        var value = f.GetValue(m);
        if(value == null || value.Equals(null) ) //GameObjects being null aren't really null, so need to use equals as well
            return true;
        if (value.Equals(String.Empty))
        {
            //f.SetValue(m, "DING!");
            return true;
        }
        var type = value.GetType();
        if (type.IsArray && ((Array)value).Length == 0)
            return true;
        if (value is ICollection && ((ICollection)value).Count == 0)
            return true;
        
        if (!(value is MonoBehaviour) && !(value is GameObject))
        {
            var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor != null)
            {
                var defaultConstructorValue = defaultConstructor.Invoke(new System.Object[0]);
                if (value == defaultConstructorValue || value.Equals(defaultConstructorValue))
                    return true;
            }
        }
        if(type.IsValueType && !type.IsEnum)
        {
            var valueInstance = Activator.CreateInstance(type);
            if (value == valueInstance || value.Equals(valueInstance))
            return true;
        }
        return false;
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Marks the field to be ignored by AutoLink's null or empty checking.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class CanBeDefaultOrNullAttribute : Attribute
{}

/// <summary>
/// Marks the field to be linked automatically.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class AutoLinkAttribute : PropertyAttribute
{
    /// <summary>
    /// Name of the link target's parent object
    /// </summary>
    public string parentName;
    /// <summary>
    /// Tag of the link target's parent object
    /// </summary>
    public string parentTag;
    /// <summary>
    /// Path between the parent object set from <see cref="parentName"/> and <see cref="parentTag"/> to the link target's direct parent
    /// </summary>
    public string childPath;
    /// <summary>
    /// Default Constructor.
    /// </summary>
    public AutoLinkAttribute() //default constructor
    {
        //set default values for the optional fields
        parentName = parentTag = childPath = string.Empty;
    }
}
/// <summary>
/// Custom PropertyDrawer for AutoLinked fields.
/// </summary>
[CustomPropertyDrawer(typeof(AutoLinkAttribute))]
public class AutoLinkDrawer : PropertyDrawer
{
    // Override the GUI drawing for this attribute
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(position, property, new GUIContent(property.name + " [AutoLink]"));

        EditorGUI.EndProperty();
    }
}

#endif