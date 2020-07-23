# The Great QuickSearch Syntax Cheat Sheet

## Provider Identifier token

By default QuickSearch will use all the Search Providers that are enabled in the Filter Window. If you want to limit your search to a single provider you can begin your search with a specific provider identifier token:

|Filter| Search <br/>token|Description|
|-|-|-|
|**Projects (or assets)**   | `p:`  | `p: asteroid`<br/>Searches for all `assets` containing the word `asteroid` in their name. |
|**Hierarchy (or scene)**   | `h:`  | `h: asteroid`<br/>Searches **current scene** for `game objects` containing the word `asteroid` in their name. |
|**Objects (prefab or indexed scenes)**   | `o:`  | See [Object Provider](#objects-provider) for detailed description.<br/><br/>`o: asteroid`<br/>Searches all indexed **scenes or prefabs** for `game objects` containing the word `asteroid` in their name. |
|**Menu**   | `me:`  | `me: test ru`<br/>Searches all menus for items containing the word `test` and the word `ru` (ex: Test Runner) |
|**Settings**   | `se:`  | `se: quick`<br/>Searches all **preferences** or **project settings** for sections containing the word `quick` (ex: QuickSearch preferences). |
|**Help**   | `? <search topic>`  | `?`<br/>Prints all sorts of search topic. |
|**Calculator**   | `=<mathematical expression>`  | `=78/2*33`<br/>Prints the result of a computation. |
|**Static C# API**   | `#`  | `# applica`<br/>Searches for **static C# properties or functions taking no parameters** containing the word `applica` (ex: `EditorApplication.applicationContentsPath`) |
|**Packages**   | `pkg:`  | `pkg: quick`<br/>Searches the package registry for packages containing the word `quick` (ex: QuickSearch). |
|**store**   | `store:`  | `store: bolt`<br/>Searches the Unity Asset Store for assets containing the word `bolt`. |
|**Resources (loaded objects)**   | `res:`  | `res: quick`<br/>Searches all in memory objects containing the word `quick` (ex: quickSearch Editor Window).  |
|**Queries**   | `q:`  | `q: enemies`<br/>Searches all **Search Query** assets containing the word `enemies`. |
|**Log**   | `log:`  | `log: adaptative`<br/>Searches the current Editor log for the word `adaptative`. |
-------------------------------


## Query Engine Operators
Most QuickSearch providers are using a `QueryEngine` ([Scene](#scene-provider), [Asset](#asset-provider), [Objects](#objects-provider) and [Resource](#resources-provider) providers) to parse and resolve their queries. This means they support a basic set of Query Operators that allows for more complex queries using boolean operators and parentheses grouping:

**A note on casing**: most Quicksearch query ignore casing. Which means `Stone` or `stone` or `sToNe` will yield the same results.

|Filter| Search <br/>token|Description|
|-|-|-|
|**Basic search**   | `<any partial name>`  | `main`<br/>Searches all objects matching the word `Main` |
|**And**   | `and`  | `Main and t:camera`<br/>Search all objects where name contains `Main` and is of type name containing `camera`<br/><br/>`t:texture and jpg`<br/>Searches all objects of type `texture` containing `jpg` in their filename. <br/><br/>Note that since `and` **is  the default operator of the QueryEngine** the last query is equivalent to:<br/>`t:texture jpg` |
|**or**   | `or`  | `Player or Monster`<br/>Searches all objects containing the word `Player` or `Monster`. |
|**Group**   | `(<group content>)`  | `t:Character and (status=Poison or status=Stunned)`<br/>Searches all objects with a `Character` component where the value of `status` property is either `Poison` or `Stunned`.  |
|**Exclusion**   | `-<Expression to exclude>`  | `p: dep:door -t:Scene`<br/>Searches all all `assets` with a dependency on an asset containing the word `door` and that are not of type `Scene`. <br/><br/>`p: dep:door -stone`<br/>Searches all all `assets` with a dependency on an asset containing the word `door` and that do not contain the word `stone`. |
|**Exact Operator**   | `!<something>`  | Most of the string matching in QuickSearch is done *partially*. When using the `!` operator you expect **exact** matching.<br/><br/>`p: stone`<br/>Searches all assets containing the word stone (`stone_hammer.png`, `stone_door.prefab`).<br/><br/>`p: !stone`<br/>Searches all assets whose name is **exactly** `stone` (ex: `stone.png`) |
|**Partial Value match (:)**   | `property:<partial value>`  | `ref:aster`<br/>Since `:` is used, searches all assets having an asset containing the word `aster` (ex: `asteroid2`, `asteroids`) as a dependency.  |
|**Exact Value (=)**   | `property=exactValue`  | `ref:aster`<br/>Since `=` is used, searches all assets having an asset named **exactly** `asteroid` as a dependency.  |
|**>**   | `property>number`  | `t:texture size>256`<br/>Searches all textures with a size bigger than 256 bytes.  |
|**<**   | `property<number`  | `t:texture size<256`<br/>Searches all textures with a size smaller than 256 bytes.  |
|**!=**   | `property!=number`  | `t:texture size!=256`<br/>Searches all textures with a size different than 256 bytes.  |
|**>=**   | `property>=number`  | `t:texture size>=256`<br/>Searches all textures with a size bigger or equal than 256 bytes.  |
|**<>=**   | `property>number`  | `t:texture size<=256`<br/>Searches all textures with a size smaller or equal than 256 bytes.  |
-------------------------------

## Scene Provider
Scene queries are run on **all** objects of the **current scene**. We do progressive caching so running the same query will be faster the next time. Note: these queries do not use indexed data (as opposed to [Asset](#asset-provider) and [Objects](#objects-provider) providers).

|Filter| Search <br/>token|Description|
|-|-|-|
|**Component type**   | `t:`  | `t:collid`<br/>Searches all game objects who have a component containing the word `collid` (ex: `Collider`, `Collider2d`, `MyCustomCollider`). |
|**Instance id**   | `id:`  | `id:210`<br/>Searches all game objects whose instanceID contains the word `210` (ex: `21064`).<br/><br/>`id=21064`<br/>Searches all game objects whose instanceID is **exactly** `21064`. |
|**Path**   | `path:parent/to/child`  | `path:Wall5/Br`<br/>Searches all game objects whose path matches the partial path `Wall5/Br` (ex: `/Structures/Wall5/Brick`)<br/><br/>`path=/Structures/Wall5/Brick`.<br/>Searches all game objects whose scene path is **exactly** `/Structures/Wall5/Brick`. |
|**Tag**   | `tag:`  | `tag:resp`<br/>Searches all game objects who have a **tag** containing the word `resp` (ex: `Respawn`)  |
|**Layer**   | `layer:<layer number>`  | `layer:8`<br/>Searches all game objects who are on layer `8` (ex: `8: Terrain` ) |
|**Size**   | `size:number`  | `size>5`<br/>Searches all game objects whose *AABB volume size* is bigger than `5`.|
|**Overlap**   | `overlap:number`  | `overlap>3`<br/>Searches all game objects whose renderer bounds intersects with more than `3` other game objects.|
|**Dependencies**   | `ref:<asset name>`  | `ref:stone`<br/>Searches all **game objects and their components** who have a dependency on an asset whose name contains the word `stone` |
|**Child**   | `is:child`  | `is:child`<br/>Searches all game objects who are the `child` of a game object. |
|**Leaf**   | `is:leaf`  | `is:leaf`<br/>Searches all game objects who don't have a `child`. |
|**Root**   | `is:root`  | `is:root`<br/>Searches all game objects who don't have a parent (i.e. that root objects in the scene). |
|**Visible**   | `is:visible`  | `is:visible`<br/>Searches all game objects who are visible by the camera of the **Scene View**. |
|**Hidden**   | `is:hidden`  | `is:hidden`<br/>Searches all game objects who are hidden according to the [SceneVisibilityManager](https://docs.unity3d.com/ScriptReference/SceneVisibilityManager.html). |
|**Static**   | `is:static`  | `is:static`<br/>Searches all game objects who are [static](https://docs.unity3d.com/ScriptReference/GameObject-isStatic.html). |
|**Prefab**   | `is:prefab`  | `is:prefab`<br/>Searches all game objects who are [part of a prefab](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfAnyPrefab.html). |
-------------------------------

### Prefab Filters
If you want to query prefab object you can use these specific filter predicates:

|Filter| Search <br/>token|Description|
|-|-|-|
|**Root prefab**   | `prefab:root`  | `prefab:root`<br/>Searches all game objects who are a [prefab root](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsAnyPrefabInstanceRoot.html).|
|**Top prefab**   | `prefab:top`  | `prefab:top`<br/>Searches all game objects who are [part of a prefab instance](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfPrefabInstance.html).|
|**Non asset prefab**   | `prefab:nonasset`  | `prefab:nonasset`<br/>Searches all game objects who are part of a [prefab that is not insided a prefab asset](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfNonAssetPrefabInstance.html).|
|**Asset prefab**   | `prefab:asset`  | `prefab:asset`<br/>Searches all game objects who are part of a [prefab asset](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfPrefabAsset.html).|
|**Any prefab**   | `prefab:any`  | `prefab:any`<br/>Searches all game objects who are [part of a prefab](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfAnyPrefab.html).|
|**Model prefab**   | `prefab:model`  | `prefab:model`<br/>Searches all game objects who are [part of a model prefab](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfModelPrefab.html).|
|**Regular prefab**   | `prefab:regular`  | `prefab:regular`<br/>Searches all game objects who are [part of a regular prefab instance or asset](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfRegularPrefab.html).|
|**Variant prefab**   | `prefab:variant`  | `prefab:variant`<br/>Searches all game objects who are [part of a prefab variant](https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfVariantPrefab.html).|
|**Modified prefab**   | `prefab:modified`  | `prefab:modified`<br/>Searches all game objects who are a [prefab instance with overrides](https://docs.unity3d.com/ScriptReference/PrefabUtility.HasPrefabInstanceAnyOverrides.html).|
|**Altered prefab**   | `prefab:altered`  | `prefab:altered`<br/>Searches all game objects who are a [prefab instance with overrides even on default overrides](https://docs.unity3d.com/ScriptReference/PrefabUtility.HasPrefabInstanceAnyOverrides.html).|

### Scene Property Search
You can use the special `p(<partial propertyname>)` syntax to filter objects according to the value of a property. We will try to match the **partial** name of the property against **any of the components of an object**. Note that we chose the syntax with `p()` to indicate this is a *dynamic operation* that loops over properties of object and that **doesn’t use an index**. Here are some examples of queries using `p()`:

- `p(drawmode)=Simple` Will match against the `drawmode` property of a Sprite renderer.
- `p(orthographic size)>2` Will match Camera with `Orthographic size` higher than 2.
- `p(istrigger)=false` Will match all objects where the `IsTrigger` property in a Collider2d is NOT a trigger.
- `p(sprite)=bee` Matches all GameObjects with a `sprite` property (ex: `Sprite Renderer`) that links to an asset whose name is **exactly** `bee`.
- `p(sprite):bee` Matches all GameObjects with a `sprite` property (ex: `Sprite Renderer`) that links to an object with a name containing the word `bee`.
- `p(spri):bee` Matches all GameObjects with a property containing the word `spri` (the `sprite` property of a `Sprite Renderer` component) that links to an object asset with a name containing the word `bee`.

**Notes on property name**
- Currently we index property name according to their "internal" name which might be different than the display name in the Inspector.
- You can always check the Inspector in debug mode to find the internal name of a property.

## Asset Provider

The Asset provider uses an **Asset** index to search efficiently. When creating an index you can choose how much of your data you want to have indexed using the Indexing Options (see Index Manager.)

### File Filters

|Filter| Search <br/>token|Description|
|-|-|-|
|**Default Search**   | `<search term>`  | Searches `term` attempting to match agains the asset name, type or path. <br/><br/>`texture`<br/>Searches all assets whose **name or path or type** contains the word `texture`. |
|**Name**   | `name:`  | `name:laser`<br/>Searches all assets who contains the word `laser`. <br/><br/>`name=laserbeam`<br/>Searches all assets where the name is exactly `laserbeam`. |
|**Directory**   | `dir:<directory exact name>`  | `dir:Scripts`<br/>Searches all assets contained in a directory named **exactly** `Scripts`.  |
|**Packages**   | `a:packages`  | `a:packages texture`<br/>Searches all textures in any packages.  |
|**Project**   | `a:assets`  | `a:assets texture`<br/>Searches all textures in the current project (i.e. in the `Assets` folder). |
|**Index file**   | `a:<index name>`  | `a:psd_textures texture`<br/>Assuming I have an index file named `psd_textures.index` in my project, Searches all textures in that index. |
|**Size**   | `size:<number of bytes>`  | `size:4000 texture`<br/>Searches all textures over 4000 bytes (4KB). |
|**Extension**   | `ext:<file extension without dots>`  | `ext:png texture`<br/>Searches all textures with the `png` extension. |
|**Age**   | `age:<number of days since last modification>`  | `age<3 texture`<br/>Searches all textures that were modified in the last 3 days. |
-------------------------------

### Type Filters
These filters are available if the index uses the **Types** Indexing option (See IndexManager).

|Filter| Search <br/>token|Description|
|-|-|-|
|**Type**   | `t:<Asset Type>`  | `t:texture`<br/>Searches all assets containing `texture` in their type name (ex: `Texture2D`, `Texture`).<br/><br/>`t:prefab`<br/> Searches all `prefab` assets. |
|**Type**   | `<Asset Type>`  | Note that since we index all type name it is possible to search asset by type without using the `t:` filter above. <br/><br/>`texture`<br/>Searches all assets containing `texture` in their **type name** (ex: `Texture2D`, `Texture`) or in **their name** (ex: `myTexture.png`).<br/><br/>`prefab`<br/>Searches all `prefab` assets or assets with `prefab in their name. |
|**File**   | `t:file` | `t:file level1`<br/>Searches all files assets containing the word `level1`. |
|**Folder**   | `t:folder`  | `t:folder`<br/>Searches all folders assets.
-------------------------------

### Indexed Property Search

Searching properties is available if the index has been specified with the **Properties** Indexing option (see IndexManager). A few remark regarding properties:

- We are improving autocompletion of properties of assets but if you want to look at the list of *all* indexed properties check the Index Manager **Keywords** tab.
- All property values are converted to string or number.
- The name of the property has to be complete and not partial (case does not matter though).
- We index properties of the top level object of a prefab asset. If you want all prefab hierarchies to be indexed create a **Prefab Index** (see [Objects Provider](#objects-provider) below).
- For `.unity` file we index the properties of the [SceneAsset](https://docs.unity3d.com/ScriptReference/SceneAsset.html) itself and not the scene content. If you want all scene contents to be indexed create a **Scene Index** (see [Objects Provider](#objects-provider) below).

|Filter| Search <br/>token|Description|
|-|-|-|
|**Type**   | `t:<type>`  | When using the **Property** indexing `t:` can be use to search for **component type** (on prefab) for **asset type**.<br/><br/>`t:collider`<br/>Searches all prefabs containing a component with the word `collider`.<br/><br/>`t:texture`<br/>Searches all asset whose type contains the word `texture` (ex: `Texture` or `Texture2D`). |
|**Has Component**   | `has:<component type>`  | `has:collider`<br/>Searches all prefabs containing a **component** with the word `collider`.<br/><br/>`has=BoxCollider`<br/>Searches all prefabs containing a **component** exactly called `BoxCollider`. |
|**Label**   | `l:<label name>`  | `l:archi`<br/>Searches all assets with a label containing the word `archi` (Ex: `Architecture`).<br/><br/>`l=Wall`<br/> Searches all assets with a label that is exactly `Wall`|
-------------------------------

All properties of an asset (prefab or other types) are indexed and searchable. Here are a few examples of property query:

|Filter| Search <br/>token|Description|
|-|-|-|
|**Number**| `property:value`  | `bounciness>0.1`<br/>Searches all assets with a property named `bounciness` (ex: a `PhysicsMaterial2D`) higher than 0.1.<br/><br/>`health=2`<br/>Searches all assets with a property named `health` (ex: `HealthSystem` Component of a prefab) with of a value of exactly 2.<br/><br/>`t:texture filtermode!=0`<br/>Searches all textures with a `filtermode` property different than 0 (i.e different than `Point`). |
|**Boolean**| `property:value`  | `t:Dungeon generatePath=true`<br/>Searches all Dungeon ScriptableObject where the property `generatePath` is `true`.<br/><br/>`isStunned=false`<br/>Searches all objects containing a property `isStunned` that is `false`. |
|**String**| `property:string value`  | `t:Character trait:indestru`<br/>Searches all prefab with a `Character` component whose `trait` property contains the word `indestru` (ex: indestructible).<br/><br/>`t:Character trait="tough but fair"`<br/>Searches all prefab with a `Character` component whose `trait` property is exactly `tough but fair`. |
|**Enum**| `property:<enum value>`  | `characterclass:rog`<br/>Searches all objects with with a property named `characterclass` whose value contains the word `rog` (ex: value is `rogue`). <br/><br/>`characterclass=FighterMage`<br/>Searches all objects with a property named `characterclass` with an exact value of `FighterMage`.|
|**Color**| `property:<html color value>`  | `color:ADA`<br/>Searches all objects with with a property named `color` where the color value starts with `ADA` (like `ADADAD00`).<br/><br/>`color=ADADAD00`<br/>Searches all objects with with a property named `color` where the color value is exactly `ADADAD00`.<br/><br/>`color=ADADAD`<br/>Searches all objects with with a property named `color` where the color value is exactly `ADADAD` and **alpha value is 1.**|
|**Vector**| `property.[xyzw]:value`  | `bounds.x>1`<br/>Searches all objects with with a property named `bounds` where the `x` value is bigger than `1`.<br/><br/>`acceleration.z=2`<br/>Searches all objects with with a property named `acceleration` where the `z` value is equal to `2`|
|**Object**| `sprite:<object exact name>`  | `sprite:CharacterBody`<br/>Searches all assets with a `sprite` property (ex: `Image` Component of a prefab) that references an object whose [name](https://docs.unity3d.com/ScriptReference/Object-name.html) is `CharacterBody`.|

### Dependency Filters

If you are using the **Dependencies** Indexing option (See IndexManager) we index direct dependencies of all assets using [AssetDatabase.GetDependencies](https://docs.unity3d.com/ScriptReference/AssetDatabase.GetDependencies.html).

|Filter| Search <br/>token|Description|
|-|-|-|
|**Reference Path**   | `ref:<asset full path>`  | `ref:assets/images/particles/p_smoke.png`<br/>Searches all assets with a direct dependencies on the **exact** asset path: `assets/images/particles/p_smoke.png` . |
|**Reference Name**   | `ref:<asset name>`  | `ref:p_smo`<br/>Searches all assets with a direct dependencies on an asset whose name contains the word `p_smo`. <br/><br/>`ref:p_smoke.png`<br/>Searches all assets with a direct dependencies on an asset whose name is `p_smoke.png`.|
-------------------------------

## Objects Provider

The Object provider uses **Prefab** indexes or **Scene** indexes to search either prefab or scene **hierarchies** (all prefabs sub objects or all of a scene gameobjects) efficiently without having the scene or prefab loaded as the current scene. When setupping a **Prefab** or **Scene** index you can choose how much of your data you want to have indexed using the Indexing Options (see Index Manager). 

Remark on Objects provider usage:
- If you are already indexing your project with an **Asset** index and if all of your prefabs contain only a single object (no deep hierarchy) using a **Prefab** index is not necessary.
- The Objects provider has some filters who are similar to the [Scene](#scene-provider) provider (ex: `tag:`, `layer:`). Keep in mind though that Objects providers indexes the data and is not dynamic in the current scene (like the Scene provider).


Object provider supports the following set of filters (coming either from the [Asset](#asset-provider) or [Scene](#scene-provider) provider)
- Objects provider supports [Type](#type-filters) filters.
- Objects provider supports [Prefabs](#prefab-filters) filters.
- Objects provider supports [Properties](#indexed-property-search) search.
- Objects provider supports [Dependency](#dependency-filters) filters.

Here are the filters that are made specifically for Objects. Note that some filters are similar to the [Scene Provider] filters. The difference is that Objects uses indexed data (so it allows for faster search) but doesn't support dynamic filter (ex: filter doing dynamic computation).

|Filter| Search <br/>token|Description|
|-|-|-|
|**Area**   | `a:<scene or prefab index name>`  | `a:MyPrefabIndex`<br/>Searches all gameobjects that are part of a prefab indexed by the `MyPrefabIndex` index. |
|**Type**   | `t:<type>`  | `t:Image`<br/>Searches all gameobjects in an indexed prefab or scene hierarchy that have a component of type `Image`. |
|**Depth**   | `depth:number`  | `depth=1`<br/>Searches all gameobjects in an indexed prefab or scene hierarchy that have a depth of 1 (i.e. root object in the hierarchy).<br/><br/>`depth>1`<br/>Searches all gameobjects in an indexed prefab or scene hierarchy that are not root object. |
|**From**   | `from:prefab`  | `from:prefab`<br/>Searches all gameobjects coming from Prefab indexes. |
|| `from:scene`  | `from:scene`<br/>Searches all gameobjects coming from Scene indexes. |
|**Child**   | `is:child`  | `is:child`<br/>Searches all gameobjects who have a parent transform (are child of another gameobject). |
|**Root**   | `is:root`  | `is:root`<br/>Searches all gameobjects who don't have a parent transform. |
|**Leaf**   | `is:leaf`  | `is:leaf`<br/>Searches all gameobjects who don't have any children. |
|**Layer**   | `layer:<layer number>`  | `layer:8`<br/>Searches all game objects who are on layer `8` (ex: `8: Terrain` ) |
|**Tag**   | `tag:`  | `tag:resp`<br/>Searches all game objects who have a **tag** containing the word `resp` (ex: `Respawn`)  |
|**Prefab root**   | `prefab:root`  | `prefab:root`<br/>Searches all game objects who are the root of a prefab and use anywhere in a hierarchy. |
|**Component count**   | `components:number`  | `components>3`<br/>Searches all game objects who have more than 3 components. |
-------------------------------

## Resource provider

The Resource provider allows querying all objects that are currently in memory for the editor. You can access GameObjects, Assets, Editor Windows, etc. Basically anything deriving from [Unity.Object](https://docs.unity3d.com/ScriptReference/Object.html).

Note that to query this provider you need to explicitly add `res:` to the query.

|Filter| Search <br/>token|Description|
|-|-|-|
|**Type**   | `t:<type>`  | `res: t:texture`<br/>Searches all loaded resources, and returns `Texture` type resources only.<br/><br/>`res: t:inspector`<br/>Searches all loaded resources, and returns objects that contains the word `inspector` in their type (ex: `UnityEditor.InspectorWindow`, `UnityEditor.GenericInspector`). |
|**Name**   | `n:name`  | `res: n:inspectorwindow`<br/>Searches all loaded resources and returns objects with the word `window` in their name (ex: `InspectorWindow MonoScript`, `ConsoleWindow Icon`). |
|**ID**   | `id:number`  | `res: id:-15`<br/>Searches all loaded resources and returns the ones whose **instance IDs** begin with `-15`. |
|**Tag**   | `tag:value`  | `res: tag:Untagged`<br/>Searches all loaded resources and returns the ones with no tag. |
-------------------------------

## Asset Store provider

This provider can query the Unity [Asset Store](https://assetstore.unity.com).

Note that to query this provider you need to explicitly add `store:` to the query.

|Filter| Search <br/>token|Description|
|-|-|-|
|**Minimum price**   | `min_price:number`  | `store: min_price:5 bolt`<br/>Searches the asset store for assets with a minimum price of `5`$ and the word `bolt` in their name. |
|**Maximum price**   | `max_price:number`  | `store: max_price:5 bolt`<br/>Searches the asset store for assets with a maximum price of `5`$ and the word `bolt` in their name. |
|**Publisher**   | `publisher:name`  | `store: publisher:Gargore`<br/>Searches the asset store for assets published by the company `Gargore`. Note that the publisher name must be **exact**. |
|**Version**   | `version:number`  | `store: version:2017`<br/>Searches the asset store for assets that minimally supports Unity 2017. |
|**Free**   | `free:boolean`  | `store: free:true asteroid`<br/>Searches the asset store for assets that are free and that have the word `asteroid` in their name. |
|**On sale**   | `on_sale:boolean`  | `store: on_sale:true max_price:5`<br/>Searches the asset store for assets that are on sale and that have a max price of 5$. |
-------------------------------

## Calculator

This provider allows for quick computation using a set of simple arithmetic operators. 

Note that to query this provider you need to explicitly add `=` to the query.

|Filter| Search <br/>token|Description|
|-|-|-|
|**Supported operators**   | `+ - * / % ^ ( )` | `=42/34 + (56 % 6)`<br/><br/>`=23 * 9 ^ 3` |
-------------------------------

## Complex Queries examples

Here are a few examples of more complex queries using various filters:

|Query|Description|
|--|-|
|`h: t:meshrenderer p(castshadows)!="Off"`| Searches all static meshes in scene that cast shadow. |
|`h: t:light p(color)=#FFFFFF p(intensity)>7.4`| Searches all lights in scene with a specific color, but with brightness higher than 7.4 |
|`o: t:healthui ref:healthcanvas`| Use the Object provider to search all indexed prefabs and scene for GameObjects with a `HealthUI` component that reference the `healthcanvas` prefab.|

-------------------------------

