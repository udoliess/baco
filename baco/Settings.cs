using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace baco
{
	public class Settings
	{
		List<Source> sources = new List<Source>();
		List<Reduce> reduces = new List<Reduce>();

		/// <summary>
		/// Initializes a new instance of the Settings class.
		/// </summary>
		/// <param name='settings'>
		/// Settings.
		/// </param>
		public Settings(string settings)
		{
			var doc = new XmlDocument();
			doc.Load(settings);
			var rootNode = doc.SelectSingleNode("baco");

			if (!Destination.Set)
			{
				var destinationNode = rootNode.SelectSingleNode("destination");
				if (destinationNode != null)
					Destination.Path = destinationNode.InnerText;
			}

			foreach (XmlNode reduceNode in rootNode.SelectNodes("reduce"))
				reduces.Add(
					new Reduce(
						reduceNode.SelectSingleNode("age").InnerText,
						reduceNode.SelectSingleNode("span").InnerText)
				);
			reduces.Sort();

			foreach (XmlNode sourceNode in rootNode.SelectNodes("source"))
			{
				var aliasNode = sourceNode.SelectSingleNode("alias");
				var includeNode = sourceNode.SelectSingleNode("include");
				var excludeNode = sourceNode.SelectSingleNode("exclude");
				sources.Add(
					new Source(
						aliasNode != null ? aliasNode.InnerText : null,
						sourceNode.SelectSingleNode("path").InnerText,
						includeNode != null ? includeNode.InnerText : null,
						sourceNode.SelectNodes("take").Cast<XmlNode>().Select(node => node.InnerText),
						excludeNode != null ? excludeNode.InnerText : null,
						sourceNode.SelectNodes("ignore").Cast<XmlNode>().Select(node => node.InnerText))
				);
			}
		}

		/// <summary>
		/// Gets the sources.
		/// </summary>
		/// <value>
		/// The sources.
		/// </value>
		public IEnumerable<Source> Sources { get { return sources; } }

		/// <summary>
		/// Gets the sorted reduces.
		/// </summary>
		/// <value>
		/// The reduces.
		/// </value>
		public List<Reduce> Reduces { get { return reduces; } }
	}
}

