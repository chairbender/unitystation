using UnityEngine;

namespace StatefulInteraction.HandApply
{
	/// <summary>
	/// Starts a particular kind of progress action and invokes more logic when complete.
	/// </summary>
	public class StartProgress : BaseServerHandApply
	{



		public override void ServerPerformInteraction(GameObject processorObject, global::HandApply interaction)
		{
			//TODO: Refactor ProgressAction config definitions to be SOs and use a singleton to share common ones so we can
			//reference it here.
			throw new System.NotImplementedException();
		}
	}
}