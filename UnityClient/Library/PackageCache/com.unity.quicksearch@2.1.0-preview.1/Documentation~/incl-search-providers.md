<a name="providers"></a>

|Provider:|Function:|Search token:|Example:|
|-|-|-|-|
|[Asset](search-assets.md)   |Searches Project Assets.| `p:` (for "project")  | `p:Player` <br/><br/>Searches for Assets that match the term "Player". |
|[Asset Store](search-asset-store.md)   |Searches the [Unity Asset Store](https://assetstore.unity.com).| `store:`   | `store:texture` <br/><br/>Searches the Unity Asset Store for Assets that match the term "texture". |
|[Menu](search-menu.md)   |Searches the Unity main menu.| `me:`  | `me:TextMesh Pro`<br/><br/>Searches the Unity main menu for commands that contain "TextMesh Pro."  |
|[Scene](search-scene.md)   |Searches GameObjects in the Scene.| `h:` (for "hierarchy")  | `h:Main Camera` <br/><br/> Searches the current Scene for GameObjects that match the term "Main Camera".  |
|[Packages](search-packages.md)   |Searches the Unity package database.| `pkg:`  | `pkg:vector`<br/><br/>Searches the Unity package database for packages that match the term "vector". |
|[Settings](search-settings.md)   |Searches all [Project Settings](https://docs.unity3d.com/Manual/comp-ManagerGroup.html) and [Preferences](https://docs.unity3d.com/Manual/Preferences.html).|`se:`   | `se:VFX` <br/><br/> Finds Project Settings and Preferences pages that match the term "VFX". |
|[Online Search](search-online.md)  |Searches Unity-related online resources.| `web:`  | `web:UIElements`<br/><br/>Returns one item for each available online resource. When you [execute](usage.md#performing-actions) the item, Quick Search searches that online resource for the search term. |