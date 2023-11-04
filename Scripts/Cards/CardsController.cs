using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    public class CardsController : MonoBehaviour
    {
        private Camera _camera;
        public List<GameObject> cardsPattern;
        // The SortedList _cardsDataDict is like the local database of this controller that store all individual
        // cards state.
        // So we can easily get all value of the card when the user touch the card triggered by a raycast.
        private readonly SortedList<GameObject, Card> _cardsDataDict = new();
        private readonly Dictionary<GameObject, Animator> _cardsAnimatorsDict = new();
        private static readonly int IsSpawn = Animator.StringToHash("isSpawn");
        public bool IsCardsSpawned { get; private set; }

        private void Start()
        {
            _camera = Camera.main;
            InitialiseCards();
        }

        private void FixedUpdate()
        {
            DetectTouch();
        }

        private void InitialiseCards()
        {
            var incrementVector = -0.5f;
            foreach (var card in cardsPattern)
            {
                var spawnedCard = Instantiate(card, transform.position + new Vector3(1,0,0) * incrementVector, Quaternion.Euler(-90,-90,0));
                _cardsAnimatorsDict.Add(spawnedCard, spawnedCard.GetComponent<Animator>());
                _cardsDataDict.Add(spawnedCard, spawnedCard.GetComponent<Card>()); // store the gameObject (automatically convert to hash) as key and set controller as value
                incrementVector += 0.5f;
            }
        }
    

        private void DetectTouch()
        {
            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;
        
            var ray = _camera.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out var hit))
            {
                var card = _cardsDataDict[hit.transform.gameObject];
                Debug.Log($"{card.Force} {card.CardName} {card.Type}");
            }
        }

        private void SelectCard(GameObject cardSelected)
        {
            var card = _cardsDataDict[cardSelected];
            
        }
        
        /// <summary>
        /// Change cards properties and respawn with switching her animator controller values
        /// </summary>
        public IEnumerator ChangeCards(List<Card> replacementCards)
        {
            foreach (var cardData in _cardsDataDict)
            {
                cardData.Key.SetActive(false);
                _cardsAnimatorsDict[cardData.Key].SetBool(IsSpawn, false);
                var replacementCard = replacementCards[_cardsDataDict.IndexOfKey(cardData.Key)];
                (cardData.Value.CardName, cardData.Value.Force, cardData.Value.Type) = 
                    (replacementCard.CardName, replacementCard.Force, replacementCard.Type);
                
                _cardsAnimatorsDict[cardData.Key].SetBool(IsSpawn, true);
                cardData.Key.SetActive(true);
                yield return new WaitForSeconds(1);
            }
            IsCardsSpawned = true;
        }
    }
}
