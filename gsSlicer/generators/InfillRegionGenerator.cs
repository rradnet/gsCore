using g3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gs.generators
{
    public class InfillRegionGenerator
    {
        protected virtual IEnumerable<int> FloorLayerIndices(int layerIndex, int layerCount, SingleMaterialFFFSettings Settings)
        {
            for (int floorIndex = layerIndex - Settings.FloorLayers; floorIndex < layerIndex; ++floorIndex)
            {
                if (floorIndex >= 0)
                {
                    yield return floorIndex;
                }
            }
        }

        protected virtual IEnumerable<int> RoofLayerIndices(int layerIndex, int layerCount, SingleMaterialFFFSettings Settings)
        {
            for (int roofIndex = layerIndex + 1; roofIndex <= layerIndex + Settings.RoofLayers; ++roofIndex)
            {
                if (roofIndex < layerCount - 1)
                {
                    yield return roofIndex;
                }
            }
        }

        protected static double OverhangAllowanceMM(SingleMaterialFFFSettings Settings)
        {
            // should be parameterizable? this is 45 degrees...  (is it? 45 if nozzlediam == layerheight...)
            //double fOverhangAllowance = 0.5 * settings.NozzleDiamMM;
            return Settings.LayerHeightMM / Math.Tan(45 * MathUtil.Deg2Rad) - (Settings.Shells + 0.5) * Settings.Machine.NozzleDiamMM;
        }

        /// <summary>
        /// compute all the roof and floor areas for the entire stack, in parallel
        /// </summary>
        /// 
        public List<GeneralPolygon2d>[] ComputeInteriorRegions(PlanarSliceStack SliceStack, SingleMaterialFFFSettings Settings, Func<bool> Cancelled, Action countProgressStep)
        {
            int nLayers = SliceStack.Count;
            var layerInfillRegions = new List<GeneralPolygon2d>[nLayers];

            int start_layer = Math.Max(0, Settings.LayerRangeFilter.a);
            int end_layer = Math.Min(nLayers - 1, Settings.LayerRangeFilter.b);
            Interval1i solve_roofs_floors = new Interval1i(start_layer, end_layer);

            gParallel.ForEach(solve_roofs_floors, (layerIndex) => {
                if (Cancelled()) return;
                layerInfillRegions[layerIndex] = new List<GeneralPolygon2d>();

                if (LayerCouldHaveInfill(Settings, layerIndex, nLayers))
                {
                    layerInfillRegions[layerIndex].AddRange(FindInteriorsForLayer(SliceStack.Slices, Settings, layerIndex));

                }
                countProgressStep();
            });
            return layerInfillRegions;
        }

        private List<GeneralPolygon2d> FindInteriorsForLayer(List<PlanarSlice> slices, SingleMaterialFFFSettings settings, int layerIndex)
        {
            var interiorRegions = slices[layerIndex].Solids;

            foreach (var i in Enumerable.Concat(
                FloorLayerIndices(layerIndex, slices.Count, settings),
                RoofLayerIndices(layerIndex, slices.Count, settings)))
            {
                interiorRegions = ClipperUtil.Intersection(interiorRegions, slices[i].Solids, MinimumArea(settings));
            }

            return ClipperUtil.MiterOffset(interiorRegions, OverhangAllowanceMM(settings), MinimumArea(settings));
        }

        private static bool LayerCouldHaveInfill(SingleMaterialFFFSettings Settings, int layerIndex, int layerCount)
        {
            return layerIndex >= Settings.FloorLayers && 
                   layerIndex <= layerCount - Settings.RoofLayers;
        }

        private static double MinimumArea(SingleMaterialFFFSettings settings)
        {
            return Math.Pow(settings.Machine.NozzleDiamMM, 2);
        }
    }
}