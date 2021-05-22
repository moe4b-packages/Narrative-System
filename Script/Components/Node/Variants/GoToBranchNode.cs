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
		Branch.Delegate branch;

        public override void Invoke()
        {
			base.Invoke();

			Script.Continue(branch);
		}

        public GoToBranchNode(Branch.Delegate function)
        {
			this.branch = function;
        }
	}

	partial class Script
	{
		protected GoToBranchNode GoTo(Branch.Delegate function) => new GoToBranchNode(function);
	}
}