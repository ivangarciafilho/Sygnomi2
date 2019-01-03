using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Linq;
using NGS.PolygonalCulling;

public enum WindowState { StartWindow, KDTreeEditor, TypeSelectionWindow, ParametersSelectionWindow, BakeWindow }

public delegate void BakeHandler(Camera[] cameras, VisibilityManagerType type, int maxStack, int minTrianglesCount, bool autoCalculate, Vector3 center, Vector3 size, float minNodeSize, int accuracy);

public class PolygonalCullingEditorWindow : EditorWindow
{
    private const string _startWindowDescription = "Hello, \nMy name is Andrey, I am a creator of Polygonal Culling. \nThank you that have bought my asset.\nTo begin work press 'Let's start'.";
    private const string _kdTreeEditorDescription = "These are settings of division of objects for triangles. \n\nMax Stack - crushing depth. \n\nMin Triangles Count - quantity of triangles in the block. \n\nEach block is a part of object which it will be cut if it isn't visible. The more blocks - the more triangles will be cut off, but also the preprocessing will longer last. \n\nPress 'visualize' to see as objects were shared on blocks.";
    private const string _typeSelectionDescription = "These are the Visibility Manager settings.\n\nRealtime - the fastest preprocessing, but big loading in playmode.\n\nBaked - preprocessing share, but the fastest work in playmode.\n\nMixed - fast preprocessing, fast work in playmode.\n\n'Standard Occlusion' - to use Unity Occlusion Culling. After work with Polygonal Culling, you will choose Polygonal Culling GameObject, press 'Unhide Objects', then adjust and bake Occlusion Culling in 'Window/Occlusion Culling'. Now you will choose Polygonal Culling GameObject and press 'Hide objects'.\n\n'Cameras' - place here cameras to which you would like to apply culling.";
    private const string _parametrsDescription = "These are settings of a zone of relocation of the camera. Here you select that part of a scene where the camera can move. \n\nMin node size - the minimum size of the unit into which to be divided a scene.Every time when the camera gets to the new unit - will be updated by culling in a scene. \n\nAccuracy - the abstract value of accuracy of determination of invisible triangles.If you selected Visibility Manager Type: 'Mixed', you can use not big values. If you selected 'Baked', you need to use value more (15 - 25 for example)\n\nIf your camera can move not to all parts of a scene, then you can select this part, thereby having lowered time for 'Preprocessing'. For this purpose don't use 'auto calculate position' \n\nFor viewing of units, click 'Visualize'.";
    private const string _bakeDescription = "So, you have completely adjusted Polygonal Culling. Now press 'Bake' and have patience)";

    public event Action<WindowState> OnWindowChangedCallback;
    public event Action<int, int> OnVisualizeKDTreeCallback;
    public event Func<bool, Vector3, Vector3, float, int> OnVisualizeHierarchyCallback;
    public event Action<Vector3, Vector3> OnHierarchyParametrsChanged;
    public event BakeHandler OnBakeCallback;
    public event Action OnCloseCallback;

    public static readonly Vector2 stdWindowSize = new Vector2(512, 380);

    private WindowState _windowState;
    private Action[] _windows;

    [SerializeField] private int _maxStack = 15;
    [SerializeField] private int _minTrianglesCount = 500;
    [SerializeField] private Camera[] _cameras = new Camera[0];
    [SerializeField] private VisibilityManagerType _managerType = VisibilityManagerType.Mixed;
    [SerializeField] private float _minNodeSize = 5;
    [SerializeField] private int _accuracy = 10;
    [SerializeField] private int _callsCount = 0;
    [SerializeField] private bool _autoCalculate = true;
    [SerializeField] private Vector3 _hierarchyCenter = Vector3.zero;
    [SerializeField] private Vector3 _hierarchySize = Vector3.one * 10;


    private void OnEnable()
    {
        _windows = new Action[] { DrawStartWindow, DrawKDTreeEditor, DrawTypeSelectionWindow, DrawParametersSelectionWindow, DrawBakeWindow };
        
        OnWindowChangedCallback += (windowState) => _windowState = windowState;
        OnCloseCallback += () => { Close(); GC.Collect(); };
    }

    private void OnGUI()
    {
        _windows[(int)_windowState]();
    }


    private void DrawStartWindow()
    {
        GUI.Box(ScaleRect(new Rect(10, 10, 492, 300)), _startWindowDescription);

        if (GUI.Button(ScaleRect(new Rect(10, 320, 80, 30)), "Close"))
            OnCloseCallback();

        if (GUI.Button(ScaleRect(new Rect(422, 320, 80, 30)), "Let's start"))
            OnWindowChangedCallback(WindowState.KDTreeEditor);
    }

    private void DrawKDTreeEditor()
    {
        GUI.Box(ScaleRect(new Rect(10, 10, 492, 200)), _kdTreeEditorDescription);

        GUI.Label(ScaleRect(new Rect(10, 220, 80, 20)), "Max Stack : ");
        _maxStack = EditorGUI.IntField(ScaleRect(new Rect(90, 220, 50, 15)), _maxStack);
        _maxStack = _maxStack <= 0 ? 1 : _maxStack;

        GUI.Label(ScaleRect(new Rect(10, 240, 160, 20)), "Min Triangles Count : ");
        _minTrianglesCount = EditorGUI.IntField(ScaleRect(new Rect(150, 240, 50, 15)), _minTrianglesCount);
        _minTrianglesCount = _minTrianglesCount <= 0 ? 1 : _minTrianglesCount;

        if (GUI.Button(ScaleRect(new Rect(10, 270, 200, 20)), "Visualize"))
            OnVisualizeKDTreeCallback(_maxStack, _minTrianglesCount);

        if (GUI.Button(ScaleRect(new Rect(332, 320, 80, 30)), "Close"))
            OnCloseCallback();

        if (GUI.Button(ScaleRect(new Rect(422, 320, 80, 30)), "Next"))
            OnWindowChangedCallback(WindowState.TypeSelectionWindow);
    }

    private void DrawTypeSelectionWindow()
    {
        GUI.Box(ScaleRect(new Rect(10, 10, 492, 200)), _typeSelectionDescription);

        GUI.Label(ScaleRect(new Rect(10, 220, 100, 20)), "Culling type : ");
        _managerType = (VisibilityManagerType)EditorGUI.EnumPopup(ScaleRect(new Rect(100, 220, 150, 20)), _managerType);

        if (_managerType != VisibilityManagerType.Standard_Occlusion)
        {
            #region Cameras foldout

            SerializedObject serializedObject = new SerializedObject(this as ScriptableObject);
            SerializedProperty property = serializedObject.FindProperty("_cameras");

            serializedObject.Update();

            EditorGUI.PropertyField(ScaleRect(new Rect(10, 240, 200, 200)), property, true);
            serializedObject.ApplyModifiedProperties();

            #endregion
        }

        if (GUI.Button(ScaleRect(new Rect(332, 320, 80, 30)), "Close"))
            OnCloseCallback();

        if (GUI.Button(ScaleRect(new Rect(422, 320, 80, 30)), "Next"))
        {
            if (_managerType == VisibilityManagerType.Standard_Occlusion)
            {
                OnWindowChangedCallback(WindowState.BakeWindow);
                return;
            }

            _cameras = _cameras.Where(c => c != null).ToArray();
            _cameras = _cameras.Distinct().ToArray();

            if (_cameras.Length == 0)
                Debug.Log("No cameras selected");

            else if (_managerType == VisibilityManagerType.Baked || _managerType == VisibilityManagerType.Mixed)
                OnWindowChangedCallback(WindowState.ParametersSelectionWindow);

            else
                OnWindowChangedCallback(WindowState.BakeWindow);
        }
    }

    private void DrawParametersSelectionWindow()
    {
        GUI.Box(ScaleRect(new Rect(10, 10, 492, 200)), _parametrsDescription);

        GUI.Label(ScaleRect(new Rect(10, 220, 100, 20)), "Min node size : ");
        _minNodeSize = EditorGUI.FloatField(ScaleRect(new Rect(95, 220, 50, 15)), _minNodeSize);

        GUI.Label(ScaleRect(new Rect(10, 240, 100, 20)), "Accuracy : ");
        _accuracy = EditorGUI.IntField(ScaleRect(new Rect(95, 240, 50, 15)), _accuracy);

        GUI.Label(ScaleRect(new Rect(10, 260, 135, 20)), "Auto calculate position : ");
        bool autoCalculate = EditorGUI.Toggle(ScaleRect(new Rect(145, 260, 10, 10)), _autoCalculate);

        if (!autoCalculate)
            if(autoCalculate != _autoCalculate)
                OnHierarchyParametrsChanged(_hierarchyCenter, _hierarchySize);

        _autoCalculate = autoCalculate;

        if (_autoCalculate)
        {
            if (GUI.Button(ScaleRect(new Rect(10, 280, 135, 20)), "Visualize"))
                _callsCount = OnVisualizeHierarchyCallback(_autoCalculate, _hierarchyCenter, _hierarchySize, _minNodeSize);
        }
        else
        {
            Vector3 center = EditorGUI.Vector3Field(ScaleRect(new Rect(10, 280, 180, 10)), "Center : ", _hierarchyCenter);
            Vector3 size = EditorGUI.Vector3Field(ScaleRect(new Rect(10, 310, 180, 10)), "Size : ", _hierarchySize);

            if (center != _hierarchyCenter || size != _hierarchySize)
            {
                _hierarchyCenter = center;
                _hierarchySize = size;
                OnHierarchyParametrsChanged(_hierarchyCenter, _hierarchySize);
            }

            if (GUI.Button(ScaleRect(new Rect(10, 350, 135, 20)), "Visualize"))
                _callsCount = OnVisualizeHierarchyCallback(_autoCalculate, _hierarchyCenter, _hierarchySize, _minNodeSize);
        }

        GUI.Label(ScaleRect(new Rect(170, 260, 135, 20)), "Calls count : " + _callsCount);

        if (GUI.Button(ScaleRect(new Rect(332, 320, 80, 30)), "Close"))
            OnCloseCallback();

        if (GUI.Button(ScaleRect(new Rect(422, 320, 80, 30)), "Next"))
            OnWindowChangedCallback(WindowState.BakeWindow);
    }

    private void DrawBakeWindow()
    {
        GUI.Box(ScaleRect(new Rect(10, 10, 492, 200)), _bakeDescription);

        if (GUI.Button(ScaleRect(new Rect(332, 320, 80, 30)), "Close"))
            OnCloseCallback();

        if (GUI.Button(ScaleRect(new Rect(422, 320, 80, 30)), "Bake"))
        {
            OnBakeCallback(_cameras, _managerType, _maxStack, _minTrianglesCount, _autoCalculate, _hierarchyCenter, _hierarchySize, _minNodeSize, _accuracy);
            OnCloseCallback();
        }
    }


    private Rect ScaleRect(Rect rect)
    {
        Vector2 scaledVector = new Vector2(
            position.width / stdWindowSize.x,
            position.height / stdWindowSize.y);

        Rect scaledRect = new Rect(
            rect.x * scaledVector.x,
            rect.y * scaledVector.y,
            rect.width * scaledVector.x,
            rect.height * scaledVector.y);

        return scaledRect;
    }
}
