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
	public class GoToBranchNode : Node
	{
		public Branch.Delegate Function { get; protected set; }

		public override void Invoke()
		{
			base.Invoke();

			Script.Continue(Function);
		}

		public GoToBranchNode(Branch.Delegate function)
		{
			this.Function = function;
		}
	}

	partial class Script
	{
		protected GoToBranchNode GoTo(Branch.Delegate function) => new GoToBranchNode(function);
	}
}