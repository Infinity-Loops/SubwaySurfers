using UnityEngine;

public class UIPreBragPopupHelper : MonoBehaviour
{
	public UILabel description;

	private FriendHandlerBrag _bragHandler;

	private void OnEnable()
	{
		if (_bragHandler == null)
		{
			_bragHandler = FriendHandlerBrag.instance;
		}
		description.text = _bragHandler.preBragPopupString;
	}

	private void BragClicked()
	{
		UIScreenController.Instance.QueuePopup("BragPopup");
		UIScreenController.Instance.ClosePopup();
	}

	private void CloseClicked()
	{
		UIScreenController.Instance.ClosePopup();
	}
}
