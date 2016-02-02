using UnityEngine;
using System.Collections;

public class TestBot : GamePlayer {
	
	public bool hasSet = false;
	
	public override void OnTurnHasStarted(){
		
		DoneTurnStart ();
	}
	
	public override void OnLandedOnUnownedTile(BoardTile landedTile){
		// try and buy the tile
		Debug.Log (playerName+": Trying to buy unowned tile");
		BuyTile(landedTile);
		
		DoneProcessingUnOwnedTile();
	}
	
	public override void OnMustPayRent(int amountToPay){
		// dont sell assets, just go bankrupt...
		// simple test bot just pays cash
		PayRent(amountToPay);
	}
	
	public override void OnEndOfTurn(){
		StartCoroutine(m_EndOfTurn());
	}
	
	public override void OnTradeOfferRecieved(TradeOffer offer){
		Debug.Log (playerName + ": Resolving Trade offer");
		if(hasSet == false){
		ResolveTradeOffer(true);
		}
		else{
		ResolveTradeOffer(false);
		}
	}
	
	IEnumerator m_EndOfTurn () {
		// trading?
		
		
		// check for a set
		hasSet = false;
			// iterate over the group of properties
			for(int i=0;i<numberOfPropertiesInGroup.Length;i++){
				if(numberOfPropertiesInGroup[i] == GameController.singleton.numberOfEachPropertyGroup[i]){
					hasSet = true;
				}
			}
			
		
		
		// only if no properties left
		if(hasSet == false){
		
			// find the property group with the largest amount in it
			int largestPropertyGroupNum = -1;
			int largestGroupIndex = -1;
			for(int i=0;i<numberOfPropertiesInGroup.Length;i++){
				
				if(numberOfPropertiesInGroup[i] > largestPropertyGroupNum){
					largestPropertyGroupNum = numberOfPropertiesInGroup[i];
					largestGroupIndex = i;
				}
			}
			
			// calculate our desired property group
			TileGroup desiredGroup = (TileGroup)largestGroupIndex;
			Debug.Log ("Desired Group is: "+desiredGroup);
			// get all the properties in the game
			BoardTile[] propertyList = GameController.GetAllProperties();
			
			BoardTile desiredProperty = null;
			// find a property from another player in our desired group
			for(int i=0;i<propertyList.Length;i++){
				// find the property we want in our group
				if(propertyList[i].tileGroup == desiredGroup && propertyList[i].owner != playerIndex){
					// this is the property we want
					desiredProperty = propertyList[i];
					// stop the loop
					i = propertyList.Length;
				}
			}
			Debug.Log ("Desired Property is: "+desiredProperty);
			// check if the property is owned, because theres no point trying to trade if it isnt
			if(desiredProperty.owner != -1){
				Debug.Log ("Desired Property owner is: "+desiredProperty.owningPlayer.playerName);
				// find a property we want to trade
				BoardTile sendingProperty = null;
				// iterate over all the properties we own
				for(int i=0;i<ownedProperties.Count;i++){
					// check for a property not part of our desired group
					if(ownedProperties[i].tileGroup != desiredGroup){
						// we have found a property to send
						sendingProperty = ownedProperties[i];
						// stop the loop
						i = ownedProperties.Count;
					}
				}
					Debug.Log ("Sending Property is: "+sendingProperty);
				if(desiredProperty != null && sendingProperty != null){
					
					// make a trade for the desired property
					TradeOffer tradeOffer = new TradeOffer(this,desiredProperty.owningPlayer,sendingProperty,desiredProperty,0,0);
					// propose the offer
					ProposeNewTradeOffer(tradeOffer);
					// wait for a response
					Debug.Log (playerName+": WaitingForTradeResponse");
					while(pendingTradeOffer == true){
						Debug.Log (playerName+ ": pending trade offer? = "+pendingTradeOffer);
						yield return new WaitForEndOfFrame();
					}
				}
			}
			else{
				Debug.Log ("Desired Property isnt owned :(");
			}
		}
		// variable to hold the full property group
		TileGroup fullPropertyGroup = TileGroup.other;
		// check for a set to upgrade
		for(int i=0;i<ownedProperties.Count;i++){
			if(ownedProperties[i].improvement != HouseImprovement.Hotel && ownedProperties[i].fullSet == true && ownedProperties[i].canBuildHouses == true){
				// set our desired set to upgrade
				fullPropertyGroup = ownedProperties[i].tileGroup;
				// stop the loop
				i=ownedProperties.Count;
			}
		}
		
		// Get the properties in our group
		BoardTile[] groupProperties = GetPropertiesInGroup(fullPropertyGroup);
		
		// buy houses on them.
		for(int i=0;i<groupProperties.Length;i++){
			BuyHouse(groupProperties[i]);
		}
		
		
		Debug.Log("Ending Turn Now...");
		DoneTurnEnd();
	}
}
