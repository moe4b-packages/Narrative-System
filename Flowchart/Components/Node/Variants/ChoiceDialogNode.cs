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
	[AddComponentMenu(Path + "Choice Dialog")]
	public class ChoiceDialogNode : FlowNode
	{
		[SerializeField]
		Entry[] entires = default;
		public Entry[] Entries => entires;
		[Serializable]
		public class Entry : IChoiceData
        {
			[SerializeField]
			string text = default;
			public string Text => text;

			[SerializeField]
            Flowbranch branch = default;
            public Flowbranch Branch => branch;
        }

		public override void Invoke()
		{
			base.Invoke();

			Narrative.Controls.Choice.Show(entires, Submit);
		}

		public void Submit(int index, IChoiceData data)
		{
			var entry = entires[index];

			Chart.Continue(entry.Branch.Nodes.First);
		}
	}
}