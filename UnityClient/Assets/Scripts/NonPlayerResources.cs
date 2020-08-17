using System.Collections.Generic;

public class NonPlayerResources : ResourceGroup
{

   public Resource health;

   void Awake()
   {
      this.resources = new Dictionary<string, Resource>();

      this.health = this.AddResource("health");
   }
}

