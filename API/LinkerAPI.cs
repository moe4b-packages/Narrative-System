using UnityEngine;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

using System;
using System.Text;
using UnityEditor;
using System.Linq;
using System.IO;

namespace MB.NarrativeSystem
{
	partial class Narrative
	{
		[field: SerializeField]
		public LinkerAPI Linker { get; private set; }
		[Serializable]
		public class LinkerAPI
		{
			[field: SerializeField]
			public TextAsset File { get; private set; }

#if UNITY_EDITOR
			internal void Validate(Narrative narrative)
			{
				if (File == null)
				{
					var text = Retrieve();

					var Path = Toolbox.IO.GenerateRuntimePath("Narrative/link.xml");

					MUtility.IO.EnsureDirectory(Path);
					System.IO.File.WriteAllText(Path, text);

					AssetDatabase.ImportAsset(Path);

					File = AssetDatabase.LoadAssetAtPath<TextAsset>(Path);

					Runtime.Save(narrative);
				}
			}

			internal void Build()
			{
				var text = Retrieve();
				File.WriteText(text);
			}

			internal string Retrieve()
            {
				var builder = new StringBuilder();
				builder.Append("<linker>");
				builder.AppendLine();

				var all = TypeCache.GetTypesDerivedFrom<Script>().GroupBy((x) => x.Assembly);

				foreach (var group in all)
				{
					builder.Append('\t', 1);
					builder.Append("<assembly fullname=\"");
					builder.Append(group.Key.GetName().Name);
					builder.Append("\">");
					builder.AppendLine();

					foreach (var type in group)
					{
						builder.Append('\t', 2);
						builder.Append("<type fullname=\"");
						builder.Append(type.FullName);
						builder.Append("\" preserve=\"all\"/>");
						builder.AppendLine();
					}

					builder.Append('\t', 1);
					builder.Append("</assembly>");
					builder.AppendLine();
				}

				builder.Append("</linker>");

				return builder.ToString();
			}
#endif
		}
	}
}