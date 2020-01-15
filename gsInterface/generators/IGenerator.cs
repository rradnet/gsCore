using g3;
using System;
using System.Collections.Generic;
using System.IO;

namespace gs
{
    public interface IGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="globalSettings"></param>
        /// <param name="gcodeLineReadyF"></param>
        /// <param name="layerReadyF"></param>
        /// <returns></returns>
        GCodeFile GenerateGCode(IList<Tuple<DMesh3, object>> parts,
                                object globalSettings,
                                out IEnumerable<string> generationReport,
                                Action<GCodeLine> gcodeLineReadyF = null,
                                Action<string> progressMessageF = null);

        void SaveGCode(TextWriter output, GCodeFile file);

        GCodeFile LoadGCode(TextReader input);

        bool AcceptsParts { get; }
        bool AcceptsPartSettings { get; }

        Version Version { get; }
    }

    public interface IGenerator<TSettings> : IGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="globalSettings"></param>
        /// <param name="gcodeLineReadyF"></param>
        /// <param name="layerReadyF"></param>
        /// <returns></returns>
        GCodeFile GenerateGCode(IList<Tuple<DMesh3, TSettings>> parts,
                                TSettings globalSettings,
                                out IEnumerable<string> generationReport,
                                Action<GCodeLine> gcodeLineReadyF = null,
                                Action<string> progressMessageF = null);
    }
}
