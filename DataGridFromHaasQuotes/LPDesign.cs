using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridFromHaasQuotes
{
    static class LPDesign
    {
        public static int TFClgNozzleQty(double Spacing, double DistFrWall, double Length, double Width)
        {
            int NumRows = (int)Math.Ceiling((Width - 2 * DistFrWall) / Spacing) + 1;
            int NozzlesPerRow = (int)Math.Ceiling((Length - 2 * DistFrWall) / Spacing) + 1;
            return NumRows * NozzlesPerRow;
        }
        public static int TFSidewallNozzleQty(double Spacing, double DistFrWall, double Length)
        {
            int NozzlesPerRow = (int)Math.Ceiling((Length - 2 * DistFrWall) / Spacing) + 1;
            return 2 * NozzlesPerRow;
        }
        public static int LALFNozzleQty(double CoverageAreaPerNozzle, double Length, double Width)
        {
            double WidthUsed = Math.Max(Width, Math.Sqrt(CoverageAreaPerNozzle));
            int NumRows = (int)Math.Ceiling(WidthUsed / Math.Sqrt(CoverageAreaPerNozzle));
            double Afactor = Length * WidthUsed / CoverageAreaPerNozzle;
            int NozzlesPerRow = (int)Math.Ceiling(Afactor / NumRows);
            return NumRows * NozzlesPerRow;
        }
        public static int LASWNozzleQty(double Spacing, double Length)
        {
            int NozzlesPerRow = (int)Math.Ceiling(Length / Spacing);
            return 2 * NozzlesPerRow;
        }
        public static int LAPTNozzleQty(double ProcessFlowThreshold, double ProcessFlowActual)
        {
            if (ProcessFlowActual <= ProcessFlowThreshold)
            {
                return 2;
            }
            else
            {
                return 4;
            }
        }

        public static double BaseAirScf(int Qty, double Scfm, double DischDuration)
        {
            return Qty * Scfm * DischDuration;
        }
        public static double BaseWaterGal(int Qty, double Gpm, double DischDuration)
        {
            return Qty * Gpm * DischDuration;
        }

        public static int CylinderQty(double ScfReqd, double ScfPerCyl)
        {
            return (int)Math.Ceiling(ScfReqd / ScfPerCyl);
        }
    }
}
