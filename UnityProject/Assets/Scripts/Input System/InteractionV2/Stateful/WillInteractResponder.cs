
/// <summary>
/// Response class, used so that UnityEvent listeners can communicate
/// return a WillInteract response (true or false) to indicate if
/// if an interaction will happen.
/// </summary>
public class WillInteractResponder
{
	private bool willInteract;

	private WillInteractResponder(bool willInteract)
	{
		this.willInteract = willInteract;
	}

	/// <summary>
	/// Respond that interaction will not happen.
	/// </summary>
	public void RespondNo()
	{
		willInteract = false;
	}

	/// <summary>
	/// Gets a new willinteractresponder to be used to validate a particular
	/// interaction.
	/// </summary>
	/// <returns></returns>
	public static WillInteractResponder Create()
	{
		return new WillInteractResponder(true);
	}
}
