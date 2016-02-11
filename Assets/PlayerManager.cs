using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {
	
	public Rect baseNameBoxRect = new Rect(5,5,150,25);
	public Rect baseMoneyBoxRect = new Rect(160,5,150,25);
	
	public Rect moveTimeRect;
	public Rect moveWaitRect;
	public Rect moveTimeLabelRect;
	public Rect moveWaitLabelRect;
	
	public float moveTime = 0.2f;
	public float waitTime = 0.5f;
	// Use this for initialization
	void Start () {
		moveTimeRect = new Rect(Screen.width-100,60,100,25);
		moveTimeLabelRect = new Rect(Screen.width-200,60,100,25);
		moveWaitRect = new Rect(Screen.width-100,90,100,25);
		moveWaitLabelRect = new Rect(Screen.width-200,90,100,25);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI () {
		Rect nameBoxRect = baseNameBoxRect;
		Rect moneyBoxRect = baseMoneyBoxRect;
		for(int i=0;i<GameController.singleton.playerCount;i++){
			nameBoxRect.y += 30;
			moneyBoxRect.y += 30;
			GUI.Box (nameBoxRect,GameController.singleton.players[i].playerName);
			GUI.Box (moneyBoxRect,GameController.singleton.players[i].money.ToString());
		}
		
		GUI.Box (new Rect(Screen.width-185,5,180,25),"Remaining Properties: "+GameController.remainingProperties.ToString());
		if(GUI.Button(new Rect(0,0,100,25),"Restart Game")){
			Application.LoadLevel(Application.loadedLevelName);
		}
		moveTime = GUI.HorizontalSlider(moveTimeRect,moveTime,0,1);
		waitTime = GUI.HorizontalSlider(moveWaitRect,waitTime,0,1);
		moveTime = Mathf.Round(moveTime*10)/10;
		waitTime = Mathf.Round(waitTime*10)/10;
		GUI.Label (moveTimeLabelRect,"Move Time: "+moveTime.ToString());
		GUI.Label (moveWaitLabelRect,"Wait Time: "+waitTime.ToString());
		
		GamePlayer.movePieceTime = moveTime;
		GamePlayer.moveWaitTime = waitTime;
	}
}
