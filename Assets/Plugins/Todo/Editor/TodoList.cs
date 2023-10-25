using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class TodoList : EditorWindow
{
	private static TodoList _window;
	private TaskDataList taskSO;
    private string _listDataDirectory = "/Plugins/Todo/";
	private string _listDataAssetPath = "Assets/Plugins/Todo/TodoList.asset";
	private int currentSelectLabelIndex = 0;
	private int newTaskLabelIndex = 0;
	private string newTask;
	private bool showCompletedTasks = true;
	private Vector2 scrollPosition = Vector2.zero;
	private GUIStyle taskTextStyle;
	private int displayCount = 0;
	private GUILayoutOption heightOption;
	[MenuItem ("Tool/Todo Window")]
    public static void Init ()
    {
        _window = ( TodoList )EditorWindow.GetWindow (typeof ( TodoList ));
		_window.titleContent = new GUIContent("ToDo");
		// 当场景发生变化，是否重新绘制
		_window.autoRepaintOnSceneChange = false;
    }
    
	public void OnGUI ()
	{
		// 加载或创建任务列表资源
		if (taskSO == null)
		{
			taskSO = AssetDatabase.LoadAssetAtPath( _listDataAssetPath, typeof(TaskDataList)) as TaskDataList;
			if(taskSO == null)
			{
				// 自动创建一个ToDo资源
				taskSO = ScriptableObject.CreateInstance(typeof(TaskDataList)) as TaskDataList;
                System.IO.Directory.CreateDirectory(Application.dataPath + _listDataDirectory);
				AssetDatabase.CreateAsset(taskSO, _listDataAssetPath );
				GUI.changed = true;				
			}						
		}

		// +1 是因为下拉列表项需要有一个All Tasks
		string[] labels = new string[taskSO.labels.Count + 1];
		string[] labelsToSelect = new string[taskSO.labels.Count];
		
		labels[0] = "All Tasks";
		for(int i = 0; i < taskSO.labels.Count; i++)
		{
			labels[i+1] = taskSO.labels[i].lableName;
			labelsToSelect[i] = taskSO.labels[i].lableName;
		}

		// 使用水平布局，后续元素将横向排列
		EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("小梦的待办列表:", EditorStyles.boldLabel);
		// 创建一个下拉列表，index代表当前索引，labels代表需要下拉的项
        currentSelectLabelIndex = EditorGUILayout.Popup(currentSelectLabelIndex, labels);
		newTaskLabelIndex = currentSelectLabelIndex - 1;
		EditorGUILayout.EndHorizontal();
		

		// 显示列表
		taskTextStyle = new GUIStyle(EditorStyles.miniBoldLabel); // EditorStyles.wordWrappedMiniLabel 是 Unity 编辑器提供的一个内置样式，通常用于显示小型标签文本，允许文本自动换行。
		taskTextStyle.alignment = TextAnchor.UpperLeft;
		// taskTextStyle.wordWrap = true;

		// 设置滚动视图
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		// 显示 未完成 Task项
		for( int i = 0; i < taskSO.tasks.Count; i++)
		{
			Task task = taskSO.tasks[i];
			TaskLabel taskLabel = GetLabel(task.lableIndex);

			//Debug.Log(currentSelectLabelIndex);
			if(currentSelectLabelIndex == 0 && task.isComplete == false)
			{	
				CreateUnDoneTaskItem(task, i);
			}
            else if(currentSelectLabelIndex > 0)
            {
                int adjustedIndex = currentSelectLabelIndex - 1;
                taskLabel = taskSO.labels[adjustedIndex];
                if (taskLabel.labelIndex == task.lableIndex && task.isComplete == false)
                {
                    CreateUnDoneTaskItem(task, i);
                }
            }
        }

        // 显示 已完成待办
        for (int i = 0; i < taskSO.tasks.Count; i++)
        {
            Task task = taskSO.tasks[i];
            TaskLabel taskLabel = GetLabel(task.lableIndex);

			if (currentSelectLabelIndex == 0 && task.isComplete)
            {
                CreateDoneTaskItem(task, Color.gray, i);
            }
			else if(currentSelectLabelIndex > 0)
			{
                int adjustedIndex = currentSelectLabelIndex - 1;
                taskLabel = taskSO.labels[adjustedIndex];
                if (taskLabel.labelIndex == task.lableIndex && task.isComplete)
                {
                    CreateDoneTaskItem(task, Color.gray, i);
                }
            }
        }

        if (displayCount == 0)
        {
			EditorGUILayout.LabelField("现在是摸鱼时间！~", EditorStyles.largeLabel);
        }
		EditorGUILayout.EndScrollView();

		// 创建任务
		//EditorGUILayout.BeginHorizontal();
		////EditorGUILayout.LabelField("Create Task:", EditorStyles.boldLabel);
		//EditorGUILayout.EndHorizontal();
		newTask = EditorGUILayout.TextField(newTask, GUILayout.Height(40));
		if( ( GUILayout.Button("创建新待办") && newTask != "" ) && newTaskLabelIndex  >= 0)
		{
			TaskLabel newOwner = taskSO.labels[newTaskLabelIndex];
			taskSO.AddTask(newTask, newOwner.labelIndex,GetDate());			
			newTask = "";
			GUI.FocusControl(null);				
		}
		if (GUI.changed)
		{
			EditorUtility.SetDirty(taskSO);
			AssetDatabase.SaveAssets();	
		}	
	}

	// 创建未完成待办显示项
	public void CreateUnDoneTaskItem(Task currTask,int forI)
    {
		// UpdateLable();
		taskTextStyle.normal.textColor = GetLabel(currTask.lableIndex).color;
		displayCount++;
		EditorGUILayout.BeginHorizontal();

		if (EditorGUILayout.Toggle(currTask.isComplete, GUILayout.Width(15), GUILayout.Height(28)) == true)
		{
			taskSO.tasks[forI].isComplete = true;
		}
		taskSO.tasks[forI].taskName = EditorGUILayout.TextField(currTask.taskName, taskTextStyle);

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
	}
	
	// 创建已完成待办显示项
	public void CreateDoneTaskItem(Task currTask,Color color,int forI)
    {
		taskTextStyle.normal.textColor = color;
		GUIStyle newStyle = taskTextStyle;
		newStyle.alignment = TextAnchor.MiddleLeft;

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(currTask.taskName, newStyle);
		if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18)))
		{
			taskSO.tasks.RemoveAt(forI);
		}
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
	}

	/// <summary>
	/// 得到当前任务对应的标签
	/// </summary>
	/// <returns></returns>
	public TaskLabel GetLabel(int index)
	{
		foreach (TaskLabel item in taskSO.labels)
		{
			if (item.labelIndex == index)
				return item;
		}
		return null;
	}

	/// <summary>
	/// 得到当前日期
	/// </summary>
	/// <returns></returns>
	public string GetDate()
    {
		DateTime currentDate = DateTime.Now; 
		string formattedDate = currentDate.ToString("yyyy-MM-dd");
		return formattedDate;
		//Debug.Log("Current Date: " + formattedDate);
	}

	void OnDestroy()
	{
		EditorUtility.SetDirty(taskSO);
		AssetDatabase.SaveAssets();
	}	
}
