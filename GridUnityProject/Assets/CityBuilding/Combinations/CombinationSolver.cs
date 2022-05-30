using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CombinationSolver
{
    private readonly ReadOnlyCollection<VoxelDesignation> designations;
    private readonly ReadOnlyCollection<VoxelVisualDesignation> allUniqueCombinations;

    public CombinationSolver()
    {
        designations = GetDesignations().ToList().AsReadOnly();
        allUniqueCombinations = GetAllUniqueCombinations();
    }

    private IEnumerable<VoxelDesignation> GetDesignations()
    {
        yield return new EmptyDesignation();
        yield return new ShellDesignation();
        yield return new AquaductDesignation();

        yield return new BuildingDesignation(DesignationBuildingRoof.Grass, DesignationBuildingWall.Cornered);
        yield return new BuildingDesignation(DesignationBuildingRoof.Grass, DesignationBuildingWall.Rounded);
        yield return new BuildingDesignation(DesignationBuildingRoof.Stone, DesignationBuildingWall.Cornered);
        yield return new BuildingDesignation(DesignationBuildingRoof.Stone, DesignationBuildingWall.Rounded);
        yield return new BuildingDesignation(DesignationBuildingRoof.Slanted, DesignationBuildingWall.Cornered);
        yield return new BuildingDesignation(DesignationBuildingRoof.Slanted, DesignationBuildingWall.Rounded);

        yield return new PlatformDesignation(DesignationPlatformType.Uncovered);
        yield return new PlatformDesignation(DesignationPlatformType.Grass);
        yield return new PlatformDesignation(DesignationPlatformType.Covered);
    }

    private ReadOnlyCollection<VoxelVisualDesignation> GetAllUniqueCombinations()
    {
        VoxelDesignation[,,] currentDescription = new VoxelDesignation[2, 2, 2];

        currentDescription[0, 0, 0] = designations[0];
        currentDescription[0, 0, 1] = designations[0];
        currentDescription[0, 1, 0] = designations[0];
        currentDescription[0, 1, 1] = designations[0];
        currentDescription[1, 0, 0] = designations[0];
        currentDescription[1, 0, 1] = designations[0];
        currentDescription[1, 1, 0] = designations[0];
        currentDescription[1, 1, 1] = designations[0];

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {

                }
            }
        }
    }
}