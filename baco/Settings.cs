using System.Collections.Generic;
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
			XmlDocument doc = new XmlDocument();
			doc.Load(settings);
			XmlNode rootNode = doc.SelectSingleNode("baco");

			if (!Destination.Set)
			{
				XmlNode destinationNode = rootNode.SelectSingleNode("destination");
				if (destinationNode != null)
					Destination.Path = destinationNode.InnerText;
			}

			foreach (XmlNode reduceNode in rootNode.SelectNodes("reduce"))
			{
				XmlNode ageNode = reduceNode.SelectSingleNode("age");
				XmlNode spanNode = reduceNode.SelectSingleNode("span");
				reduces.Add(
					new Reduce(
						ageNode.InnerText,
						spanNode.InnerText)
				);
			}
			reduces.Sort();

			foreach (XmlNode sourceNode in rootNode.SelectNodes("source"))
			{
				XmlNode aliasNode = sourceNode.SelectSingleNode("alias");
				XmlNode includeNode = sourceNode.SelectSingleNode("include");
				XmlNode excludeNode = sourceNode.SelectSingleNode("exclude");
				sources.Add(
					new Source(
						aliasNode != null ? aliasNode.InnerText : null,
						sourceNode.SelectSingleNode("path").InnerText,
						includeNode != null ? includeNode.InnerText : null,
						excludeNode != null ? excludeNode.InnerText : null)
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

