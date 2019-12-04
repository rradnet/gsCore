using System;

namespace gs
{

    public class SingleMaterialFFFPrintGenerator : ThreeAxisPrintGenerator
    {
        GCodeFileAccumulator file_accumulator;
        GCodeBuilder builder;
        SingleMaterialFFFCompiler compiler;

        public SingleMaterialFFFPrintGenerator() { }

        public SingleMaterialFFFPrintGenerator(PrintMeshAssembly meshes,
                                               PlanarSliceStack slices,
                                               SingleMaterialFFFSettings settings,
                                               AssemblerFactoryF overrideAssemblerF = null)
        {
            Initialize(meshes, slices, settings, overrideAssemblerF);
        }

        public override void Initialize(PrintMeshAssembly meshes,
                               PlanarSliceStack slices,
                               SingleMaterialFFFSettings settings,
                               AssemblerFactoryF overrideAssemblerF = null)
        {
            file_accumulator = new GCodeFileAccumulator();
            builder = new GCodeBuilder(file_accumulator);
            AssemblerFactoryF useAssembler = overrideAssemblerF ?? settings.AssemblerType();
            compiler = new SingleMaterialFFFCompiler(builder, settings, useAssembler);
            Initialize(meshes, slices, settings, compiler);
        }

        protected override GCodeFile extract_result()
        {
            return file_accumulator.File;
        }


    }


}
