using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;
using System;

namespace Items
{
    public class DroppedItemComponent : MonoBehaviour, ISaveableComponent
    {
        private class SaveData : GenericSaveData<DroppedItemComponent>
        {
            public int dropIndex = 0;
        }

        const string prefabName = "Prefabs/ItemContainerObject";

        static readonly float[] fallPositions = {
            0.25f,
            0.24858767036226256f,
            0.24576301108678766f,
            0.24152602217357533f,
            0.23587670362262553f,
            0.2288150554339383f,
            0.2203410776075136f,
            0.21045477014335146f,
            0.19915613304145188f,
            0.18644516630181485f,
            0.17232186992444037f,
            0.15678624390932847f,
            0.13983828825647907f,
            0.12147800296589226f,
            0.101705388037568f,
            0.08052044347150629f,
            0.05792316926770714f,
            0.033913565426170536f,
            0.008491631946896491f,
            0f,
            0.017371654544170594f,
            0.033330979450603745f,
            0.04787797471929945f,
            0.061012640350257705f,
            0.07273497634347854f,
            0.0830449826989619f,
            0.09194265941670782f,
            0.09942800649671628f,
            0.10550102393898729f,
            0.11016171174352085f,
            0.11341006991031698f,
            0.11524609843937565f,
            0.11566979733069688f,
            0.11468116658428065f,
            0.11228020620012699f,
            0.10846691617823587f,
            0.10324129651860732f,
            0.09660334722124128f,
            0.08855306828613785f,
            0.07909045971329691f,
            0.06821552150271859f,
            0.05592825365440275f,
            0.04222865616834955f,
            0.027116729044558807f,
            0.010592472283030722f,
            0f,
        };

        GridActor actor;
        InventoryComponent inventory;
        BlockVisualizer visualizer;

        SaveData data = new SaveData();

        SimpleObserver<CameraController> cameraObserver;
        MeshRenderer myRenderer;

        public static DroppedItemComponent InstantiateNew(Vector3Int position)
        {
            GameObject prefabObj = PrefabLoader.GetPrefab(prefabName);
            GameObject obj = Instantiate(prefabObj) as GameObject;
            if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
            GridActor gridActor = obj.GetComponent<GridActor>();
            if (!gridActor) throw new System.Exception("No GridActor Component on " + prefabName);
            gridActor.Move(position);
            DroppedItemComponent comp = obj.GetComponent<DroppedItemComponent>();
            if (!comp) throw new System.Exception("No ItemContainerComponent Component on " + prefabName);
            return comp;
        }

        void Start()
        {
            myRenderer = GetComponent<MeshRenderer>();
            actor = GetComponent<GridActor>();
            inventory = GetComponent<InventoryComponent>();
            if (actor && inventory)
            {
                inventory.RegisterOnItemRemovedCallback(OnItemRemoved);
                inventory.RegisterOnItemAddedCallback(OnItemAdded);
                ItemMap.RegisterInventory(inventory, actor.GetPos());
                transform.position = actor.GetPos() + new Vector3(.25f, -.25f, .25f);


                Item mainItem = inventory.GetMostCommonItem();
                visualizer = GetComponent<BlockVisualizer>();
                visualizer.RenderBlock(mainItem.GetTexturePosition());
            }
            cameraObserver = new SimpleObserver<CameraController>(Camera.main.GetComponent<CameraController>(), (c) =>
            {
                if (this.myRenderer && actor)
                {
                    this.myRenderer.enabled = c.PositionShouldBeVisible(actor.GetPos());
                }

            });

        }

        void FixedUpdate()
        {
            if (data.dropIndex < fallPositions.Length)
            {
                transform.position = actor.GetPos() + new Vector3(.25f, -.50f + fallPositions[data.dropIndex], .25f);
                data.dropIndex++;
            }
        }

        void OnItemAdded()
        {

        }

        void OnItemRemoved()
        {
            if (!inventory.HasItems())
            {
                print("No items, DroppedItemComponent removing itself");
                GameObject.Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (actor && inventory)
            {
                ItemMap.UnregisterInventory(inventory, actor.GetPos());
            }
        }

        public IGenericSaveData Save()
        {
            return data;
        }

        public void Load(IGenericSaveData data)
        {
            this.data = (SaveData)data;
        }
    }
}