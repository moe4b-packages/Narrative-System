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
	public class SayDialogNode : FlowNode, ISayData
	{
		[SerializeField]
		Character character = default;
		public Character Character => character;

		[SerializeField]
		string text = default;
		public string Text => text;

		[SerializeField]
		bool autoSubmit = false;
        public bool AutoSubmit => autoSubmit;

        public override void Invoke()
		{
			base.Invoke();

			Narrative.Controls.Say.Show(this, Submit);
		}

		void Submit() => Chart.Continue();
	}
}