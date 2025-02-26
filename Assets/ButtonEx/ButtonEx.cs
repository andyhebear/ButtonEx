/*************************************************************************
 *  Copyright (C) 2025 -2099 URS. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  ButtonEx.cs
 *  Description  :  Null.
 *------------------------------------------------------------------------
 *  Author       :  SGD
 *  Version      :  1.0.0
 *  Date         :  2025/2/26
 *  Description  :  Initial development version.
 *************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
//using MyProject.UI;
using EventType = ButtonEx.EventType;
//namespace MyProject.UI
//{
/*
 * ButtonEx扩展按钮组件
 *
 * 【功能】
 *     - 多种按钮交互事件
 *     - Button无损转ButtonEx（Button右上角三个点设置里调用）
 */

/// <summary>
/// 扩展按钮组件
/// </summary>
[AddComponentMenu("UI/Utilities/Button/" + nameof(ButtonEx))]
public class ButtonEx : Button
{
    #region Transition
    public enum ExtdTransition
    {
        None=1,
        Scale=2
    }

    [Serializable]
    public class TransScale
    {
        [HideInInspector] public float originSize;
        [SerializeField, Tooltip("Button scale size")]
        public float size = 0.95f;
    }
    #endregion
    #region Extd Transition
    [SerializeField]
    public ExtdTransition extdTransition = ExtdTransition.Scale;
    [SerializeField]
    public TransScale transScale = new TransScale();
    protected void RunExtdTransition() {
        // Extended Transition
        switch (this.extdTransition) {
            case ExtdTransition.Scale:
                float size = this.transScale.originSize * this.transScale.size;
                this.transform.localScale = new Vector3(size, size, size);
                break;

            // None
            default:
                break;
        }
    }
    protected void ResetExtdTransition() {
        if (this.interactable) {
            switch (this.extdTransition) {
                case ExtdTransition.Scale:
                    this.transform.localScale = new Vector3(this.transScale.originSize, this.transScale.originSize, this.transScale.originSize);
                    break;

                // None
                default:
                    break;
            }
        }
    }
    #endregion
    /// <summary>
    /// 按钮交互事件枚举
    /// </summary>
    [System.Flags]
    public enum EventType
    {
        Click = 1 << 0,
        LongClick = 1 << 1,
        Down = 1 << 2,
        Up = 1 << 3,
        Enter = 1 << 4,
        Exit = 1 << 5,
        DoubleClick = 1 << 6,
    }
    [SerializeField] private EventType m_EventType = EventType.Click;

    /// <summary>
    /// 长按判定时间
    /// </summary>
    [SerializeField] private float onLongWaitTime = 1.5f;

    /// <summary>
    /// 是否重复抛出长按事件（false：长按onLongWaitTime后只触发一次onLongClick  true：从onDown起，每onLongWaitTime触发一次onLongClick）
    /// </summary>
    [SerializeField] private bool onLongContinue = false;

    /// <summary>
    /// 双击判定时间（两次OnDown的间隔时间小于此值即判定为一次双击，但完全不影响onClick的触发）
    /// </summary>
    [SerializeField] private float onDoubleClickTime = 0.5f;


    /*[SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent(); //点击事件*/

    [SerializeField] private ButtonClickedEvent m_OnLongClick = new ButtonClickedEvent(); //长按事件（触发一次）

    [SerializeField] private ButtonClickedEvent m_OnDown = new ButtonClickedEvent(); //按下事件

    [SerializeField] private ButtonClickedEvent m_OnUp = new ButtonClickedEvent(); //抬起事件

    [SerializeField] private ButtonClickedEvent m_OnEnter = new ButtonClickedEvent(); //进入事件

    [SerializeField] private ButtonClickedEvent m_OnExit = new ButtonClickedEvent(); //移出事件

    [SerializeField] private ButtonClickedEvent m_onDoubleClick = new ButtonClickedEvent(); //双击事件


    private Coroutine log;

    private bool isPointerDown = false;
    private bool isPointerInside = false;


    #region 对外属性

    /// <summary>
    /// 是否被按下
    /// </summary>
    public bool isDown {
        get { return isPointerDown; }
    }

    /// <summary>
    /// 是否进入
    /// </summary>
    public bool isEnter {
        get { return isPointerInside; }
    }

    /*/// <summary>
    /// 点击事件
    /// </summary>
    public ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }*/

    /// <summary>
    /// 长按事件
    /// </summary>
    public ButtonClickedEvent onLongClick {
        get { return m_OnLongClick; }
        set { m_OnLongClick = value; }
    }

    /// <summary>
    /// 双击事件
    /// </summary>
    public ButtonClickedEvent onDoubleClick {
        get { return m_onDoubleClick; }
        set { m_onDoubleClick = value; }
    }

    /// <summary>
    /// 按下事件
    /// </summary>
    public ButtonClickedEvent onDown {
        get { return m_OnDown; }
        set { m_OnDown = value; }
    }

    /// <summary>
    /// 松开事件
    /// </summary>
    public ButtonClickedEvent onUp {
        get { return m_OnUp; }
        set { m_OnUp = value; }
    }

    /// <summary>
    /// 进入事件
    /// </summary>
    public ButtonClickedEvent onEnter {
        get { return m_OnEnter; }
        set { m_OnEnter = value; }
    }

    /// <summary>
    /// 离开事件
    /// </summary>
    public ButtonClickedEvent onExit {
        get { return m_OnExit; }
        set { m_OnExit = value; }
    }

    #endregion


    private float lastClickTime;
    private void Down() {
        if (!IsActive() || !IsInteractable())
            return;
        this.RunExtdTransition();
        m_OnDown.Invoke();
        if (lastClickTime != 0 && Time.time - lastClickTime <= onDoubleClickTime) {
            DoubleClick();
        }

        lastClickTime = Time.time;
        log = StartCoroutine(grow());
    }

    private void Up() {
        if (!IsActive() || !IsInteractable() || !isDown)
            return;
        this.ResetExtdTransition();
        m_OnUp.Invoke();
        if (log != null) {
            StopCoroutine(log);
            log = null;
        }
    }

    private void Enter() {
        if (!IsActive())
            return;
        this.RunExtdTransition();
        m_OnEnter.Invoke();
    }

    private void Exit() {
        if (!IsActive() || !isEnter)
            return;
        this.ResetExtdTransition();
        m_OnExit.Invoke();
    }

    private void LongClick() {
        if (!IsActive() || !isDown)
            return;
        m_OnLongClick.Invoke();
    }

    private void DoubleClick() {
        if (!IsActive() || !isDown)
            return;
        m_onDoubleClick.Invoke();
    }

    private float downTime = 0f;
    private IEnumerator grow() {
        downTime = Time.time;
        while (isDown) {
            if (Time.time - downTime > onLongWaitTime) {
                LongClick();
                if (onLongContinue)
                    downTime = Time.time;
                else
                    break;
            }
            else
                yield return null;
        }
    }
    protected override void Awake() {
        this.transScale.originSize = this.transform.localScale.x;
    }
    protected override void OnDisable() {
        isPointerDown = false;
        isPointerInside = false;
        this.ResetExtdTransition();
        base.OnDisable();
    }
    protected override void OnDestroy() {       
        isPointerDown = false;
        isPointerInside = false;
        this.ResetExtdTransition();
        base.OnDestroy();
    }
    /*private void ExPress()
    {
        if (!IsActive() || !IsInteractable())
            return;

        UISystemProfilerApi.AddMarker("Button.onClick", this);
        onClick.Invoke();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        ExPress();
    }*/

    public override void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        isPointerDown = true;
        Down();
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Up();
        isPointerDown = false;
        base.OnPointerUp(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        isPointerInside = true;
        Enter();
    }

    public override void OnPointerExit(PointerEventData eventData) {
        Exit();
        isPointerInside = false;
        base.OnPointerExit(eventData);
    }

    #region Button->ButtonEx转换相关

#if UNITY_EDITOR

    [MenuItem("CONTEXT/Button/Convert To ButtonEx", true)]
    static bool _ConvertToButtonEx(MenuCommand command) {
        return CanConvertTo<ButtonEx>(command.context);
    }

    [MenuItem("CONTEXT/Button/Convert To ButtonEx", false)]
    static void ConvertToButtonEx(MenuCommand command) {
        ConvertTo<ButtonEx>(command.context);
    }

    [MenuItem("CONTEXT/Button/Convert To Button", true)]
    static bool _ConvertToButton(MenuCommand command) {
        return CanConvertTo<Button>(command.context);
    }

    [MenuItem("CONTEXT/Button/Convert To Button", false)]
    static void ConvertToButton(MenuCommand command) {
        ConvertTo<Button>(command.context);
    }

    protected static bool CanConvertTo<T>(Object context)
        where T : MonoBehaviour {
        return context && context.GetType() != typeof(T);
    }

    protected static void ConvertTo<T>(Object context) where T : MonoBehaviour {
        var target = context as MonoBehaviour;
        var so = new SerializedObject(target);
        so.Update();

        bool oldEnable = target.enabled;
        target.enabled = false;

        // Find MonoScript of the specified component.
        foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>()) {
            if (script.GetClass() != typeof(T))
                continue;

            // Set 'm_Script' to convert.
            so.FindProperty("m_Script").objectReferenceValue = script;
            so.ApplyModifiedProperties();
            break;
        }

        (so.targetObject as MonoBehaviour).enabled = oldEnable;
    }
#endif

    #endregion

}

#if UNITY_EDITOR
[CustomEditor(typeof(ButtonEx), true)]
public class ButtonExEditor : SelectableEditor
{
    /// <summary>
    /// 按钮交互事件绘制方法的字典
    /// </summary>
    Dictionary<EventType, Action> callbackDrawersDic;

    private SerializedProperty spType;
    private SerializedProperty spExtdTransition;
    //private SerializedProperty spTransScale;
    /// <summary>
    /// 按钮交互事件属性的字典
    /// </summary>
    Dictionary<EventType, SerializedProperty> eventPropertiesDic;

    private SerializedProperty sponLongWaitTime;

    private SerializedProperty sponLongContinue;

    private SerializedProperty sponDoubleClickTime;


    protected override void OnEnable() {
        base.OnEnable();

        //初始化各属性
        InitProperties();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.Update();
        // 获取当前编辑的脚本实例
        ButtonEx buttonT = (ButtonEx)target;
        GUILayout.Space(15);

        spExtdTransition.intValue=(int)(ButtonEx.ExtdTransition)EditorGUILayout.EnumFlagsField(new GUIContent("缩放动画"), (ButtonEx.ExtdTransition)spExtdTransition.intValue);
        // 绘制可序列化类中的 intValue 字段
        buttonT.transScale.size = EditorGUILayout.FloatField("缩放值", buttonT.transScale.size);

        // 绘制按钮相关参数
        sponLongWaitTime.floatValue = EditorGUILayout.FloatField("长按判定时间", sponLongWaitTime.floatValue);
        sponLongContinue.boolValue = EditorGUILayout.Toggle("长按期间是否持续触发", sponLongContinue.boolValue);
        sponDoubleClickTime.floatValue = EditorGUILayout.FloatField("双击间隔判定时间", sponDoubleClickTime.floatValue);

        // 绘制按钮交互事件栏
        GUILayout.Space(10);
        int oldType = spType.intValue;
        spType.intValue = (int)((EventType)EditorGUILayout.EnumFlagsField(new GUIContent("按钮交互事件"), (EventType)spType.intValue));
        foreach (EventType e in Enum.GetValues(typeof(EventType))) {
            if (0 < (spType.intValue & (int)e))
                callbackDrawersDic[e]();
            else if (0 < (oldType & (int)e))
                eventPropertiesDic[e].FindPropertyRelative("m_PersistentCalls").FindPropertyRelative("m_Calls").ClearArray();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void InitProperties() {
        eventPropertiesDic = new Dictionary<EventType, SerializedProperty>()
        {
                {EventType.Click, serializedObject.FindProperty("m_OnClick")},
                {EventType.LongClick, serializedObject.FindProperty("m_OnLongClick")},
                {EventType.Down, serializedObject.FindProperty("m_OnDown")},
                {EventType.Up, serializedObject.FindProperty("m_OnUp")},
                {EventType.Enter, serializedObject.FindProperty("m_OnEnter")},
                {EventType.Exit, serializedObject.FindProperty("m_OnExit")},
                {EventType.DoubleClick, serializedObject.FindProperty("m_onDoubleClick")},
            };

        callbackDrawersDic = new Dictionary<EventType, Action>();
        foreach (EventType eventType in Enum.GetValues(typeof(EventType))) {
            callbackDrawersDic.Add(eventType, () => {
                EditorGUILayout.LabelField(eventType.ToString(), EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(eventPropertiesDic[eventType]);

                //可以根据EventType再特殊定制其他属性的显示
            });
        }
        spExtdTransition = serializedObject.FindProperty("extdTransition");
        //spTransScale=serializedObject.FindProperty("transScale");
        spType = serializedObject.FindProperty("m_EventType");
        sponLongWaitTime = serializedObject.FindProperty("onLongWaitTime");
        sponLongContinue = serializedObject.FindProperty("onLongContinue");
        sponDoubleClickTime = serializedObject.FindProperty("onDoubleClickTime");
    }
}


public class Convert2ButtonExWindow : EditorWindow
{
    /*
    * MenuItem属性在编辑器上方创建了一个选项类
    * 默认的类有File, Edit, Assets, GameObject, Component, Window, Help七类
    */

    //window菜单下
    [MenuItem("Tool/Convert to ButtonEx")]
    private static void ShowWindow() {
        EditorWindow.GetWindow<Convert2ButtonPlusWindow>(true, "Tool/Convert Button to ButtonEx");
        style = new GUIStyle();
        style.normal.textColor = Color.red;
    }
    private static GUIStyle style;
    static GameObject gameObject;
    private void OnGUI() {
        GUILayout.Space(10);
        GUILayout.Label("目标Button:");
        /*
      *  EditorGUILayout.ObjectField生成了一个选项栏，允许给予特定的类型或资产
      *  其返回类型为Object，需要自己进行类型转换
      */
        GUILayout.Label("具体选中GameObject:");
        gameObject = null;
        if (Selection.objects != null && Selection.objects.Length > 0 && Selection.objects[0] is GameObject) {
            gameObject = (GameObject)EditorGUILayout.ObjectField(Selection.objects[0], typeof(GameObject), true, GUILayout.MinWidth(100f));
        }
        if (gameObject != null) {
            if (GUILayout.Button("确认转换button为buttonEx")) {
                Change();
            }
        }
        else {
            EditorGUILayout.LabelField("请选择包含Prefab文件夹路径", style);
        }
    }
    public static void Change() {
        //获取所有UILabel组件
        if (Selection.objects == null || Selection.objects.Length == 0) {
            Debug.LogError("请先选择对象");
            return;
        }
        Object[] labels = Selection.GetFiltered(typeof(Button), SelectionMode.Deep);
        if (labels == null) {
            Debug.LogError("没找到Button组件");
            return;
        }
        if (labels != null) {
            foreach (Button btn in labels) {
                Undo.RecordObject(btn, btn.gameObject.name);
                var clicks = btn.onClick;
                //转换                
                DestroyImmediate(btn, true);
                var tmp = gameObject.AddComponent<ButtonEx>();
                tmp.onClick = clicks;
                EditorUtility.SetDirty(btn);
            }
        }

        //         
        Debug.Log("替换完成!");
    }
}
#endif

//}
