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
		Branch.Delegate function;

        public override void Invoke()
        {
			base.Invoke();

			var id = Branch.FormatID(function);

			if (Script.Branches.TryGet(id, out var branch) == false)
				throw new Exception($"Invalid Branch with ID {id}, Can not Go To");

			Script.Continue(branch);
		}

        public GoToBranchNode(Branch.Delegate function)
        {
			this.function = function;
        }
	}

	partial class Script
	{
		protected GoToBranchNode GoTo(Branch.Delegate function)
        {
			var node = new GoToBranchNode(function);

			Register(node);

			return node;
		}
	}
}