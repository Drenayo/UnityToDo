using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

// 待做：按照日期排序

public class TaskDataList : ScriptableObject
{
	/// <summary>
	/// 标签列表
	/// </summary>
	public List<TaskLabel> labels = new List<TaskLabel>();
	/// <summary>
	/// 任务列表
	/// </summary>
	public List<Task> tasks = new List<Task>();

	public ToDoSetting todoSetting;

	public TaskDataList ()
	{	
		labels.Add( new TaskLabel("常规任务",0) );		
		labels.Add( new TaskLabel("紧急任务", 1) );
		labels.Add( new TaskLabel("正在完成", 2) );
	}
	
	public void AddTask( string taskName, int lableIndex,string date)
	{
		Task task = new Task( taskName, lableIndex, date);
		tasks.Add(task);
	}
}

/// <summary>
/// 任务
/// </summary>
[Serializable]
public class Task
{
	/// <summary>
	/// 任务名
	/// </summary>
	public string taskName;
	/// <summary>
	/// 任务所属标签序号
	/// </summary>
	public int lableIndex;
	/// <summary>
	/// 任务是否完成
	/// </summary>
	public bool isComplete;
	/// <summary>
	/// 创建日期
	/// </summary>
	public string creationDate;
	
	public Task( string task,int lableIndex ,string date)
	{
		this.taskName = task;
		this.isComplete = false;
		this.lableIndex = lableIndex;
		creationDate = date;
	}


}

/// <summary>
/// 任务标签
/// </summary>
[Serializable]
public class TaskLabel
{
	/// <summary>
	/// 标签名
	/// </summary>
	public string lableName;
	/// <summary>
	/// 标签颜色
	/// </summary>
	public Color color;
	/// <summary>
	/// 标签序号
	/// </summary>
	public int labelIndex;
	
	public TaskLabel( string name, int index)
	{
		this.lableName = name;
		this.color = Color.white;
		this.labelIndex = index;
	}
}

[Serializable]
public class ToDoSetting
{
	/// <summary>
	/// 是否自动生成（当找不到TODO文件的时候，防止Bug,文件被覆盖）
	/// </summary>
	public bool isAutoCreate;

}
