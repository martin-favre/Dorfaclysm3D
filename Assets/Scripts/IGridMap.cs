using System;
using UnityEngine;

public interface IGridMap : IHasBlocks, IObservable<BlockUpdate>
{

}