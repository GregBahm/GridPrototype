using System.Collections.Generic;
using UnityEngine;

class OctreeMain
{
    public OctreeMain(int levels)
    {
        //TODO: build octress containing octrees for each level
    }
}

class Octree
{
    private Octree[,] subTrees;
}

class OctreeLeaf
{
    private readonly IEnumerable<GameObject> gameObjects;
}