using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	
	public static GameController singleton = null;
	
	public uint gameTurnNumber = 0;
	public uint playerCount;
	public uint tileCount = 40;
	public static uint remainingProperties = 0;
	
 	public GamePlayer[] players;
 	public List<BoardTile> propertyList;
 	public int[] numberOfEachPropertyGroup = new int[11];
	public bool gameOver = false;
	public bool waitingForEndOfTurn = false;
	
	
	public uint currentPlayerIndex = 0;
	public string currentPlayerName = "LOL";
	
	public BoardTile startTile;
	
	
	void Awake () {
		singleton =  this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	// Use this for initialization
	void Start () {
		// TODO change this
		BeginGame();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void BeginGame () {
		
		// TODO fill the player array first
		
		remainingProperties = 0;
		// fill the property array
		BoardTile currentTile = startTile.nextTile;
		
		while(currentTile != startTile){
			if(currentTile.tileType == TileType.property){
				// add the property to the list
				propertyList.Add (currentTile);
				// find the group index
				int groupIndex = (int)currentTile.tileGroup;
				numberOfEachPropertyGroup[groupIndex] +=1;
				// add one to the number of properties
				remainingProperties++;
			}
			currentTile = currentTile.nextTile;
		}
		
		// get player count
		playerCount = (uint)players.Length;
		// get turn order
		
		// TODO randomise the turn order
		
		// set the players start data
		for(uint i=0;i<playerCount;i++){
			
			players[i].currentTile = startTile;
			players[i].playerIndex = (int)i;
			players[i].money = 1500;
		}
		StartCoroutine(GameLoop());
	}
	
	public static DiceData RollDice () {
		// generate numbers for both dice
		int dice1 = Random.Range(1,7);
		int dice2 = Random.Range(1,7);
		DiceData diceRoll;
		
		// check if its a double
		if(dice1 == dice2){
			diceRoll = new DiceData(dice1+dice2,true);
		}
		else{
			diceRoll =  new DiceData(dice1+dice2,false);
		}
		Debug.Log (dice1+"+"+dice2);
		Debug.Log ("Rolled a "+diceRoll.number);
		Debug.Log ("isDouble?: "+diceRoll.isDouble);
		return diceRoll;
	}
	
	IEnumerator GameLoop () {
	
		// reset game
		gameOver = false;
		currentPlayerIndex = playerCount;
		
		// actual game loop
		while(gameOver == false){
			
			// TODO change this as it gets random
			// TODO remove this, it only slows down for testing
			if(remainingProperties == 0){
			}
			// TODO check for victory
			
			// move to the next player
			
			currentPlayerIndex++;
			if(currentPlayerIndex >= playerCount){
				currentPlayerIndex =0;
			}
			Debug.Log ("<<<<<<<<< Player : "+currentPlayerIndex+" >>>>>>>>>");
			
			// start their turn
			waitingForEndOfTurn = true;
			players[currentPlayerIndex].BeginTurn();
			
			// wait until that turn is over
			while(waitingForEndOfTurn == true){
				yield return new WaitForEndOfFrame();
			}
			
			yield return new WaitForEndOfFrame();
		}
		// TODO
		// END OF GAME
		
		// end of enumerator
		yield return null;
	}
	
	public void AuctionProperty (BoardTile tile, GamePlayer hostPlayer){
		// TODO actually do an auction
		// notify the player that the auction is complete
		hostPlayer.DoneAuction();
	}
	
	public void AddMoneyToPlayer(int newMoney, int playerIndex){
		players[playerIndex].money += newMoney;
	}
	
	public void TurnFinished () {
	waitingForEndOfTurn = false;
	}
	
	public static GamePlayer[] GetAllPlayers () {
		return GameController.singleton.players;
	}
	
	public static BoardTile[] GetAllProperties () {
		return GameController.singleton.propertyList.ToArray();
	}
	
}
