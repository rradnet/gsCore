using System.Collections.Generic;

namespace gs
{
    public class GCodeFile
    {
        List<GCodeLine> lines;

        public GCodeLine this[int i]
        {
            get
            {
                return lines[i];
            }
        }

        public GCodeFile()
        {
            lines = new List<GCodeLine>();
        }

        public void AppendLine(GCodeLine l)
        {
            lines.Add(l);
        }
        
        public int LineCount {
            get { return lines.Count; }
        }

        public IEnumerable<GCodeLine> AllLines()
        {
            int N = lines.Count;
            for (int i = 0; i < N; ++i) {
                yield return lines[i];
            }
        }

        public IEnumerable<GCodeLine> AllLinesOfType(GCodeLine.LType eType)
        {
            int N = lines.Count;
            for (int i = 0; i < N; ++i) {
                if ( lines[i].type == eType )
                    yield return lines[i];
            }
        }
    }
}
