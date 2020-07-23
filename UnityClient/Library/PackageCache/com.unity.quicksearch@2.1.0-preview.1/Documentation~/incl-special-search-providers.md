<a name="providers"></a>

|Provider:|Function:|Search token:|Example:|
|-|-|-|-|
|[Calculator](search-calculator.md)   |Computes mathematical expressions.| `=`  | `=2*3+29/2` <br/><br/>Calculates the answer to the expression `2*3+29/2`.  |
|[Command Query](search-command-query.md)   |Searches for items that support a specific command.|  `>` | `>select` <br/><br/>Searches for Scene items that you can select.  |
|[Help](search-help.md)   |Searches the Quick Search help.|  `?` | `?asset` <br/><br/>Searches for Quick Search help entries containing the word "Asset".  |
|[Resource](search-resources.md)   |Searches for loaded resources.| `res:` + optional sub-filter  | `res: t: texture`<br/><br/>Searches all loaded resources, and returns Texture type resources only.  |
|[Static API Method](search-api.md)   |Finds and executes static API methods.| `#`  | `#Mesh` <br/><br/>Searches for static API methods with "Mesh" in their names.   |