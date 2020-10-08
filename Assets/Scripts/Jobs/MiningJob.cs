using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;

public class MiningJob : IJob
{
    StateMachine machine;

    public MiningJob(GridActor actor, MiningRequest request)
    {
        Debug.Assert(request != null);
        Debug.Log("Started a mining job");
        machine = new StateMachine(new WalkToBlockState(actor, request));
    }
    public IGenericSaveData GetSave()
    {
        throw new System.NotImplementedException();
    }

    public bool Work()
    {
        machine.Update();
        return machine.IsTerminated();
    }

    private class WalkToBlockState : WalkingState
    {
        private readonly GridActor user;
        private readonly MiningRequest request;

        public WalkToBlockState(GridActor user, MiningRequest request) : base(user, 0.3f)
        {
            this.user = user;
            this.request = request;
            Debug.Assert(this.request != null);
        }   

        public override Vector3Int GetTargetPos()
        {
            Vector3Int actualPos;
            Debug.Assert(request != null);
            bool success = GridMapHelper.GetClosestPassablePosition(request.Position, 1, out actualPos);
            if (success)
            {
                return actualPos;
            }
            else
            {
                OnPathFindFail();
                return request.Position;
            }
        }

        public override State OnPathFindFail()
        {
            MiningRequestPool.Instance.ReturnRequest(request);
            TerminateMachine();
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            return new MineBlockState(user, request);
        }
    }

    private class MineBlockState : State
    {
        private readonly GridActor user;
        private readonly MiningRequest request;

        public MineBlockState(GridActor user, MiningRequest request)
        {
            this.user = user;
            this.request = request;
        }
        public override State OnDuring()
        {
            if (!request.IsCancelled())
            {
                Block block;
                GridMap.TryGetBlock(request.Position, out block);
                if (block != null && block.Type == request.BlockType)
                {
                    GridMap.SetBlock(request.Position, new AirBlock());
                    request.Finish();
                }
            }
            TerminateMachine();
            return StateMachine.NoTransition();
        }
    }
}
