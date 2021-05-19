using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MB.NarrativeSystem
{
	[RequireComponent(typeof(Flowbranch))]
	public abstract class FlowNode : MonoBehaviour
	{
		public const string Path = Narrative.Path + "Nodes/";

		[SerializeField]
		ComponentInspectorUtility InspectorUtility;

		public Flowbranch Branch { get; protected set; }
		public int Index { get; protected set; }

		public Flowchart Chart => Branch.Chart;

		public FlowNode Previous
        {
			get
			{
				if (Chart.Nodes.Collection.TryGet(Index - 1, out var value) == false)
					return null;

				return value;
			}
		}
		public FlowNode Next
        {
			get
            {
				if (Chart.Nodes.Collection.TryGet(Index + 1, out var value) == false)
					return null;

				return value;
			}
        }

		internal void Set(Flowbranch branch, int index)
        {
			this.Branch = branch;
			this.Index = index;
		}

		public event Action OnInvoke;
		public virtual void Invoke()
		{
			OnInvoke?.Invoke();
		}
	}
}