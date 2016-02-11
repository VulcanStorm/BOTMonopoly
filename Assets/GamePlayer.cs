using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePlayer : MonoBehaviour {
	
	
	
	public static float movePieceTime = 0f;
	public static float moveWaitTime = 0f;
	
	public List<BoardTile> ownedProperties;
	public int[] numberOfPropertiesInGroup = new int[10];
	public bool[] fullPropertySets = new bool[10];
	public int numOfStationsOwned = 0;
	public int numOfUtilitiesOwned = 0;
	
	// id data
	public string playerName = "lololol";
	public int playerIndex = -1;
	
	public Transform thisTransform;
	public Sprite myPlayerSprite;
	
	// movement variables
	public DiceData moveDiceScore;
	private bool movedToJail = false;
	private bool didBuyTile = false;
	private bool isMoving = false;
	private bool isProcessingTurnStart = false;
	private bool isProcessingUnOwnedPropertyTile = false;
	private bool isProcessingMoveAction = false;
	private bool isProcessingOwnedTile = false;
	private bool isProcessingTradeOffer = false;
	public bool pendingTradeOffer = false;
	private bool didAcceptTrade = false;
	private bool turnOver = false;
	// movement data
	private BoardTile mCurrentTile;
	public BoardTile currentTile {
		get{
			return mCurrentTile;
		}
		set{
			mCurrentTile = value;
			// TODO move the graphics
			
		}
	}
	
	// monetary variables
	private int _cashInHand = 0;
	public int money {
		get{
			return _cashInHand;
		}
		set{
			if(isBankrupt == false){
				// set our cash in hand
				_cashInHand = value;
				// check for bankruptcy
				if(_cashInHand < 0){
					isBankrupt = true;
				}
			}
			else{
			// TODO set cash in hand to zero
			_cashInHand = 0;
			}
			
		}
	}
	private bool isBankrupt = false;
	private int amountOfFundsPaid = 0;
	private bool hasPaidFunds = false;
	private bool hasPaidAllFunds = false;
	private bool auctionDone = false;
	private bool allPropertyRentPaid = false;
	
	// Use this for initialization
	void Start () {
		thisTransform = this.transform;
		movePieceTime = 0;
		moveWaitTime = 0;
	}
	
	void OnDestroy(){
	thisTransform = null;
	
	}
	
	public void BeginTurn () {
		if(isBankrupt == false){
			StartCoroutine(StartTurn ());
		}
		else{
			Debug.Log ("PlayerIsBankrupt");
			ForceEndTurn();
			
		}
	}
	
	IEnumerator StartTurn () {
		// do stuff at start of turn
		Debug.Log("Started Turn For Player: "+playerName);
		
		// reset all the turn variables from the previous turn
		// and recalculate information
		ResetTurnVariables();
		// for AI
		OnTurnHasStarted();
		// wait for the turn start to be processed
		while(isProcessingTurnStart == true){
			yield return new WaitForEndOfFrame();
		}
		// roll dice
		moveDiceScore = GameController.RollDice();
		
		// start moving
		int doubleCount = 0;
		// check for a double
		if(moveDiceScore.isDouble == true){
			doubleCount = 1;
		}
		movedToJail = false;
		
		isMoving = true;
		StartCoroutine(MoveSpaces(moveDiceScore.number));
		
		// wait for the movement to finish
		while(isMoving == true){
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(moveWaitTime);
		
		// keep rolling until double
		while(moveDiceScore.isDouble == true && movedToJail == false && isBankrupt == false){
		
			// reroll!
			moveDiceScore = GameController.RollDice();
		
			// move squares
			doubleCount ++;
			// did we move to jail?
			if(doubleCount > 2){
				movedToJail = true;
				Debug.Log ("JAILED!");
				GoToJail();
				// TODO move us to jail now.
			}
			// didnt move to jail
			else{
				// start the movement sequence
				isMoving = true;
				StartCoroutine(MoveSpaces(moveDiceScore.number));
				
				// wait for the movement to finish
				while(isMoving == true){
					yield return new WaitForEndOfFrame();
				}
			}
				yield return new WaitForSeconds(moveWaitTime);
		
		}
		
		StartCoroutine(EndOfTurn());
	}
	
	void ResetTurnVariables () {
		isProcessingTurnStart = false;
		isProcessingOwnedTile = false;
		isProcessingUnOwnedPropertyTile = false;
		isProcessingTradeOffer = false;
		didAcceptTrade = false;
		isMoving = false;
		movedToJail = false;
		didBuyTile = false;
		hasPaidAllFunds = false;
		amountOfFundsPaid = 0;
		auctionDone = false;
		allPropertyRentPaid = false;
		turnOver = false;
		pendingTradeOffer = false;
		
		// TODO change this at some point, it needs to be replaced
		UpdateOwnedProperties();
		
		
	}
	
	void UpdateOwnedProperties () {
		// recalculate stations and utilities
		// reset our count
		numOfStationsOwned = 0;
		numOfUtilitiesOwned = 0;
		
		// TODO possibly abstract this into a banker class
		// reset our counters for which properties we own
		for(int i=0;i<numberOfPropertiesInGroup.Length;i++){
			numberOfPropertiesInGroup[i] = 0;
			fullPropertySets[i] = false;
		}
		
		
		for(int i=0;i<ownedProperties.Count;i++){
			if(ownedProperties[i].tileGroup == TileGroup.station){
				numOfStationsOwned ++;
			}
			else if(ownedProperties[i].tileGroup == TileGroup.utility){
				numOfUtilitiesOwned ++;
			}
			// get the property index for that group
			int propertyIndex = (int)ownedProperties[i].tileGroup;
			
			// add 1 to the number of properties for that group
			numberOfPropertiesInGroup[propertyIndex] +=1;
		}
		
		
		// check if any of the properties form part of a full set
		for(int i=0;i<numberOfPropertiesInGroup.Length;i++){
			// do we have all the set?
			if(numberOfPropertiesInGroup[i] == GameController.singleton.numberOfEachPropertyGroup[i]){
				// we have this full set...
				fullPropertySets[i] = true;
			}
		}
		
		// update all our properties
		for(int i=0;i<ownedProperties.Count;i++){
			// get the property index for that group
			int propertyIndex = (int)ownedProperties[i].tileGroup;
			// check if this property is part of a full set
			if(fullPropertySets[propertyIndex] == true){
				// set the full set variable if this is the case
				ownedProperties[i].fullSet = true;
			}
			
			// set the rent cost
			ownedProperties[i].SetRentCost();
		}
		
	}
	
	IEnumerator MoveSpaces(int moveNumber){
		Debug.Log(playerName+": Moving");
		// we are now moving
		isMoving = true;
		// count number of tiles moved
		int tilesMoved = 0;
		// move number of tiles incrementally
		while(tilesMoved < moveNumber){
			tilesMoved++;
			currentTile = currentTile.nextTile;
			currentTile.OnTilePassed(this);
			yield return new WaitForSeconds(movePieceTime);
			thisTransform.position = currentTile.gamePiecePos.position;
			// TODO animate graphics moving here
		}
		// start processing our mvoe action
		isProcessingMoveAction = true;
		// determine what to do from the tile we landed on
		Debug.Log (playerName+": Landed on: "+currentTile.actualTileName);
		currentTile.OnTileLanded(this);
		
		
		
		while(isProcessingMoveAction == true){
			yield return new WaitForEndOfFrame();
		}
		
		Debug.Log (playerName+": Done Moving");
		// done movement
		isMoving = false;
		
	}
	
	IEnumerator PayRentForProperty (BoardTile propertyTile){
		// we haven't paid all the rent yet
		allPropertyRentPaid = false;
		// keep going until we have paid all the rent.
		Debug.Log ("Paying Funds...");
		StartCoroutine(PayFunds(propertyTile.currentRentCost));
		
		while(hasPaidAllFunds == false){
			yield return new WaitForEndOfFrame();
		}
		
		// check for bankruptcy
		if(isBankrupt == true){
			BankruptOnPlayer(propertyTile.owningPlayer);
		}
		// pay the other player
		GameController.singleton.AddMoneyToPlayer(amountOfFundsPaid, propertyTile.owner);
		// we have just paid all the rent
		allPropertyRentPaid = true;
		// end the function
		yield return 0;
	}
	
	IEnumerator PayFunds(int totalToPay){
		Debug.Log ("Got to pay funds: "+totalToPay);
		// reset the paying variables
		hasPaidAllFunds = false;
		amountOfFundsPaid = 0;
		hasPaidFunds = false;
		// calculate how much we need to pay
		int fundsToPay = totalToPay - amountOfFundsPaid;
		
		while(hasPaidAllFunds== false){
			// calculate the amount left to pay
			
			// do the paying function
			
			// bear in mind, you can over pay, so watch out bots...
			Debug.Log ("Total left to pay: "+fundsToPay);
			OnMustPayRent(fundsToPay);
			Debug.Log ("Waiting for funds to be paid");
			// wait for the funds to be paid
			while(hasPaidFunds == false){
				yield return new WaitForEndOfFrame();
			}
			hasPaidFunds = false;
			
			// recalculate the new funds to pay
			fundsToPay = totalToPay - amountOfFundsPaid;
			// check if we have paid it all
			if(fundsToPay <=0){
				hasPaidAllFunds = true;
				Debug.Log ("Paid all funds");
			}
		}
	}
	
	
	
	public void DoneMoveAction () {
		isProcessingMoveAction = false;
	}
	
	public void PayRent(int amount){
		
		// get the previous money for bankruptcy
		int lastMoney = money;
		// reduce our ready cash
		money -=amount;
		if(isBankrupt == true){
			// we have paid all the funds we can
			hasPaidAllFunds = true;
			// we can only pay what we had, so the amount is our last money
			amount = lastMoney;
			
		}
		// increase the funds paid
		amountOfFundsPaid += amount;
		
		
		// we have just paid in another funds packet
		hasPaidFunds = true;
	}
	
	IEnumerator EndOfTurn () {
		Debug.Log (playerName+": End Of Turn Started");
		turnOver = false;
		OnEndOfTurn();
		
		while(turnOver == false){
			Debug.Log (playerName+": Waiting For end of turn");
			yield return new WaitForEndOfFrame();
		}
		Debug.Log (playerName+": Turn Over");
		GameController.singleton.TurnFinished();
	}
	
	public IEnumerator ProcessUnownedPropertyTile(BoardTile tile){
		// woohoo unowned tile
		
		// we are now processing an unowned tile...
		isProcessingUnOwnedPropertyTile = true;
		// call the function
		OnLandedOnUnownedTile(tile);
		Debug.Log (playerName+": Landed on unowned tile");
		
		// wait for buying tile feedback
		while(isProcessingUnOwnedPropertyTile == true){
			Debug.Log (playerName+": Waiting for unowned tile to be processed");
			yield return new WaitForEndOfFrame();
		}
		// did we buy that tile?
		if(didBuyTile == false){
			// if not, AUCTION TIME WOOOT!
			Debug.Log (playerName+": AUCTIONING");
			auctionDone = false;
			GameController.singleton.AuctionProperty(tile, this);
			// wait for the auction to complete before continuing
			while(auctionDone == false){
				yield return new WaitForEndOfFrame();
			}
		}
		// done with the result of the move action
		DoneMoveAction();
	}
	
	public IEnumerator ProcessOwnedPropertyTile (BoardTile tile){
		if(tile.owner != playerIndex){
		// ah damn, we landed on a tile that was owned
		Debug.Log (playerName+": Paying Rent For Property");
		StartCoroutine(PayRentForProperty(tile));
		
		while(allPropertyRentPaid == false){
			yield return new WaitForEndOfFrame();
		}
		}
		else{
			Debug.Log ("We landed on our own tile... phew");
		}
		// done with the result of the move action
		DoneMoveAction();
	}
	
	public IEnumerator ProcessChanceCard (){
		
		DoneMoveAction();
		yield return 0;
	}
	
	public IEnumerator ProcessCommunityChestCard () {
		
		DoneMoveAction();
		yield return 0;
	}
	
	public IEnumerator PayTax (int tax) {
		
		hasPaidAllFunds = false;
		StartCoroutine(PayFunds(tax));
		
		// wait for tax to be paid
		while(hasPaidAllFunds == false){
			yield return new WaitForEndOfFrame();
		}
		
		if(isBankrupt == true){
			BankruptOnBank();
		}
		
		// just done the move action
		DoneMoveAction();
		yield return 0;
	}
	
	public void GoToJail () {
		// TODO implement jail
		DoneMoveAction();
	}
	
	public virtual void OnTurnHasStarted(){
		DoneTurnStart ();
	}
	
	public virtual void OnLandedOnUnownedTile(BoardTile landedTile){
		DoneProcessingUnOwnedTile();
	}
	
	public virtual void OnMustPayRent(int amountToPay){
		PayRent(amountToPay);
	}
	
	public virtual void OnEndOfTurn(){
		Debug.Log("DONE TURN END");
		DoneTurnEnd();
		
	}
	
	public virtual void OnTradeOfferRecieved (TradeOffer offer) {
		ResolveTradeOffer(true);
	}
	
	public void DoneTurnStart(){
		isProcessingTurnStart = false;
	}
	
	public void DoneTurnEnd () {
		turnOver = true;
	}
	
	public void DoneProcessingUnOwnedTile () {
		isProcessingUnOwnedPropertyTile = false;
	}
	
	public void DoneAuction () {
	auctionDone = true;
	}
	
	public bool BuyTile (BoardTile tileToBuy) {
		// check if we have enough money
		if(tileToBuy.isOwned == false && tileToBuy.tileType == TileType.property){
			if(money > tileToBuy.buyPrice){
				money -= (int)tileToBuy.buyPrice;
				didBuyTile = true;
				
				// add it to our list
				ownedProperties.Add (tileToBuy);
				// set ownership of the tile
				tileToBuy.SetOwner(this);
				// reduce the number of remaining properties
				GameController.remainingProperties -=1;
				// update our property values
				UpdateOwnedProperties();
				
				
			}
		}
		
		return didBuyTile;
	}
	
	public void ProposeNewTradeOffer(TradeOffer offer){
		pendingTradeOffer = true;
		if(isProcessingTradeOffer == true){
			Debug.Log (playerName+": Already proposing a trade offer, aborting current trade...");
			// decline this offer
			ResolveTradeOffer(false);
		}
		// check if the offer has a recipient
		if(offer.recieverPlayerId == -1){
			// we don't have a trade offer any more
			pendingTradeOffer = false;
			// end the function
			return;
		}
		GamePlayer target = offer.GetReciever();
		// check for a valid offer. otherwise, we just can't make this trade
		// do both players have the required funds
		if(offer.recieverFunds > target.money || offer.senderFunds > money){
			if(offer.senderFunds < 0 || offer.recieverFunds < 0){
			// no more trade offer
			Debug.Log (playerName+": Cannot Propose trade offer as the required funds cannot be transferred");
			pendingTradeOffer = false;
			}
		}
		else{
		Debug.Log (playerName+": New Trade Offer Proposed to "+target.playerName);
		target.StartCoroutine(target.ProcessTradeOffer(offer));
		}
	}
	
	public IEnumerator ProcessTradeOffer (TradeOffer offer) {
		// wait for 2 frames, incase this was a counter offer
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Debug.Log (playerName+": Trade Offer Recieved");
		// process trade offer
		isProcessingTradeOffer = true;
		Debug.Log (playerName+": Processing Trade Offer...");
		OnTradeOfferRecieved(offer);
		while(isProcessingTradeOffer == true){
			yield return new WaitForEndOfFrame();
		}
		Debug.Log (playerName+": Done processing trade offer...");
		// did we accept?
		if(didAcceptTrade == true){
			// make the swap
			Debug.Log ("YAY TRADE ACCEPTED");
			
			// get all the data from the trade
			GamePlayer senderPlayer = offer.GetSender();
			GamePlayer recieverPlayer = offer.GetReciever();
			BoardTile senderProperty = offer.GetSenderProperty();
			BoardTile recieverProperty = offer.GetRecieverProperty();
			// swap the funds
			senderPlayer.money += offer.recieverFunds;
			senderPlayer.money -= offer.senderFunds;
			recieverPlayer.money += offer.senderFunds;
			recieverPlayer.money -= offer.recieverFunds;
			
			// swap the properties
			senderPlayer.RemoveProperty(senderProperty);
			recieverPlayer.RemoveProperty(recieverProperty);
			senderPlayer.AddProperty(recieverProperty);
			recieverPlayer.AddProperty(senderProperty);
			
			// notify the sender that the trade was accepted
			senderPlayer.TradeOfferAccepted();
			UpdateOwnedProperties();
			Debug.Log ("Trade Complete!");
		}
		else{
			// nothing happens, trade failed
			offer.GetSender().TradeOfferDeclined();
			Debug.Log ("TRADE REJECTED");
		}
	}
	
	void AddProperty (BoardTile property){
		// add it to our list
		ownedProperties.Add (property);
		// set ownership of the tile
		property.SetOwner(this);
		// update our property values
		UpdateOwnedProperties();
	}
	
	void RemoveProperty(BoardTile property){
		if(ownedProperties.Contains(property) == false){
			//Debug.LogError ("PLAYER NEVER OWNED PROPERTY, ERROR");
		}
		ownedProperties.Remove (property);
		if(ownedProperties.Contains(property) == true){
			//Debug.LogError ("STILL OWNS PROPERTY AFTER TRADE, ERROR");
		}
		UpdateOwnedProperties();
	}
	
	public void ResolveTradeOffer (bool acceptTrade){
			Debug.Log (playerName+": Trade offer Resolved");
			didAcceptTrade = acceptTrade;
			isProcessingTradeOffer = false;
	}
	
	public void TradeOfferDeclined (){
		pendingTradeOffer = false;
	}
	public void TradeOfferAccepted () {
		pendingTradeOffer = false;
		UpdateOwnedProperties();
	}
	
	public bool BuyHouse (BoardTile property){
		
		
	// check if it can be upgraded
		if(property.tileType != TileType.property || property.tileGroup == TileGroup.other || property.tileGroup == TileGroup.station || property.tileGroup == TileGroup.utility){
			Debug.Log ("House cannot be bought here, invalid property");
			return false;
		}
		
		if(property.improvement == HouseImprovement.Hotel){
			return false;
			Debug.Log ("Property cannot be improved any more");
		}
		
		// check if we have the funds
		if(money < property.houseBuyCost){
			Debug.Log ("Don't have enough funds to buy a house there");
			return false;
		}
		int minHouseUpgradeStatus = 9;
		// get all the properties of this color
		for(int i=0;i<ownedProperties.Count;i++){
			// check if it is part of our group we are upgrading
			if(ownedProperties[i].tileGroup == property.tileGroup){
				// get the current house upgrade status
				int houseUpgradeStatus = (int)ownedProperties[i].improvement;
				// check if it is below the current minimum upgrade
				if(houseUpgradeStatus < minHouseUpgradeStatus){
					minHouseUpgradeStatus = houseUpgradeStatus;
				}
			}
		}
		
		// check if our improvement is the same as the minimum house upgrade status
		if((int)property.improvement  == minHouseUpgradeStatus){
			property.AddHouseUpgrade();
			money -= property.houseBuyCost;
			return true;
		}
		else{
			Debug.Log ("Cannot upgrade property, properties must be upgraded evenly.");
			return false;
		}
	}
	
	public void SellHouse (BoardTile property,int numOfHouses){
		if(property.owner == playerIndex){
			// determine how many houses can be sold back
			if(numOfHouses > (int)property.improvement){
				numOfHouses = (int)property.improvement;
			}
			
			// calculate money back from selling houses
			int moneyBack = (property.houseBuyCost/2)*numOfHouses;
			property.RemoveHouseUpgrade(numOfHouses);
		}
	}
	
	public BoardTile[] GetPropertiesInGroup(TileGroup grp){
		// create a property list
		List<BoardTile> properties = new List<BoardTile>();
		// iterate over our properties, and find all that match the criteria
		for(int i=0;i<ownedProperties.Count;i++){
			if(ownedProperties[i].tileGroup == grp){
				properties.Add(ownedProperties[i]);
			}
		}
		
		return properties.ToArray();
	}
	
	public void BankruptOnPlayer (GamePlayer player) {
		Debug.Log (playerName+": Bankrupt on Player");
		#if UNITY_EDITOR 
			//UnityEditor.EditorApplication.isPaused = true;
		#endif
		for(int i=0;i<ownedProperties.Count;i++){
			// remove all houses on that property
			ownedProperties[i].RemoveHouseUpgrade(999);
			// add our property to that player
			player.AddProperty(ownedProperties[i]);
			// remove property from our player
			RemoveProperty(ownedProperties[i]);
			// go back a step in the list
			i--;
		}
	}
	
	public void BankruptOnBank () {
		Debug.Log (playerName+": Bankrupt on Bank");
		#if UNITY_EDITOR 
			//UnityEditor.EditorApplication.isPaused = true;
		#endif
		for(int i=0;i<ownedProperties.Count;i++){
			// sell all houses on that property
			SellPropertyToBank(ownedProperties[i]);
			// go back a step in the list
			i--;
		}
	}
	
	public void SellPropertyToBank (BoardTile property) {
		Debug.Log (playerName+": Selling property to bank "+property.actualTileName);
		// remove the property from us
		RemoveProperty(property);
		// remove property
		property.ResetProperty();
		// add the money for that property back on
		money += (int)(property.buyPrice/2);
		// add 1 to the number of properties remaining
		GameController.remainingProperties +=1;
		
	}
	
	private void ForceEndTurn () {
		GameController.singleton.TurnFinished();
	}
	
}
