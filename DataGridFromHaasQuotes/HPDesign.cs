using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataGridFromHaasQuotes
{
    class HPDesign
    {
        public static int TFClgNozzleQty(double Spacing, double DistFrWall, double Length, double Width)
        {
            /* General comment added to test sync with git repo */
            int NumRows = (int)Math.Ceiling((Width - 2 * DistFrWall) / Spacing) + 1;
            int NozzlesPerRow = (int)Math.Ceiling((Length - 2 * DistFrWall) / Spacing) + 1;
            return NumRows * NozzlesPerRow;
        }

        public static int TFSWNozzleQty(double Length, double DistFrWall, double Spacing)
        {
            //only reqd for FM design when V > 500 m3 (17657 ft3) or ht > 5 m (16.4 ft)
            int NumRows = 2;
            int NozzlesPerRow = (int)Math.Ceiling((Length - 2 * DistFrWall) / Spacing) + 1;
            return NumRows * NozzlesPerRow;
        }

        public static int LALFNozzleQty(double Length, double Width, double Height, double Layout1MinHt, double Layout1MaxHt, 
            double Layout2MinHt, double Layout2MaxHt, double SingleRowMaxWidth, double MaxNozzleSpacing)
        {
            int NumRows = 0;
            int NozzlesPerRow = 0;
            if (Width <= SingleRowMaxWidth)
            {
                NumRows = 1;
            }

            if (Length <= SingleRowMaxWidth)
            {
                NozzlesPerRow = 1;
            }

            if (Height < Layout1MinHt)
            {
                String msg1 = String.Format("Height entered of {0} ft is < minimum approved ht of {1} ft.", Height, Layout1MinHt);
                String msg2 = String.Format("Nozzle layout and qty returned will be based on ht of {0} ft.", Layout1MinHt);
                MessageBox.Show(msg1 + "\n\n" + msg2);

                if (NumRows == 0)
                {
                    NumRows = (int)Math.Ceiling((Width/MaxNozzleSpacing) + 1);
                }

                if (NozzlesPerRow == 0)
                {
                    NozzlesPerRow = (int)Math.Ceiling((Length / MaxNozzleSpacing) + 1);
                }

                return NumRows * NozzlesPerRow;
            }
            else if (Height >= Layout1MinHt & Height <= Layout1MaxHt)
            {
                if (NumRows == 0)
                {
                    NumRows = (int)Math.Ceiling((Width / MaxNozzleSpacing) + 1);
                }

                if (NozzlesPerRow == 0)
                {
                    NozzlesPerRow = (int)Math.Ceiling((Length / MaxNozzleSpacing) + 1);
                }

                return NumRows * NozzlesPerRow;
            }
            else if (Height > Layout1MaxHt & Height < Layout2MinHt)
            {
                //ht entered is in no man's land
                String msg1 = String.Format("Height entered of {0} ft is > maximum approved ht of {1} ft for Layout1 \n" +
                    "and < minimum approved ht of {2} for Layout2.", Height, Layout1MaxHt, Layout2MinHt);
                String msg2 = "Enter ht which is within one of the approved ranges.";
                MessageBox.Show(msg1 + "\n\n" + msg2);

                return 0;
            }
            else if (Height >= Layout2MinHt & Height <= Layout2MaxHt)
            {
                if (NumRows == 0)
                {
                    NumRows = (int)Math.Ceiling((Width / MaxNozzleSpacing) + 1.5);
                }

                if (NozzlesPerRow == 0)
                {
                    NozzlesPerRow = (int)Math.Ceiling((Length / MaxNozzleSpacing) + 1.5);
                }

                return NumRows * NozzlesPerRow;
            }
            else
            {
                //ht is > Layout2MaxHt
                String msg1 = String.Format("Height entered of {0} ft is > maximum approved ht of {1} ft.", Height, Layout2MaxHt);
                String msg2 = String.Format("Nozzle layout and qty returned will be based on ht of {0} ft.", Layout2MaxHt);
                MessageBox.Show(msg1 + "\n\n" + msg2);
                if (NumRows == 0)
                {
                    NumRows = (int)Math.Ceiling((Width / MaxNozzleSpacing) + 1.5);
                }

                if (NozzlesPerRow == 0)
                {
                    NozzlesPerRow = (int)Math.Ceiling((Length / MaxNozzleSpacing) + 1.5);
                }

                return NumRows * NozzlesPerRow;
            }
        }
    }
}
