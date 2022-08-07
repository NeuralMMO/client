using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.InventoryEngine
{	
	[RequireComponent(typeof(InventoryDisplay))]
	/// <summary>
	/// A component that will handle the playing of songs when paired with an InventoryDisplay
	/// </summary>
	public class InventorySoundPlayer : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Sounds")]
		[MMInformation("Here you can define the default sounds that will get played when interacting with this inventory.",MMInformationAttribute.InformationType.Info,false)]
		/// the audioclip to play when the inventory opens
		public AudioClip OpenFx;
		/// the audioclip to play when the inventory closes
		public AudioClip CloseFx;
		/// the audioclip to play when moving from one slot to another
		public AudioClip SelectionChangeFx;
		/// the audioclip to play when moving from one slot to another
		public AudioClip ClickFX;
		/// the audioclip to play when moving an object successfully
		public AudioClip MoveFX;
		/// the audioclip to play when an error occurs (selecting an empty slot, etc)
		public AudioClip ErrorFx;
		/// the audioclip to play when an item is used, if no other sound has been defined for it
		public AudioClip UseFx;
		/// the audioclip to play when an item is dropped, if no other sound has been defined for it
		public AudioClip DropFx;
		/// the audioclip to play when an item is equipped, if no other sound has been defined for it
		public AudioClip EquipFx;

		protected string _targetInventoryName;
		protected AudioSource _audioSource;

		/// <summary>
		/// On Start we setup our player and grab a few references for future use.
		/// </summary>
		protected virtual void Start()
		{
			SetupInventorySoundPlayer ();
			_audioSource = GetComponent<AudioSource> ();
			_targetInventoryName = this.gameObject.MMGetComponentNoAlloc<InventoryDisplay> ().TargetInventoryName;
		}

		/// <summary>
		/// Setups the inventory sound player.
		/// </summary>
		public virtual void SetupInventorySoundPlayer()
		{
			AddAudioSource ();			
		}

		/// <summary>
		/// Adds an audio source component if needed.
		/// </summary>
		protected virtual void AddAudioSource()
		{
			if (GetComponent<AudioSource>() == null)
			{
				this.gameObject.AddComponent<AudioSource>();
			}
		}

		/// <summary>
		/// Plays the sound specified in the parameter string
		/// </summary>
		/// <param name="soundFx">Sound fx.</param>
		public virtual void PlaySound(string soundFx)
		{
			if (soundFx==null || soundFx=="")
			{
				return;
			}

			AudioClip soundToPlay=null;
			float volume=1f;

			switch (soundFx)
			{
				case "error":
					soundToPlay=ErrorFx;
					volume=1f;
					break;
				case "select":
					soundToPlay=SelectionChangeFx;
					volume=0.5f;
					break;
				case "click":
					soundToPlay=ClickFX;
					volume=0.5f;
					break;
				case "open":
					soundToPlay=OpenFx;
					volume=1f;
					break;
				case "close":
					soundToPlay=CloseFx;
					volume=1f;
					break;
				case "move":
					soundToPlay=MoveFX;
					volume=1f;
					break;
				case "use":
					soundToPlay=UseFx;
					volume=1f;
					break;
				case "drop":
					soundToPlay=DropFx;
					volume=1f;
					break;
				case "equip":
					soundToPlay=EquipFx;
					volume=1f;
					break;
			}

			if (soundToPlay!=null)
			{
				_audioSource.PlayOneShot(soundToPlay,volume);
			}
		}		

		/// <summary>
		/// Plays the sound fx specified in parameters at the desired volume
		/// </summary>
		/// <param name="soundFx">Sound fx.</param>
		/// <param name="volume">Volume.</param>
		public virtual void PlaySound(AudioClip soundFx,float volume)
		{
			if (soundFx!=null)
			{
				_audioSource.PlayOneShot(soundFx,volume);
			}
		}	

		/// <summary>
		/// Catches MMInventoryEvents and acts on them, playing the corresponding sounds
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			// if this event doesn't concern our inventory display, we do nothing and exit
			if (inventoryEvent.TargetInventoryName != _targetInventoryName)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					this.PlaySound("select");
					break;
				case MMInventoryEventType.Click:
					this.PlaySound("click");
					break;
				case MMInventoryEventType.InventoryOpens:
					this.PlaySound("open");
					break;
				case MMInventoryEventType.InventoryCloses:
					this.PlaySound("close");
					break;
				case MMInventoryEventType.Error:
					this.PlaySound("error");
					break;
				case MMInventoryEventType.Move:
					if (inventoryEvent.EventItem.MovedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("move"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.MovedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemEquipped:
					if (inventoryEvent.EventItem.EquippedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("equip"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.EquippedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemUsed:
					if (inventoryEvent.EventItem.UsedSound == null)
					{
								if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("use"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.UsedSound, 1f);
					}
					break;
				case MMInventoryEventType.Drop:
					if (inventoryEvent.EventItem.DroppedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("drop"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.DroppedSound, 1f);
					}
					break;
			}
		}

		/// <summary>
		/// OnEnable, we start listening to MMInventoryEvents.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to MMInventoryEvents.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}
