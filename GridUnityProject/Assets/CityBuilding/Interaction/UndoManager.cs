using System.Collections.Generic;
using System.Linq;

public class UndoManager
{
    private readonly InteractionManager interactor;
    private readonly List<UndoableOperation> stack = new List<UndoableOperation>();

    public UndoManager(InteractionManager interactor)
    {
        this.interactor = interactor;
    }

    public bool CanUndo()
    {
        return stack.Any();
    }

    public void Undo()
    {
        UndoableOperation last = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        last.Undo();
    }

    public void RegisterDesignationPlacement(DesignationCell cell)
    {
        DesignationPlacementOperation operation = new DesignationPlacementOperation(interactor, cell);
        stack.Add(operation);
    }

    public abstract class UndoableOperation
    {
        public abstract void Undo();
    }

    public class DesignationPlacementOperation : UndoableOperation
    {
        private readonly InteractionManager interactor;
        private readonly VoxelDesignationType oldContents;
        private readonly DesignationCell cell;

        public DesignationPlacementOperation(
            InteractionManager interactor,
            DesignationCell cell)
        {
            this.interactor = interactor;
            this.cell = cell;
            this.oldContents = cell.Designation;
        }

        public override void Undo()
        {
            interactor.SetDesignation(cell, oldContents);
        }
    }
}