using System;
using System.Collections.Generic;
using System.Linq;
using g3;

namespace gs.generators
{
    public class InfillRegionGenerator
    {
        public int FloorLayers { get; set; } = 2;
        public int RoofLayers { get; set; } = 2;
        public double MinimumArea { get; set; } = 0;
        public double InfillInsetDistanceMM { get; set; } = 1;

        protected virtual IEnumerable<int> FloorLayerIndices(int layerIndex, int layerCount)
        {
            for (var floorIndex = layerIndex - FloorLayers; floorIndex < layerIndex; ++floorIndex)
                if (floorIndex >= 0)
                    yield return floorIndex;
        }

        protected virtual IEnumerable<int> RoofLayerIndices(int layerIndex, int layerCount)
        {
            for (var roofIndex = layerIndex + 1; roofIndex <= layerIndex + RoofLayers; ++roofIndex)
                if (roofIndex < layerCount - 1)
                    yield return roofIndex;
        }

        /// <summary>
        ///     Compute the interior regions for infill for every layer
        /// </summary>
        public List<GeneralPolygon2d>[] ComputeInteriorRegions(List<PlanarSlice> slices, Interval1i layerSolveInterval,
            Func<bool> cancelled, Action countProgressStep)
        {
            var layerInfillRegions = new List<GeneralPolygon2d>[slices.Count];

            gParallel.ForEach(layerSolveInterval, layerIndex =>
            {
                if (cancelled()) return;
                layerInfillRegions[layerIndex] = new List<GeneralPolygon2d>();

                if (LayerCouldHaveInfill(layerIndex, slices.Count))
                    layerInfillRegions[layerIndex].AddRange(FindInteriorsForLayer(slices, layerIndex));
                countProgressStep();
            });
            return layerInfillRegions;
        }

        /// <summary>
        ///     Compute the interior regions for a layer
        /// </summary>
        /// <remarks>
        ///     The interior is computed by taking the intersection of the neighboring N slices above and below
        ///     the current layer. Any area that is contained on all the subset of layers around the current
        ///     layer does not need to be solid. This area is then inset by a small distance to provide an anchor
        ///     for solid fill.
        /// </remarks>
        private List<GeneralPolygon2d> FindInteriorsForLayer(List<PlanarSlice> slices, int layerIndex)
        {
            var interiorRegions = slices[layerIndex].Solids;

            foreach (var i in FloorLayerIndices(layerIndex, slices.Count)
                .Concat(RoofLayerIndices(layerIndex, slices.Count)))
                interiorRegions = ClipperUtil.Intersection(interiorRegions, slices[i].Solids, MinimumArea);

            return ClipperUtil.MiterOffset(interiorRegions, -InfillInsetDistanceMM, MinimumArea);
        }

        private bool LayerCouldHaveInfill(int layerIndex, int layerCount)
        {
            return layerIndex >= FloorLayers &&
                   layerIndex <= layerCount - RoofLayers;
        }
    }
}