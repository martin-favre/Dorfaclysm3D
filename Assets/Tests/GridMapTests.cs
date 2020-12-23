using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GridMapTests
    {

        [Test]
        public void GetSetSizeTest()
        {
            GridMap.Instance.SetSize(new Vector3Int(5, 5, 5));
            Vector3Int size = new Vector3Int(10, 10, 10);
            Assert.AreNotEqual(size, GridMap.Instance.GetSize()); // reality check

            GridMap.Instance.SetSize(size);

            Assert.IsTrue(size.Equals(GridMap.Instance.GetSize()));
        }

        [Test]
        public void GetBlockTest_Succesful()
        {
            Vector3Int size = new Vector3Int(10, 10, 10);
            GridMap.Instance.SetSize(size);
            Vector3Int pos = new Vector3Int(0, 0, 0);
            Block newBlock = new RockBlock();
            GridMap.Instance.SetBlock(pos, newBlock);
            Block block = GridMap.Instance.GetBlock(pos);
            Assert.AreEqual(block, newBlock);
        }

        [Test]
        public void GetBlockTest_NoBlock_ExpectThrow()
        {
            Vector3Int size = new Vector3Int(10, 10, 10);
            GridMap.Instance.SetSize(size);
            try
            {
                Block block = GridMap.Instance.GetBlock(new Vector3Int(0, 0, 0));
                Assert.IsTrue(false);
            }
            catch (KeyNotFoundException)
            {

            }
        }


    }
}
