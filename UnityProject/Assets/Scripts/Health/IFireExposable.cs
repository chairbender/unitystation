
/// <summary>
/// Allows a component to be do something when in contact with fire. It's up to the component what
/// it does in response to this.
/// </summary>
public interface IFireExposable
{

	/// <summary>
	/// Expose this component to fire.
	/// </summary>
	/// <param name="exposure">information on the exposure</param>
	void OnExposed(FireExposure exposure);
}
