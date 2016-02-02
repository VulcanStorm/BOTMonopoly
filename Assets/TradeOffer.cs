using UnityEngine;
using System.Collections;

public struct TradeOffer {
	
	public int senderPlayerId;
	public int recieverPlayerId;
	public int senderPropertyId;
	public int recieverPropertyId;
	public int senderFunds;
	public int recieverFunds;
	
	public GamePlayer GetSender (){
		return GameController.singleton.players[senderPlayerId];
	}
	
	public GamePlayer GetReciever (){
		return GameController.singleton.players[recieverPlayerId];
	}
	
	public BoardTile GetRecieverProperty(){
		Debug.Log ("Sender property ID is= "+senderPropertyId.ToString());
		return GameController.singleton.players[recieverPlayerId].ownedProperties[recieverPropertyId];
	}
	
	public BoardTile GetSenderProperty(){
		Debug.Log ("Sender property ID is= "+senderPropertyId.ToString());
		return GameController.singleton.players[senderPlayerId].ownedProperties[senderPropertyId];
	}
	
	public TradeOffer (GamePlayer _sender,GamePlayer _reciever, BoardTile _senderProperty, BoardTile _recieverProperty, int _senderFunds, int _recieverFunds){
		senderPlayerId = _sender.playerIndex;
		recieverPlayerId = _reciever.playerIndex;
		// find the sender property ID
		senderPropertyId = -1;
		for(int i=0;i<_sender.ownedProperties.Count;i++){
			// find the property
			if(_sender.ownedProperties[i] == _senderProperty){
				// set the property ID
				senderPropertyId = i;
				// end the loop
				i = _sender.ownedProperties.Count;
			}
			
		}
		
		
		// find the reciever property ID
		recieverPropertyId = -1;
		for(int i=0;i<_reciever.ownedProperties.Count;i++){
			
			// find the property
			if(_reciever.ownedProperties[i] == _recieverProperty){
				// set the property ID
				recieverPropertyId = i;
				// end the loop
				i = _reciever.ownedProperties.Count;
			}
			
		}
		
		senderFunds = _senderFunds;
		recieverFunds = _recieverFunds;
		
	}
	
}
