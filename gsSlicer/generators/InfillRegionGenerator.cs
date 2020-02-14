using System;
using System.Collections.Generic;
using System.Text;
using g3;

namespace gs.generators
{
    public class InfillRegionGenerator
    {
        private List<GeneralPolygon2d>[] LayerRoofAreas;
        private List<GeneralPolygon2d>[] LayerFloorAreas;

        /// <summary>
        /// return the set of roof polygons for a layer
        /// </summary>
        public virtual List<GeneralPolygon2d> get_layer_roof_area(int layer_i)
        {
            return LayerRoofAreas[layer_i];
        }

        /// <summary>
        /// return the set of floor polygons for a layer
        /// </summary>
        public virtual List<GeneralPolygon2d> get_layer_floor_area(int layer_i)
        {
            return LayerFloorAreas[layer_i];
        }

        /// <summary>
        /// construct region that needs to be solid for "roofs".
        /// This is the intersection of infill polygons for the next N layers.
        /// </summary>
        protected virtual List<GeneralPolygon2d> find_roof_areas_for_layer(int layer_i, List<PlanarSlice> slices, SingleMaterialFFFSettings Settings)
        {
            double min_area = Settings.Machine.NozzleDiamMM * Settings.Machine.NozzleDiamMM;

            List<GeneralPolygon2d> roof_cover = new List<GeneralPolygon2d>(slices[layer_i + 1].Solids);

            // If we want > 1 roof layer, we need to look further ahead.
            // The full area we need to print as "roof" is the infill minus
            // the intersection of the infill areas above
            for (int k = 2; k <= Settings.RoofLayers; ++k)
            {
                int ri = layer_i + k;
                if (ri < slices.Count)
                {
                    List<GeneralPolygon2d> infillN = new List<GeneralPolygon2d>(slices[ri].Solids);
                    roof_cover = ClipperUtil.Intersection(roof_cover, infillN, min_area);
                }
            }

            // add overhang allowance. Technically any non-vertical surface will result in
            // non-empty roof regions. However we do not need to explicitly support roofs
            // until they are "too horizontal". 
            var result = ClipperUtil.MiterOffset(roof_cover, OverhangAllowanceMM(Settings), min_area);
            return result;
        }

        /// <summary>
        /// construct region that needs to be solid for "floors"
        /// </summary>
        protected virtual List<GeneralPolygon2d> find_floor_areas_for_layer(int layer_i, List<PlanarSlice> slices,  SingleMaterialFFFSettings Settings)
        {
            double min_area = Settings.Machine.NozzleDiamMM * Settings.Machine.NozzleDiamMM;

            List<GeneralPolygon2d> floor_cover = new List<GeneralPolygon2d>(slices[layer_i - 1].Solids);

            // If we want > 1 floor layer, we need to look further back.
            for (int k = 2; k <= Settings.FloorLayers; ++k)
            {
                int ri = layer_i - k;
                if (ri >= 0)
                {
                    List<GeneralPolygon2d> infillN = new List<GeneralPolygon2d>(slices[ri].Solids);
                    floor_cover = ClipperUtil.Intersection(floor_cover, infillN, min_area);
                }
            }

            // add overhang allowance. 
            var result = ClipperUtil.MiterOffset(floor_cover, OverhangAllowanceMM(Settings), min_area);
            return result;
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
        public void precompute_roofs_floors(PlanarSliceStack SliceStack, SingleMaterialFFFSettings Settings, Func<bool> Cancelled, Action countProgressStep)
        {
            int nLayers = SliceStack.Count;
            LayerRoofAreas = new List<GeneralPolygon2d>[nLayers];
            LayerFloorAreas = new List<GeneralPolygon2d>[nLayers];

            int start_layer = Math.Max(0, Settings.LayerRangeFilter.a);
            int end_layer = Math.Min(nLayers - 1, Settings.LayerRangeFilter.b);
            Interval1i solve_roofs_floors = new Interval1i(start_layer, end_layer);
            gParallel.ForEach(solve_roofs_floors, (layer_i) => {
                if (Cancelled()) return;
                bool is_infill = (layer_i >= Settings.FloorLayers && layer_i < nLayers - Settings.RoofLayers);

                if (is_infill)
                {
                    if (Settings.RoofLayers > 0)
                    {
                        LayerRoofAreas[layer_i] = find_roof_areas_for_layer(layer_i, SliceStack.Slices, Settings);
                    }
                    else
                    {
                        LayerRoofAreas[layer_i] = find_roof_areas_for_layer(layer_i - 1, SliceStack.Slices, Settings);     // will return "our" layer
                    }
                    if (Settings.FloorLayers > 0)
                    {
                        LayerFloorAreas[layer_i] = find_floor_areas_for_layer(layer_i, SliceStack.Slices, Settings);
                    }
                    else
                    {
                        LayerFloorAreas[layer_i] = find_floor_areas_for_layer(layer_i + 1, SliceStack.Slices, Settings);   // will return "our" layer
                    }
                }
                else
                {
                    LayerRoofAreas[layer_i] = new List<GeneralPolygon2d>();
                    LayerFloorAreas[layer_i] = new List<GeneralPolygon2d>();
                }

                countProgressStep();
            });
        }
    }
}
